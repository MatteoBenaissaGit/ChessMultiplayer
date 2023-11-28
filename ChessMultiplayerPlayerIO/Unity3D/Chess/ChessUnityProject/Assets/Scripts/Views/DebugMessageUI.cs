using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Views
{
    public class DebugMessageUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        private float _timerDeath = 5f;
        private bool _isDestroying;

        public void Set(string message)
        {
            _text.text = message;
        }
        
        private void Update()
        {
            if (_isDestroying)
            {
                return;
            }
            
            _timerDeath -= Time.deltaTime;
            if (_timerDeath <= 0)
            {
                _text.DOFade(0, 2f).OnComplete(() => Destroy(gameObject));
                _isDestroying = true;
            }
        }
    }
}