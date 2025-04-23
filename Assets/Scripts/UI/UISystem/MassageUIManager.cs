using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UISystem
{
    public class MassageUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text _messageTitle;
        [SerializeField] private TMP_Text _messageContent;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _okButton;

        public void ShowMassage(string title, string message = "", Action onOk = null, Action onClose = null)
        {
            ResetListeners();

            _messageTitle.text = title;
            _messageContent.text = message;

            if(onOk != null)
            {
                _okButton.onClick.AddListener(() => onOk?.Invoke());
                _okButton.gameObject.SetActive(true);
            }
            else
                _okButton.gameObject.SetActive(false);

            if(onClose != null)
                _closeButton.onClick.AddListener(() => onClose?.Invoke());

            gameObject.SetActive(true);
        }

        private void ResetListeners()
        {
            _okButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();

            _okButton.onClick.AddListener(OkButtonBase);
            _closeButton.onClick.AddListener(CloseButtonBase);
        }

        private void CloseButtonBase()
        {
            gameObject.SetActive(false);
        }

        private void OkButtonBase()
        {
            gameObject.SetActive(false);
        }
    }
}
