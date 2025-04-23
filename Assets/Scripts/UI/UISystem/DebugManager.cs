using System;
using UnityEngine;

namespace UI.UISystem
{
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance;

        private void Awake()
        {
            if(Instance == null)    
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private MassageUIManager _massageUIManager;

        private int _loadingCount = 0;

        public void ShowLoading()
        {
            _loadingCount++;
            _loadingPanel.SetActive(true);
        }

        public void HideLoading()
        {
            _loadingCount--;
            if(_loadingCount <= 0)
            {
                _loadingPanel.SetActive(false);
            }
        }

        public void ShowMassage(string title, string message = "", Action onOk = null, Action onClose = null)
        {
            _massageUIManager.ShowMassage(title, message, onOk, onClose);
        }
        
    }
}
