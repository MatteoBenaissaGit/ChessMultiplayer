using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Views;

public class ChessBoard : MonoBehaviour
{
    public ChessPieceController[,] BoardArray { get; private set; }
    public HashSet<ChessPieceController> PiecesOnBoard { get; private set; }
    
    [Space(10),SerializeField] private ChessPieceView _pawnPrefab;
    
    private Dictionary<string,ChessPieceController> _piecesIdToController;
    
    private static float _offsetPerCaseXY = 1f;
    private static float _offsetHeight = 0f;

    private void Awake()
    {
        _piecesIdToController = new Dictionary<string, ChessPieceController>();
        BoardArray = new ChessPieceController[8, 8];
        PiecesOnBoard = new HashSet<ChessPieceController>();
    }

    public void SetBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            PlayerGameManager.Instance.PlayerIoConnection.Send("CreatePiece", "Pawn", x, 1);
        }
    }

    public void CreatePieceAt(string type, string id, string ownerID, Vector2Int coordinates, int team)
    {
        if (_piecesIdToController.ContainsKey(id))
        {
            Debug.LogError("this piece already exist");
            return;
        }
        
        Debug.Log($"Chess board creating {type}_{id} from {ownerID} at {coordinates.x},{coordinates.y}");
        
        ChessPieceView pawnView = null;
        PawnController pawnController = null;
        
        switch (type)
        {
            case "Pawn":
                pawnView = Instantiate(_pawnPrefab);
                ChessPieceData data = new ChessPieceData()
                {
                    Type = type, ID = id, PieceOwnerID = ownerID, Coordinates = GetCoordinatesTeamVerified(coordinates,team), Team = team
                };
                pawnController = new PawnController(data, pawnView);
                break;
            case "King":
                break;
            case "Queen":
                break;
            case "Knight":
                break;
            case "Bishop":
                break;
            case "Rook":
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (pawnView == null)
        {
            throw new Exception("Pawn not created correctly");
        }
        
        pawnView.SetPawnView(pawnController);
        BoardArray[coordinates.x, coordinates.y] = pawnView.Controller;
        _piecesIdToController.Add(id,pawnController);
        PiecesOnBoard.Add(pawnController);
        
        PlayerGameManager.Instance.PlayerIoConnection.Send("MovePiece", 
            BoardArray[coordinates.x,coordinates.y].Data.ID, coordinates.x,coordinates.y);
    }

    public static Vector3 GetWorldPositionFromCoordinates(Vector2Int coordinates)
    {
        return new Vector3(coordinates.x * _offsetPerCaseXY, _offsetHeight, coordinates.y * _offsetPerCaseXY);
    }

    public Vector2Int GetCoordinatesTeamVerified(Vector2Int coordinate, int team)
    {
        if (team == PlayerGameManager.Instance.Team)
        {
            return coordinate;
        }

        int boardSize = BoardArray.GetLength(0);
        return new Vector2Int(GetInvertedCoordinate(coordinate.x,boardSize), GetInvertedCoordinate(coordinate.y,boardSize));
    }

    public static int GetInvertedCoordinate(int coordinate, int boardSize)
    {
        int c = coordinate;
        int difference = c - boardSize/2;
        if (difference < 0)
        {
            c = boardSize / 2 + Mathf.Abs(difference) - 1;
        }
        else
        {
            c = boardSize / 2 - Mathf.Abs(difference) - 1;
        }

        return c;
    }

    public void SendAllPiecesDataToServer()
    {
        foreach (ChessPieceController piece in PiecesOnBoard)
        {
            PlayerGameManager.Instance.PlayerIoConnection.Send("SendPieceToPlayers", 
                piece.Data.Type, piece.Data.ID, piece.Data.PieceOwnerID, piece.Data.Coordinates.x, piece.Data.Coordinates.y, piece.Data.Team);
        }
    }

    public void GetPieceDataFromServer(string type, string id, string ownerID, Vector2Int coordinates, int team)
    {
        CreatePieceAt(type,id,ownerID,coordinates,team);
    }
    
    public void MovePiece(string pieceID, Vector2Int coordinates)
    {
        if (_piecesIdToController.ContainsKey(pieceID) == false)
        {
            throw new Exception("this piece doesn't exist");
        }

        ChessPieceController piece = _piecesIdToController[pieceID];
        BoardArray[piece.Data.Coordinates.x, piece.Data.Coordinates.y] = null;
        piece.MoveTo(GetCoordinatesTeamVerified(coordinates, piece.Data.Team));
        BoardArray[coordinates.x, coordinates.y] = null;
    }
}