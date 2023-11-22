using System;
using Controllers;
using UnityEngine;

namespace Views
{
    public class ChessPieceView : MonoBehaviour
    { 
        [SerializeField] private GameObject _whitePiece;
        [SerializeField] private GameObject _darkPiece;

        public ChessPieceController Controller { get; private set; }

        public void SetPawnView(ChessPieceController controller)
        {
            Controller = controller;
            
            _whitePiece.SetActive(Controller.Data.Team == 0);
            _darkPiece.SetActive(Controller.Data.Team == 1);
        }

        public void MoveTo(Vector2Int coordinates)
        {
            transform.position = ChessBoard.GetWorldPositionFromCoordinates(coordinates);
        }
    }
}