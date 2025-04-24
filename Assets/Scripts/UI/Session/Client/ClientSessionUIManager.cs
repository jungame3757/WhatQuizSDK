using Firebase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Session.Client
{
    public class ClientSessionUIManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ClientSessionManager _clientSessionManager;

        [Header("Panels")]
        [SerializeField] private GameObject _profileCreaterPanel;
        [SerializeField] private GameObject _SessionPanel;

        [Header("Profile Creater Panel")]
        [SerializeField] private TMP_InputField _playerNameInput;
        [SerializeField] private TMP_Text _playerNameCountText;
        [SerializeField] private Button _joinSessionButton;

        [Header("Session Panel")]
        [SerializeField] private TMP_Text _waitingText;

        private string _sessionCode;
        private string _playerName;
        private string _playerId;
        
        private void Start()
        {
            Debug.Log("ClientSessionUIManager 시작...");
            
            // 세션 관리자 찾기
            if (_clientSessionManager == null)
            {
                _clientSessionManager = FindObjectOfType<ClientSessionManager>();
                Debug.Log($"클라이언트 세션 관리자 검색 결과: {(_clientSessionManager != null ? "찾음" : "찾지 못함")}");
            }
            
            if (_clientSessionManager == null)
            {
                Debug.LogError("ClientSessionManager 참조를 찾을 수 없습니다! UI 초기화를 중단합니다.");
                ShowError("ClientSessionManager를 찾을 수 없습니다.");
                return;
            }
            
            // 이벤트 등록
            RegisterEvents();
            
            // UI 버튼 이벤트 설정
            SetupButtonsAndInputField();
            
            // URL에서 세션 코드 추출
            _sessionCode = ExtractSessionCodeFromUrl();
            
            if (string.IsNullOrEmpty(_sessionCode))
            {
                Debug.LogError("세션 코드를 찾을 수 없습니다.");
                ShowError("세션 코드가 없거나 올바르지 않습니다.");
                return;
            }
            
            Debug.Log($"세션 코드 확인: {_sessionCode}");
            
            // 세션 존재 여부 및 활성 상태 확인
            _clientSessionManager.CheckSessionExistAndActive(_sessionCode);
            
            // 패널 초기화
            InitializePanels();
        }

        private void OnDestroy()
        {
            // 이벤트 해제
            UnregisterEvents();
        }
        
        private void InitializePanels()
        {
            // 패널 초기화
            if (_profileCreaterPanel != null)
                _profileCreaterPanel.SetActive(true);
            
            if (_SessionPanel != null)
                _SessionPanel.SetActive(false);
        }
        
        // ClientSessionManager.OnSessionExistCheck에 대한 응답을 처리하는 로직 추가
        // 이 메서드를 직접 호출하지는 않지만, CheckSessionExistAndActive 메서드가 
        // 내부적으로 Firebase 데이터베이스 콜백으로 OnSessionExistCheck를 호출함
        
        // OnError 이벤트를 통해 세션 검증 실패 처리
        private void OnError(string errorMessage)
        {
            Debug.LogError($"에러 발생: {errorMessage}");
            
            // 에러 메시지 표시
            ShowError(errorMessage);
            
            // 세션 존재 여부 체크 에러일 경우 _isValidSession은 false 유지
        }
        
        private void RegisterEvents()
        {
            if (_clientSessionManager != null)
            {
                _clientSessionManager.OnSessionJoined += OnSessionJoined;
                _clientSessionManager.OnPlayersChanged += OnPlayersChanged;
                _clientSessionManager.OnSessionStatusChanged += OnSessionStatusChanged;
                _clientSessionManager.OnGameStatusChanged += OnGameStatusChanged;
                _clientSessionManager.OnPlayerKicked += OnPlayerKicked;
                _clientSessionManager.OnError += OnError;
                
                Debug.Log("ClientSessionManager 이벤트 등록 완료");
            }
        }
        
        private void UnregisterEvents()
        {
            if (_clientSessionManager != null)
            {
                _clientSessionManager.OnSessionJoined -= OnSessionJoined;
                _clientSessionManager.OnPlayersChanged -= OnPlayersChanged;
                _clientSessionManager.OnSessionStatusChanged -= OnSessionStatusChanged;
                _clientSessionManager.OnGameStatusChanged -= OnGameStatusChanged;
                _clientSessionManager.OnPlayerKicked -= OnPlayerKicked;
                _clientSessionManager.OnError -= OnError;
                
                // 임시로 추가한 핸들러 제거
                RemoveSessionExistCheckHandler();
            }
        }
        
        private void SetupButtonsAndInputField()
        {
            if (_joinSessionButton != null)
            {
                _joinSessionButton.onClick.AddListener(OnJoinSessionClicked);
                _joinSessionButton.interactable = false; // 초기에는 비활성화
            }

            if(_playerNameInput != null)
            {
                _playerNameInput.onValueChanged.AddListener(OnPlayerNameInputChanged);
            }
        }
        
        private void OnPlayerNameInputChanged(string value)
        {
            _playerName = value;
            UpdatePlayerNameCount(value.Length);
            
            // 입력값이 있으면 버튼 활성화
            if (_joinSessionButton != null)
            {
                _joinSessionButton.interactable = !string.IsNullOrEmpty(value.Trim());
            }
        }
        
        private void UpdatePlayerNameCount(int count)
        {
            if (_playerNameCountText != null)
            {
                _playerNameCountText.text = $"{count}/10";
                _playerNameCountText.color = count > 10 ? Color.red : Color.black;
            }
        }
        
        private void OnJoinSessionClicked()
        {
            if (string.IsNullOrEmpty(_playerName) || _playerName.Trim().Length == 0)
            {
                Debug.LogError("플레이어 이름을 입력해주세요.");
                ShowError("플레이어 이름을 입력해주세요.");
                return;
            }
            
            if (_playerName.Length > 10)
            {
                Debug.LogError("플레이어 이름은 10자를 초과할 수 없습니다.");
                ShowError("플레이어 이름은 10자를 초과할 수 없습니다.");
                return;
            }
            
            // 세션 참가 요청
            Debug.Log($"세션 참가 요청: {_sessionCode}, {_playerName}");
            _clientSessionManager.JoinSession(_sessionCode, _playerName);
        }
        
        private string ExtractSessionCodeFromUrl()
        {
            // URL에서 세션 코드 추출 로직
            // 예: https://jungame3757.github.io/WhatQuizClient/?code=ABC123 -> ABC123
            string url = Application.absoluteURL;
            
            if (string.IsNullOrEmpty(url))
            {
                // 개발 환경에서는 더미 코드 사용
                return "TESTCODE";
            }
            
            int codeIndex = url.IndexOf("code=");
            if (codeIndex >= 0)
            {
                string code = url.Substring(codeIndex + 5);
                int endIndex = code.IndexOf("&");
                if (endIndex >= 0)
                {
                    code = code.Substring(0, endIndex);
                }
                return code;
            }
            
            return string.Empty;
        }
        
        // 세션 이벤트 핸들러
        private void OnSessionJoined(SessionData sessionData)
        {
            Debug.Log($"세션 참가 성공: {_sessionCode}");
            
            // ClientSessionManager에서 직접 로컬 플레이어 ID 가져오기
            if (_clientSessionManager != null)
            {
                _playerId = _clientSessionManager.LocalPlayerId;
                Debug.Log($"세션 참가 - 로컬 플레이어 ID: {_playerId}");
            }
            
            // 플레이어 ID가 여전히 비어있는 경우 플레이어 목록에서 검색 (백업 방법)
            if (string.IsNullOrEmpty(_playerId))
            {
                foreach (var player in sessionData.players)
                {
                    if (player.name == _playerName)
                    {
                        _playerId = player.id;
                        Debug.Log($"세션 참가 - 플레이어 목록에서 찾은 ID: {_playerId}");
                        break;
                    }
                }
            }
            
            // 여전히 ID를 찾지 못했을 경우 로그 출력
            if (string.IsNullOrEmpty(_playerId))
            {
                Debug.LogWarning("플레이어 ID를 찾을 수 없습니다!");
            }
            
            // 프로필 생성 패널 숨기고 세션 패널 표시
            _profileCreaterPanel.SetActive(false);
            _SessionPanel.SetActive(true);
            
            // 대기 메시지 업데이트
            UpdateWaitingText(sessionData.players.Count);
        }
        
        private void OnPlayersChanged(List<Player> players)
        {
            Debug.Log($"플레이어 목록 업데이트: {players.Count}명");
            
            // 대기 메시지 업데이트
            UpdateWaitingText(players.Count);
        }
        
        private void UpdateWaitingText(int playerCount)
        {
            if (_waitingText != null)
            {
                _waitingText.text = $"{_playerName}님\n참가완료!";
            }
        }
        
        private void OnSessionStatusChanged(string status)
        {
            Debug.Log($"세션 상태 변경: {status}");
            
            // 세션 상태에 따른 UI 업데이트
            switch (status)
            {
                case "waiting":
                    // 대기 중 상태 UI
                    break;
                case "playing":
                    // 게임 시작 UI
                    break;
                case "ended":
                    // 게임 종료 UI
                    break;
            }
        }
        
        private void OnGameStatusChanged(string status)
        {
            Debug.Log($"게임 상태 변경: {status}");
            
            // 게임 상태에 따른 UI 업데이트
        }
        
        private void RemoveSessionExistCheckHandler()
        {
            // 임시 핸들러 제거 로직 (필요시 구현)
            CancelInvoke("ValidateSessionAfterDelay");
        }
        
        private void ShowError(string message)
        {
            UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", message, null, null);
        }
        
        // 강퇴 이벤트 핸들러
        private void OnPlayerKicked(string kickedPlayerId)
        {
            Debug.Log($"플레이어 강퇴 이벤트: {kickedPlayerId}, 내 ID: {_playerId}");
            
            // 현재 플레이어 ID가 비어있으면 ClientSessionManager에서 다시 가져오기 시도
            if (string.IsNullOrEmpty(_playerId) && _clientSessionManager != null)
            {
                _playerId = _clientSessionManager.LocalPlayerId;
                Debug.Log($"강퇴 이벤트 처리 시 플레이어 ID 재설정: {_playerId}");
            }
            
            // 강퇴된 플레이어가 본인인지 확인
            if (_playerId == kickedPlayerId)
            {
                Debug.Log("세션에서 강퇴되었습니다. 메인 페이지로 이동합니다.");
                
                // 알림 표시
                UI.UISystem.DebugManager.Instance.ShowMassage("강퇴", "호스트에 의해 세션에서 강퇴되었습니다.", 
                    _clientSessionManager.RedirectToHome, _clientSessionManager.RedirectToHome);
            }
            else
            {
                Debug.Log($"다른 플레이어가 강퇴되었습니다. 강퇴된 ID: {kickedPlayerId}");
            }
        }
    }
}