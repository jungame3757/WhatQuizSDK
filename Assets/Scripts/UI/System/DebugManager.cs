using System;
using UnityEngine;

namespace UI.System{
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

        public void ShowLoading()
        {
            _loadingPanel.SetActive(true);
        }

        public void HideLoading()
        {
            _loadingPanel.SetActive(false);
        }

        public void ShowMassage(string title, string message = "", Action onOk = null, Action onClose = null)
        {
            _massageUIManager.ShowMassage(title, message, onOk, onClose);
        }
        
    }
}
