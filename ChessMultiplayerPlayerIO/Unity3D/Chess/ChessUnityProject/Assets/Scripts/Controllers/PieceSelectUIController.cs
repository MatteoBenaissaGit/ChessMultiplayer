using UnityEngine;

namespace Controllers
{
    public class PieceSelectUIController : MonoBehaviour
    {
        private const float HeightOffset = 0.1f; 
        
        public void Set(Vector2Int coordinates)
        {
            Debug.Log("set active");
            gameObject.SetActive(true);
            Vector3 worldPosition = ChessBoard.GetWorldPositionFromCoordinates(coordinates);
            transform.position = new Vector3(worldPosition.x, HeightOffset, worldPosition.z);
        }

        public void Set(bool show)
        {
            Debug.Log("set active false");
            gameObject.SetActive(false);
        }
    }
}
