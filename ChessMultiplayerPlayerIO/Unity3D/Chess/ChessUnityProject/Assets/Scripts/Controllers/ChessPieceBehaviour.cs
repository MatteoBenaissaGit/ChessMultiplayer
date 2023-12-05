using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public abstract class ChessPieceBehaviour
    {
        protected int Team;
        protected int Sign;
        public ChessPieceBehaviour(int team)
        {
            Team = team;
            Sign = Team == 0 ? 1 : -1;
        }
        public abstract List<Vector2Int> GetPossibleTilesFromCoordinates(Vector2Int coordinates);
        public abstract List<Vector2Int> GetPossibleTilesTakeFromCoordinates(Vector2Int coordinates);
        public abstract void HasMoved();
    }

    public class PawnBehaviour : ChessPieceBehaviour
    {
        private bool _firstMove;
        private int _moveAmount;

        public PawnBehaviour(int team) : base(team)
        {
            _firstMove = true;
        }
        
        public override List<Vector2Int> GetPossibleTilesFromCoordinates(Vector2Int coordinates)
        {
            List<Vector2Int> possibleTiles = new List<Vector2Int>();
            
            possibleTiles.Add(coordinates + new Vector2Int(0,1 * Sign));
            if (_firstMove)
            {
                possibleTiles.Add(coordinates + new Vector2Int(0,2 * Sign));
            }
            
            return possibleTiles;
        }

        public override List<Vector2Int> GetPossibleTilesTakeFromCoordinates(Vector2Int coordinates)
        {
            List<Vector2Int> possibleTiles = new List<Vector2Int>();
            
            possibleTiles.Add(coordinates + new Vector2Int(1, 1 * Sign));
            possibleTiles.Add(coordinates + new Vector2Int(-1, 1 * Sign));

            return possibleTiles;
        }

        public override void HasMoved()
        {
            _moveAmount++;
            _firstMove = false;
        }
    }
}