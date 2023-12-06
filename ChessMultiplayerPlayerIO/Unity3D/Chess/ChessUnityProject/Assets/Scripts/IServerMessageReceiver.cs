using PlayerIOClient;
using UnityEngine;

public abstract class IServerMessageReceiver
{
    public abstract void Receive(Message m);
}

public class ReceiveSetTurn : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        int turn = m.GetInt(0);
        PlayerGameManager.Instance.SetTurn(turn);
    }
}

public class ReceiveSendGameInfosToServer : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        PlayerGameManager.Instance.UI.DebugMessage("send game infos from server");
        PlayerGameManager.Instance.PlayerIoConnection.Send("SendGameInfosToPlayers",
            PlayerGameManager.Instance.Turn);
    }
}

public class ReceiveGetGameInfosFromServer : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        PlayerGameManager.Instance.UI.DebugMessage("get game infos from server");
        int turnInfo = m.GetInt(0);
        PlayerGameManager.Instance.SetTurn(turnInfo);
    }
}

public class ReceiveGetPieceFromServer : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        string getPieceType = m.GetString(0);
        string getPieceId = m.GetString(1);
        string getPieceOwnerId = m.GetString(2);
        Vector2Int getPieceCreateCoordinates = new Vector2Int(m.GetInt(3), m.GetInt(4));
        int getPieceTeam = m.GetInt(5);
        PlayerGameManager.Instance.UI.DebugMessage("get piece from server");
        PlayerGameManager.Instance.Board.GetPieceDataFromServer(getPieceType, getPieceId, getPieceOwnerId,
            getPieceCreateCoordinates, getPieceTeam);
    }
}

public class ReceiveSendPiecesToServer : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        PlayerGameManager.Instance.UI.DebugMessage("send pieces data to server");
        PlayerGameManager.Instance.Board.SendAllPiecesDataToServer();
    }
}

public class ReceiveDestroyPiece : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        string destroyPieceId = m.GetString(0);
        PlayerGameManager.Instance.UI.DebugMessage($"Destroy piece {destroyPieceId}");
        PlayerGameManager.Instance.Board.DestroyPiece(destroyPieceId);
    }
}

public class ReceiveMovePiece : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        string pieceId = m.GetString(0);
        Vector2Int moveCoordinates = new Vector2Int(m.GetInt(1), m.GetInt(2));
        PlayerGameManager.Instance.UI.DebugMessage(
            $"move piece {pieceId} to {moveCoordinates.x},{moveCoordinates.y}");
        PlayerGameManager.Instance.Board.MovePiece(pieceId, moveCoordinates);
    }
}

public class ReceiveCreatePiece : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        string createPieceType = m.GetString(0);
        string createPieceId = m.GetString(1);
        string createPieceOwnerId = m.GetString(2);
        Vector2Int createPieceCreateCoordinates = new Vector2Int(m.GetInt(3), m.GetInt(4));
        int createPieceTeam = m.GetInt(5);
        PlayerGameManager.Instance.UI.DebugMessage(
            $"create piece {createPieceType}_{createPieceId} for {createPieceOwnerId}, at {createPieceCreateCoordinates.x},{createPieceCreateCoordinates.y}");
        PlayerGameManager.Instance.Board.CreatePieceAt(createPieceType, createPieceId, createPieceOwnerId,
            createPieceCreateCoordinates,
            createPieceTeam);
    }
}

public class ReceiveSetTeam : IServerMessageReceiver
{
    public override void Receive(Message m)
    {
        PlayerGameManager.Instance.Team = m.GetInt(0);
        PlayerGameManager.Instance.Cameras[0].gameObject.SetActive(PlayerGameManager.Instance.Team == 0);
        PlayerGameManager.Instance.Cameras[1].gameObject.SetActive(PlayerGameManager.Instance.Team == 1);
        PlayerGameManager.Instance.Board.SetBoard();
    }
}