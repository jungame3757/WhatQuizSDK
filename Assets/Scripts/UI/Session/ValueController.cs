using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Session{
    public class ValueController : MonoBehaviour
    {
        public int CurrentValue;
        [SerializeField] private TMP_InputField _valueInputField;
        [SerializeField] private Button _increaseButton;
        [SerializeField] private Button _decreaseButton;

        [SerializeField] private int _minValue;
        [SerializeField] private int _maxValue;
        [SerializeField] private int _changeValue;

        private void Awake()
        {
            _increaseButton.onClick.AddListener(IncreaseValue);
            _decreaseButton.onClick.AddListener(DecreaseValue);
            _valueInputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void IncreaseValue()
        {
            CurrentValue = Mathf.Clamp(CurrentValue + _changeValue, _minValue, _maxValue);
            _valueInputField.text = CurrentValue.ToString();
        }

        private void DecreaseValue()
        {
            CurrentValue = Mathf.Clamp(CurrentValue - _changeValue, _minValue, _maxValue);
            _valueInputField.text = CurrentValue.ToString();
        }

        private void OnEndEdit(string value)
        {
            CurrentValue = int.Parse(value);
            CurrentValue = Mathf.Clamp(CurrentValue, _minValue, _maxValue);
            _valueInputField.text = CurrentValue.ToString();
        }
    }
}
