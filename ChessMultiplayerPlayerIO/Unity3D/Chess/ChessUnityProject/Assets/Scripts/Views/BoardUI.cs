using TMPro;
using UnityEngine;

namespace Views
{
    public class BoardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private DebugMessageUI _debugMessagePrefab;
        [SerializeField] private Transform _debugMessageLayout;

        public void SetTurnUI()
        {
            bool whiteTurn = PlayerGameManager.Instance.Turn % 2 == 0;
            string text = whiteTurn ? "Turn white" : "Turn black";
            Color color = whiteTurn ? Color.white : Color.black;
            _turnText.text = text;
            _turnText.color = color;
        }

        public void DebugMessage(string message)
        {
            DebugMessageUI debug = Instantiate(_debugMessagePrefab, _debugMessageLayout);
            debug.Set(message);
        }
    }
}