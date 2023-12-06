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
        public abstract List<Vector2Int> GetPossibleTileMoveFromCoordinates(Vector2Int coordinates);
        public abstract List<Vector2Int> GetPossibleTilesTakeFromCoordinates(Vector2Int coordinates);
        public abstract List<Vector2Int> GetImpossibleTilesTakeFromCoordinates(Vector2Int coordinates);
        public abstract List<Vector2Int> GetImpossibleTileMoveFromCoordinates(Vector2Int coordinates);
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
        
        public override List<Vector2Int> GetPossibleTileMoveFromCoordinates(Vector2Int coordinates)
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

        public override List<Vector2Int> GetImpossibleTilesTakeFromCoordinates(Vector2Int coordinates)
        {
            return GetPossibleTileMoveFromCoordinates(coordinates);
        }

        public override List<Vector2Int> GetImpossibleTileMoveFromCoordinates(Vector2Int coordinates)
        {
            return GetPossibleTilesTakeFromCoordinates(coordinates);
        }

        public override void HasMoved()
        {
            _moveAmount++;
            _firstMove = false;
        }
    }

    public class BishopBehaviour : ChessPieceBehaviour
    {
        public Vector2Int[] DiagonalDirections = new Vector2Int[]
        {
            new Vector2Int(1,1),
            new Vector2Int(-1,-1),
            new Vector2Int(1,-1),
            new Vector2Int(-1,1),
        };
        
        public BishopBehaviour(int team) : base(team)
        {
        }

        public override List<Vector2Int> GetPossibleTileMoveFromCoordinates(Vector2Int coordinates)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();

            const int BoardSize = 7;
            foreach (Vector2Int direction in DiagonalDirections)
            {
                for (int i = 1; i <= BoardSize; i++)
                {
                    Vector2Int newCoordinate = coordinates + direction * i;
                    if (PlayerGameManager.Instance.Board.IsCoordinateOutOfBoard(newCoordinate))
                    {
                        break;
                    }
                    
                    tiles.Add(newCoordinate);
                    
                    if (PlayerGameManager.Instance.Board.BoardArray[newCoordinate.x, newCoordinate.y] != null)
                    {
                        break;
                    }
                }
            }

            return tiles;
        }

        public override List<Vector2Int> GetPossibleTilesTakeFromCoordinates(Vector2Int coordinates)
        {
            return GetPossibleTileMoveFromCoordinates(coordinates);
        }

        public override List<Vector2Int> GetImpossibleTilesTakeFromCoordinates(Vector2Int coordinates)
        {
            return new List<Vector2Int>();
        }

        public override List<Vector2Int> GetImpossibleTileMoveFromCoordinates(Vector2Int coordinates)
        {
            return new List<Vector2Int>();
        }

        public override void HasMoved()
        {
        }
    }
}