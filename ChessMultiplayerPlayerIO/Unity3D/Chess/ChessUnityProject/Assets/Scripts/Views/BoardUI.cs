using TMPro;
using UnityEngine;

namespace Views
{
    public class BoardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _turnText;

        public void SetTurnUI()
        {
            _turnText.text = "Turn " + PlayerGameManager.Instance.Turn;
        }
    }
}