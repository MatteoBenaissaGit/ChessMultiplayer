using System;
using UnityEngine;

namespace Controllers
{
    public class BoardInputController : MonoBehaviour
    {
        [SerializeField] private ChessBoard _board;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) == false)
            {
                return;
            }
            
            Ray ray = PlayerGameManager.Instance.Cameras[PlayerGameManager.Instance.Team].ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject.TryGetComponent(out BoardCaseController boardCase))
                {
                    Debug.Log("hit case");
                    _board.SelectCase(boardCase.Coordinates);
                }
            }
        }
    }
}