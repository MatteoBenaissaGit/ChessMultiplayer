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
            _turnText.text = "Turn " + PlayerGameManager.Instance.Turn;
        }

        public void DebugMessage(string message)
        {
            DebugMessageUI debug = Instantiate(_debugMessagePrefab, _debugMessageLayout);
            debug.Set(message);
        }
    }
}