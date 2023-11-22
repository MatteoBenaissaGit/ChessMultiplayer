using System;
using UnityEngine;
using Views;

namespace Controllers
{
    public enum ChessPieceType
    {
        Pawn = 0,
        King = 1,
        Queen = 2,
        Knight = 3,
        Bishop = 4,
        Rook = 5
    }

    public class ChessPieceData
    {
        public string Type { get; internal set; }
        public string ID { get; internal set; }
        public string PieceOwnerID { get; internal set; }
        public Vector2Int Coordinates { get; internal set; }
        public int Team { get; internal set; }
    }
    
    public abstract class ChessPieceController
    {
        public ChessPieceData Data { get; private set; }
        public ChessPieceView View { get; internal set; }

        public ChessPieceController(ChessPieceData data, ChessPieceView view)
        {
            Data = data;
            View = view;
        }

        public virtual void MoveTo(Vector2Int coordinates)
        {
            Data.Coordinates = coordinates;
            View.MoveTo(Data.Coordinates);
        }
    }

    public class PawnController : ChessPieceController
    {
        public PawnController(ChessPieceData data, ChessPieceView view) : base(data, view)
        {
            
        }
    }
}