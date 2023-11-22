using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Views;

public class ChessBoard : MonoBehaviour
{
    public ChessPieceController[,] BoardArray { get; private set; }
    public HashSet<ChessPieceController> PiecesOnBoard { get; private set; }
    
    private static float _offsetPerCaseXY = 1f;
    private static float _offsetHeight = 0f;
    
    [Space(10), SerializeField] private PieceSelectUIController _pieceSelectUI;
    [SerializeField] private BoardCaseController _caseControllerPrefab;
    [Space(10), SerializeField] private ChessPieceView _pawnPrefab;
    
    private Dictionary<string,ChessPieceController> _piecesIdToController;
    private ChessPieceController _currentSelectedPiece;

    private void Awake()
    {
        _piecesIdToController = new Dictionary<string, ChessPieceController>();
        BoardArray = new ChessPieceController[8, 8];
        PiecesOnBoard = new HashSet<ChessPieceController>();
        
        _pieceSelectUI.Set(false);
    }

    public void SetBoard()
    {
        for (int y = 0; y < BoardArray.GetLength(1); y++)
        {
            for (int x = 0; x < BoardArray.GetLength(0); x++)
            {
                BoardCaseController caseController = Instantiate(_caseControllerPrefab);
                caseController.Set(new Vector2Int(x,y));
            }
        }
        
        for (int x = 0; x < 8; x++)
        {
            PlayerGameManager.Instance.PlayerIoConnection.Send("CreatePiece", "Pawn", x, PlayerGameManager.Instance.Team == 0 ? 1 : 6);
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
                    Type = type, ID = id, PieceOwnerID = ownerID, Coordinates = coordinates, Team = team
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
        
        PlayerGameManager.Instance.PlayerIoConnection.Send("MovePiece", pawnController.Data.ID, coordinates.x,coordinates.y);
    }

    public static Vector3 GetWorldPositionFromCoordinates(Vector2Int coordinates)
    {
        return new Vector3(coordinates.x * _offsetPerCaseXY, _offsetHeight, coordinates.y * _offsetPerCaseXY);
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
        BoardArray[piece.Data.Coordinates.x, piece.Data.Coordinates.x] = null;
        piece.MoveTo(coordinates);
        BoardArray[coordinates.x, coordinates.y] = piece;
    }

    public void SelectCase(Vector2Int coordinates)
    {
        ChessPieceController piece = BoardArray[coordinates.x, coordinates.y];
        if (piece == null)
        {
            if (_currentSelectedPiece == null)
            {
                return;
            }
            
            PlayerGameManager.Instance.PlayerIoConnection.Send("MovePiece", 
                _currentSelectedPiece.Data.ID, coordinates.x, coordinates.y);
            _currentSelectedPiece = null;
            _pieceSelectUI.Set(false);
            return;
        }

        if (piece.Data.Team != PlayerGameManager.Instance.Team)
        {
            return;
        }
        
        _currentSelectedPiece = piece;
        _pieceSelectUI.Set(coordinates);
    }
}