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
    
    public class ChessPieceController
    {
        public ChessPieceData Data { get; private set; }
        public ChessPieceView View { get; internal set; }
        public ChessPieceBehaviour Behaviour { get; internal set; }

        public ChessPieceController(ChessPieceData data, ChessPieceView view, ChessPieceBehaviour behaviour)
        {
            Data = data;
            View = view;
            Behaviour = behaviour;
        }

        public bool CanPieceMoveTo(Vector2Int coordinates)
        {
            bool move = Behaviour.GetPossibleTileMoveFromCoordinates(Data.Coordinates).Contains(coordinates);
            bool attack = Behaviour.GetPossibleTilesTakeFromCoordinates(Data.Coordinates).Contains(coordinates);
            return (move || attack);
        }

        public void MoveTo(Vector2Int coordinates)
        {
            Data.Coordinates = coordinates;
            View.MoveTo(Data.Coordinates);
        }
    }
}