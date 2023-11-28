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
	public int Team { get; private set; }
	public int Turn { get; private set; }
	
	private const string GameId = "firsttakeonplayerio-ixhwduuevk2vbbrptwfga";
	private const string ConnectionId = "public";
	private const string RoomId = "UnityDefaultRoomID";
	private const string RoomType = "UnityBaseRoom";
	
	private List<Message> _messagesList = new List<Message>();
	private bool _joinedRoom;
	private bool _useLocalServer = false;
	private string _userId;
	
	protected override void InternalAwake()
	{
		Application.runInBackground = true;
		
		System.Random random = new System.Random();
		_userId = "Guest" + random.Next(0, 10000);
		
		PlayerIO.Authenticate(
			GameId,
			ConnectionId,
			new Dictionary<string, string> { { "userId", _userId } },
			null,
			MasterServerJoined,
			delegate (PlayerIOError error) { Debug.Log("Error connecting: " + error.ToString()); }
		);
	}

	private void Update()
	{
		ProcessMessageQueue();
	}
	
	private void MasterServerJoined(Client client)
	{
		Debug.Log("Successfully connected to Player.IO");
		
		if (_useLocalServer)
		{
			client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
			Debug.Log("Successfully created server end point");
		}

		client.Multiplayer.CreateJoinRoom(
			RoomId, 
			RoomType, 
			true,
			null,
			null,
			RoomJoined,
			delegate(PlayerIOError error) { Debug.Log("Error Joining Room: " + error.ToString()); }
		);
	}

	private void RoomJoined(Connection connection)
	{
		Debug.Log("Joined Room.");
		
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
			switch (m.Type)
			{
				case "SetTeam":
					Team = m.GetInt(0);
					Cameras[0].gameObject.SetActive(Team == 0);
					Cameras[1].gameObject.SetActive(Team == 1);
					Board.SetBoard();
					break;
				case "CreatePiece":
					string createPieceType = m.GetString(0);
					string createPieceId = m.GetString(1);
					string createPieceOwnerId = m.GetString(2);
					Vector2Int createPieceCreateCoordinates = new Vector2Int(m.GetInt(3), m.GetInt(4));
					int createPieceTeam = m.GetInt(5);
					Debug.Log($"create piece {createPieceType}_{createPieceId} for {createPieceOwnerId}, at {createPieceCreateCoordinates.x},{createPieceCreateCoordinates.y}");
					Board.CreatePieceAt(createPieceType, createPieceId, createPieceOwnerId, createPieceCreateCoordinates, createPieceTeam);
					break;
				case "MovePiece":
					string pieceId = m.GetString(0);
					Vector2Int moveCoordinates = new Vector2Int(m.GetInt(1), m.GetInt(2));
					Debug.Log($"move piece {pieceId} to {moveCoordinates.x},{moveCoordinates.y}");
					Board.MovePiece(pieceId, moveCoordinates);
					break;
				case "SendPiecesToServer":
					Debug.Log("send pieces data to server");
					Board.SendAllPiecesDataToServer();
					break;
				case "GetPieceFromServer":
					string getPieceType = m.GetString(0);
					string getPieceId = m.GetString(1);
					string getPieceOwnerId = m.GetString(2);
					Vector2Int getPieceCreateCoordinates = new Vector2Int(m.GetInt(3), m.GetInt(4));
					int getPieceTeam = m.GetInt(5);
					Debug.Log("get piece from server");
					Board.GetPieceDataFromServer(getPieceType, getPieceId, getPieceOwnerId, getPieceCreateCoordinates, getPieceTeam);
					break;
				case "GetGameInfosFromServer":
					int turnInfo = m.GetInt(0);
					SetTurn(turnInfo);
					break;
				case "SendGameInfosToServer":
					PlayerIoConnection.Send("SendGameInfosToPlayers", Turn);
					break;
				case "SetTurn":
					int turn = m.GetInt(0);
					SetTurn(turn);
					break;
			}
		}

		_messagesList.Clear();
	}

	private void OnApplicationQuit()
	{
		PlayerIoConnection?.Send("Chat", "I left !");
	}

	public void SetTurn(int turn)
	{
		Debug.Log($"turn {turn}");
		Turn = turn;
		UI.SetTurnUI();
	}
	
	public bool CanPlay()
	{
		bool isMyTurn = Turn % 2 == Team;

		return isMyTurn;
	}
}
