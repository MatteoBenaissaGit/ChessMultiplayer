using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Common;
using PlayerIOClient;
using Views;

public class PlayerGameManager : Singleton<PlayerGameManager>
{
	[field:Space(10)] [field:SerializeField] public ChessBoard Board { get; private set; }
	[field:SerializeField] public Camera[] Cameras { get; private set; }
	[field:SerializeField] public BoardUI UI { get; private set; }
	
	public Connection PlayerIoConnection { get; private set; }
	public int Team { get; set; }
	public int Turn { get; private set; }
	
	[SerializeField] private bool _useLocalServer;
	
	private const string GameId = "firsttakeonplayerio-ixhwduuevk2vbbrptwfga";
	private const string ConnectionId = "public";
	private const string RoomId = "UnityDefaultRoomID";
	private const string RoomType = "UnityBaseRoom";
	
	private List<Message> _messagesList = new List<Message>();
	private bool _joinedRoom;
	private string _userId;

	private Dictionary<string, ServerMessageReceiver> _receivedMessageToMethod = new Dictionary<string, ServerMessageReceiver>();

	protected override void InternalAwake()
	{
		_receivedMessageToMethod.Add("SetTeam", new ReceiveSetTeam());
		_receivedMessageToMethod.Add("CreatePiece", new ReceiveCreatePiece());
		_receivedMessageToMethod.Add("MovePiece", new ReceiveMovePiece());
		_receivedMessageToMethod.Add("DestroyPiece", new ReceiveDestroyPiece());
		_receivedMessageToMethod.Add("SendPiecesToServer", new ReceiveSendPiecesToServer());
		_receivedMessageToMethod.Add("GetPieceFromServer", new ReceiveGetPieceFromServer());
		_receivedMessageToMethod.Add("GetGameInfosFromServer", new ReceiveGetGameInfosFromServer());
		_receivedMessageToMethod.Add("SendGameInfosToServer", new ReceiveSendGameInfosToServer());
		_receivedMessageToMethod.Add("SetTurn", new ReceiveSetTurn());
		
		Application.runInBackground = true;
		
		System.Random random = new System.Random();
		_userId = "Guest" + random.Next(0, 10000);
		
		PlayerIO.Authenticate(
			GameId,
			ConnectionId,
			new Dictionary<string, string> { { "userId", _userId } },
			null,
			MasterServerJoined,
			delegate (PlayerIOError error) { UI.DebugMessage("Error connecting: " + error.ToString()); }
		);
	}

	private void Update()
	{
		ProcessMessageQueue();
	}
	
	private void MasterServerJoined(Client client)
	{
		UI.DebugMessage("Successfully connected to Player.IO");
		
		if (_useLocalServer)
		{
			client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
			UI.DebugMessage("Successfully created server end point");
		}

		client.Multiplayer.CreateJoinRoom(
			RoomId, 
			RoomType, 
			true,
			null,
			null,
			RoomJoined,
			delegate(PlayerIOError error) { UI.DebugMessage("Error Joining Room: " + error.ToString()); }
		);
	}

	private void RoomJoined(Connection connection)
	{
		UI.DebugMessage("Joined Room.");
		
		PlayerIoConnection = connection;
		PlayerIoConnection.OnMessage += HandleMessage;
		_joinedRoom = true;
		
		PlayerIoConnection.Send("Chat", $"I'm connected !");
	}

	private void HandleMessage(object sender, Message m) 
	{
		_messagesList.Add(m);
	}

	private void ProcessMessageQueue()
	{
		foreach (Message m in _messagesList)
		{
			if (_receivedMessageToMethod.TryGetValue(m.Type, out ServerMessageReceiver receiver) == false)
			{
				continue;
			}
			receiver.Receive(m);
		}

		_messagesList.Clear();
	}

	private void OnApplicationQuit()
	{
		PlayerIoConnection?.Send("Chat", "I left !");
	}

	public void SetTurn(int turn)
	{
		UI.DebugMessage($"set turn {turn}");
		Turn = turn;
		UI.SetTurnUI();
	}
	
	public bool CanPlay()
	{
		bool isMyTurn = Turn % 2 == Team;

		return isMyTurn;
	}
}
