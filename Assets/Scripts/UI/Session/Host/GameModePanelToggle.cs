using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Session
{
    public class GameModePanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _blockPanel;
        [SerializeField] private GameObject _checkIcon;
        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            _blockPanel.SetActive(!value);
            _toggle.interactable = !value;
            _checkIcon.SetActive(value);
        }
    }
}
