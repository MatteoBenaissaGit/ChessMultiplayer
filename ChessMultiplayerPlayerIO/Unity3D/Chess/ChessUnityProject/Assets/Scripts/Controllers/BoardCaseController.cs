using UnityEngine;

namespace Controllers
{
    public class BoardCaseController : MonoBehaviour
    {
        public Vector2Int Coordinates { get; private set; }

        public void Set(Vector2Int coordinates)
        {
            Coordinates = coordinates;
            transform.position = new Vector3(coordinates.x, 0, coordinates.y);
        }
    }
}
