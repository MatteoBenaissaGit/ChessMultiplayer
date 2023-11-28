using PlayerIO.GameLibrary;
using System;
using System.Collections.Generic;

namespace MushroomsUnity3DExample
{

	public class Player : BasePlayer
	{
	}

	[RoomType("UnityBaseRoom")]
	public class GameCode : Game<Player>
	{
		private int _piecesInGameAmount;
		private Dictionary<string, int> _userIdToTeam = new Dictionary<string, int>();

		// This method is called when an instance of your the game is created
		public override void GameStarted()
		{
			Console.WriteLine("Game is started, room : " + RoomId);
		}

		// This method is called when the last player leaves the room, and it's closed down.
		public override void GameClosed()
		{
			Console.WriteLine("No more players in room : " + RoomId + ", room closed.");
		}

		// This method is called whenever a player joins the game
		public override void UserJoined(Player playerJoining)
		{
			if (base.PlayerCount >= 3)
			{
				playerJoining.Disconnect();
				Console.WriteLine("no more player can join");
				return;
			}

			int team = base.PlayerCount - 1;
			_userIdToTeam.Add(playerJoining.ConnectUserId, team);
			playerJoining.Send("SetTeam", team);

			foreach (Player player in base.Players)
			{
				if (player.ConnectUserId != playerJoining.ConnectUserId)
				{
					player.Send("PlayerJoined", playerJoining.ConnectUserId);
					playerJoining.Send("PlayerJoined", player.ConnectUserId);

					player.Send("SendPiecesToServer");
					player.Send("SendGameInfosToServer");
				}
			}


		}

		// This method is called when a player leaves the game
		public override void UserLeft(Player player)
		{
			Broadcast("PlayerLeft", player.ConnectUserId);
			Console.WriteLine($"Player {player.ConnectUserId} left");
		}

		// This method is called when a player sends a message into the server code
		public override void GotMessage(Player playerSender, Message message)
		{
			switch (message.Type)
			{
				case "CreatePiece":
					string pieceType = message.GetString(0);
					string pieceId = _piecesInGameAmount.ToString();
					string ownerId = playerSender.ConnectUserId;
					int createX = message.GetInt(1);
					int createY = message.GetInt(2);
					int team = _userIdToTeam[ownerId];
					Console.WriteLine($"create piece from {playerSender.ConnectUserId} : {pieceType}");
					foreach (Player player in base.Players)
					{
						player.Send("CreatePiece", pieceType, pieceId, ownerId, createX, createY, team);
					}
					_piecesInGameAmount++;
					break;
				case "MovePiece":
					string pieceID = message.GetString(0);
					int moveX = message.GetInt(1);
					int moveY = message.GetInt(2);
					Console.WriteLine($"move piece from {playerSender.ConnectUserId} : {pieceID} to {moveX},{moveY}");
					foreach (Player player in base.Players)
					{
						player.Send("MovePiece", pieceID, message.GetInt(1), message.GetInt(2));
					}
					break;
				case "SendPieceToPlayers":
					string getPieceType = message.GetString(0);
					string getPieceId = message.GetString(1);
					string getPieceOwnerId = message.GetString(2);
					int getPieceX = message.GetInt(3);
					int getPieceY = message.GetInt(4);
					int getPieceTeam = message.GetInt(5);
					foreach (Player player in base.Players)
					{
						if (player.ConnectUserId == playerSender.ConnectUserId)
						{
							continue;
						}
						player.Send("GetPieceFromServer", getPieceType, getPieceId, getPieceOwnerId, getPieceX, getPieceY, getPieceTeam);
					}
					break;
				case "SendGameInfosToPlayers":
					int turnInfo = message.GetInt(0);
					foreach (Player player in base.Players)
					{
						if (player.ConnectUserId == playerSender.ConnectUserId)
						{
							continue;
						}
						player.Send("GetGameInfosFromServer", turnInfo);
					}
					break;
				case "SetTurn":
					int turn = message.GetInt(0);
					foreach (Player player in base.Players)
					{
						player.Send("SetTurn", turn);
					}
					break;
			}
		}
	}
}