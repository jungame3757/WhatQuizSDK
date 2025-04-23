using UnityEngine;
using UnityEngine.UI;
using Firebase;
using TMPro;
using System;
using UI.UISystem;

namespace UI.Session.Host
{
    public class SessionCreatorUI : MonoBehaviour
    {
        [Header("Session Manager")]
        [SerializeField] private HostSessionManager _sessionManager;
    
        [Header("Session Creater")]
        [SerializeField] private ToggleGroup _gameModeToggleGroup;
        [SerializeField] private TMP_InputField timeLimitInput;
        [SerializeField] private TMP_InputField scoreLimitInput;
        [SerializeField] private Button createSessionButton;

        [Header("SessionPanels")]
        [SerializeField] private GameObject _sessionPanel;
        [SerializeField] private GameObject _sessionCreatorPanel;
    
        private void Start()
        {
            // UI 이벤트 등록
            if (createSessionButton != null)
                createSessionButton.onClick.AddListener(OnCreateSessionClicked);
        
            // SessionManager 이벤트 등록
            if (_sessionManager != null)
            {
                _sessionManager.OnSessionCreated += HandleSessionCreated;
                _sessionManager.OnError += HandleError;
            }
            else
            {
                Debug.LogError("SessionManager가 할당되지 않았습니다. Inspector에서 할당해주세요.");
            }
        }
    
        private void OnDestroy()
        {
            // 이벤트 해제
            if (_sessionManager != null)
            {
                _sessionManager.OnSessionCreated -= HandleSessionCreated;
                _sessionManager.OnError -= HandleError;
            }
        
            if (createSessionButton != null)
                createSessionButton.onClick.RemoveListener(OnCreateSessionClicked);
        }
    
        private void OnCreateSessionClicked()
        {
            if (_sessionManager == null) return;

            DebugManager.Instance.ShowLoading();
        
            // 게임 설정 데이터 생성
            GameSettingData gameSettings = CreateGameSettingsFromUI();
        
            // 세션 생성 요청
            _sessionManager.CreateSession(gameSettings);
        }
    
        private GameSettingData CreateGameSettingsFromUI()
        {
            string gameMode = "time";
            int timeLimit = 10;
            int scoreLimit = 0;
        
            // 게임 모드 설정
            if (_gameModeToggleGroup != null)
            {
                foreach (Toggle toggle in _gameModeToggleGroup.GetComponentsInChildren<Toggle>())
                {
                    if (toggle.isOn)
                    {
                        gameMode = toggle.name;
                        break;
                    }
                }
            }
        
            // 시간 제한 설정
            if(gameMode == "time"){
                if (timeLimitInput != null && !string.IsNullOrEmpty(timeLimitInput.text))
                {
                    if (int.TryParse(timeLimitInput.text, out int result))
                        timeLimit = result;
                }
            }// 점수 제한 설정
            else if(gameMode == "score"){
                if (scoreLimitInput != null && !string.IsNullOrEmpty(scoreLimitInput.text))
                {
                    if (int.TryParse(scoreLimitInput.text, out int result))
                        scoreLimit = result;
                }
            }
        
            return new GameSettingData(gameMode, timeLimit, scoreLimit);
        }
    
        private void HandleSessionCreated(string sessionCode, SessionData sessionData)
        {
            Debug.Log($"세션 생성 완료 - 코드: {sessionCode}, 생성 시간: {new DateTime(1970, 1, 1).AddMilliseconds(sessionData.createdAt)}");

            _sessionPanel.SetActive(true);
            _sessionCreatorPanel.SetActive(false);
            
            DebugManager.Instance.HideLoading();
        }
    
        private void HandleError(string errorMessage)
        {
            DebugManager.Instance.HideLoading();
            DebugManager.Instance.ShowMassage("오류", errorMessage);
        }
    }
}
