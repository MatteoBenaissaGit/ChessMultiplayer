﻿using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;
using Views;
using Object = UnityEngine.Object;

public class ChessBoard : MonoBehaviour
{
    public ChessPieceController[,] BoardArray { get; private set; }
    public HashSet<ChessPieceController> PiecesOnBoard { get; private set; }
    
    private static float _offsetPerCaseXY = 1f;
    private static float _offsetHeight = 0f;
    
    [Space(10), SerializeField] private PieceSelectUIController _pieceSelectUI;
    [SerializeField] private GameObject _pieceCanMoveToUIPrefab;
    [SerializeField] private BoardCaseController _caseControllerPrefab;
    [Space(10), SerializeField] private ChessPieceView _pawnPrefab;
    
    private Dictionary<string,ChessPieceController> _piecesIdToController;
    private ChessPieceController _currentSelectedPiece;
    private List<GameObject> _pieceCanMoveToUIList = new List<GameObject>();

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
            PlayerGameManager.Instance.UI.DebugMessage("this piece already exist");
            return;
        }
        
        //PlayerGameManager.Instance.UI.DebugMessage($"Chess board creating {type}_{id} from {ownerID} at {coordinates.x},{coordinates.y}");
        
        ChessPieceView pawnView = null;
        ChessPieceController pawnController = null;

        switch (type)
        {
            case "Pawn":
                pawnView = Instantiate(_pawnPrefab);
                ChessPieceData data = new ChessPieceData()
                {
                    Type = type, ID = id, PieceOwnerID = ownerID, Coordinates = coordinates, Team = team
                };
                pawnController = new ChessPieceController(data, pawnView, new PawnBehaviour(team));
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
        BoardArray[piece.Data.Coordinates.x, piece.Data.Coordinates.y] = null;
        piece.MoveTo(coordinates);
        BoardArray[coordinates.x, coordinates.y] = piece;
    }

    public void SelectCase(Vector2Int coordinates)
    {
        ChessPieceController piece = BoardArray[coordinates.x, coordinates.y];
        
        if (piece == null)
        {
            if (_currentSelectedPiece == null 
                || PlayerGameManager.Instance.CanPlay() == false
                || _currentSelectedPiece.CanPieceMoveTo(coordinates) == false)
            {
                ClearCurrentSelectPiece();
                return;
            }

            _currentSelectedPiece.Behaviour.HasMoved();
            PlayerGameManager.Instance.PlayerIoConnection.Send("MovePiece", _currentSelectedPiece.Data.ID, coordinates.x, coordinates.y);
            PlayerGameManager.Instance.PlayerIoConnection.Send("SetTurn", PlayerGameManager.Instance.Turn + 1);

            ClearCurrentSelectPiece();

            return;
        }
        
        if (piece.Data.Team != PlayerGameManager.Instance.Team)
        {
            if (PlayerGameManager.Instance.CanPlay() == false || _currentSelectedPiece.CanPieceMoveTo(coordinates) == false)
            {
                ClearCurrentSelectPiece();
                return;
            }
            
            PlayerGameManager.Instance.PlayerIoConnection.Send("DestroyPiece", piece.Data.ID);
            PlayerGameManager.Instance.PlayerIoConnection.Send("SetTurn", PlayerGameManager.Instance.Turn + 1);
            PlayerGameManager.Instance.PlayerIoConnection.Send("MovePiece", _currentSelectedPiece.Data.ID, coordinates.x, coordinates.y);
            
            ClearCurrentSelectPiece();

            return;
        }


        ClearCurrentSelectPiece();
        _currentSelectedPiece = piece;
        _pieceSelectUI.Set(coordinates);

        List<Vector2Int> possibleMoves = _currentSelectedPiece.Behaviour.GetPossibleTilesFromCoordinates(coordinates);
        foreach (Vector2Int coordinate in possibleMoves)
        {
            Vector3 worldPosition = GetWorldPositionFromCoordinates(coordinate);
            worldPosition.y = 0.1f;
            _pieceCanMoveToUIList.Add(Instantiate(_pieceCanMoveToUIPrefab, worldPosition, Quaternion.identity));
        }
    }

    private void ClearCurrentSelectPiece()
    {
        _currentSelectedPiece = null;
        _pieceSelectUI.Set(false);
        
        if (_pieceCanMoveToUIList.Count <= 0)
        {
            return;
        }
        
        _pieceCanMoveToUIList.ForEach(Destroy);
        _pieceCanMoveToUIList.Clear();
    }

    public void DestroyPiece(string pieceID)
    {
        ChessPieceController piece = _piecesIdToController[pieceID];
        BoardArray[piece.Data.Coordinates.x, piece.Data.Coordinates.y] = null;
        PiecesOnBoard.Remove(piece);
        Object.Destroy(piece.View.gameObject);
    }
}