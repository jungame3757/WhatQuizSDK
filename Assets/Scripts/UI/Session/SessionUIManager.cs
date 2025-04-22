using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using System;
using FirebaseWebGL.Scripts.FirebaseBridge;
using System.Collections;
using UnityEngine.Networking;

public class SessionUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SessionManager _sessionManager;

    [Header("System UI")]
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _gameModeStatusText;

    [Header("Invite UI")]
    [SerializeField] private RawImage _qrCodeImage;
    [SerializeField] private TMP_Text _inviteCodeText;
    [SerializeField] private Button _copyButton;

    [Header("QR Wide UI")]
    [SerializeField] private RawImage _qrCodeWideImage;
    [SerializeField] private TMP_Text _inviteCodeWideText;
    [SerializeField] private Button _copyWideButton;
    
    [Header("Player List UI")]
    [SerializeField] private GameObject _playerListContent;
    [SerializeField] private GameObject _playerIconPrefab;
    [SerializeField] private TMP_Text _playerCountText;
    
    private string _sessionCode;
    private string _localPlayerId;
    private bool _isLocalPlayerHost;
    private Dictionary<string, PlayerIcon> _playerIcons = new Dictionary<string, PlayerIcon>();
    private Texture2D _qrCodeTexture;
    
    private void Start()
    {
        Debug.Log("SessionUIManager 시작...");
        
        // 세션 관리자 찾기
        if (_sessionManager == null)
        {
            _sessionManager = FindObjectOfType<Firebase.SessionManager>();
            Debug.Log($"세션 관리자 검색 결과: {(_sessionManager != null ? "찾음" : "찾지 못함")}");
        }
        
        if (_sessionManager == null)
        {
            Debug.LogError("SessionManager 참조를 찾을 수 없습니다! UI 초기화를 중단합니다.");
            return;
        }
        
        // 플레이어 리스트 초기화
        InitializePlayerList();
        
        // 이벤트 등록
        _sessionManager.OnSessionCreated += OnSessionCreated;
        _sessionManager.OnPlayersChanged += OnPlayersChanged;
        _sessionManager.OnSessionStatusChanged += OnSessionStatusChanged;
        _sessionManager.OnGameStatusChanged += OnGameStatusChanged;
        _sessionManager.OnError += OnError;
        Debug.Log("SessionManager 이벤트 등록 완료");
        
        // 버튼 이벤트 설정
        SetupButtons();
        
        // 현재 로컬 플레이어 ID 가져오기
        _localPlayerId = GetLocalPlayerId();
    }
    
    private void OnDestroy()
    {
        if (_sessionManager != null)
        {
            // 이벤트 해제
            _sessionManager.OnSessionCreated -= OnSessionCreated;
            _sessionManager.OnPlayersChanged -= OnPlayersChanged;
            _sessionManager.OnSessionStatusChanged -= OnSessionStatusChanged;
            _sessionManager.OnGameStatusChanged -= OnGameStatusChanged;
            _sessionManager.OnError -= OnError;
        }
        
        // QR 코드 텍스처 정리
        if (_qrCodeTexture != null)
        {
            Destroy(_qrCodeTexture);
        }
    }
    
    private void SetupButtons()
    {
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClicked);
            _startButton.interactable = false; // 기본적으로 비활성화
        }
        
        if (_copyButton != null)
        {
            _copyButton.onClick.AddListener(() => CopySessionCodeToClipboard());
        }
        
        if (_copyWideButton != null)
        {
            _copyWideButton.onClick.AddListener(() => CopySessionCodeToClipboard());
        }
    }
    
    #region Event Handlers
    
    private void OnSessionCreated(string sessionCode, SessionData sessionData)
    {
        _sessionCode = sessionCode;
        
        // 로컬 플레이어 ID가 없으면 호스트 ID 사용
        if (string.IsNullOrEmpty(_localPlayerId))
        {
            _localPlayerId = sessionData.hostId;
            Debug.Log($"로컬 ID가 설정되지 않아 호스트 ID 사용: {_localPlayerId}");
        }
        
        _isLocalPlayerHost = sessionData.hostId == _localPlayerId;
        Debug.Log($"세션 생성됨 - 코드: {sessionCode}");
        
        // 세션 코드 표시
        UpdateSessionCodeUI(sessionCode);

        // 게임 모드 표시
        UpdateGameModeStatus(sessionData.gameSetting);
        
        // QR 코드 생성
        GenerateQRCode(sessionCode);
        
        // 플레이어 목록 초기화
        UpdatePlayerList(sessionData.players);
    }


    private void OnPlayersChanged(List<Player> players)
    {
        Debug.Log($"플레이어 목록 변경 감지: {players?.Count}명");
        
        // 세션 ID가 설정되었는지 확인
        if (string.IsNullOrEmpty(_sessionCode))
        {
            Debug.LogWarning("세션 코드가 설정되지 않았습니다. 플레이어 목록을 업데이트할 수 없습니다.");
            return;
        }
        
        // 플레이어 목록이 null이 아닌지 확인
        if (players == null)
        {
            Debug.LogWarning("플레이어 데이터가 null입니다.");
            return;
        }
        
        // 플레이어 목록 업데이트
        UpdatePlayerList(players);
    }
    
    private void OnSessionStatusChanged(string status)
    {
        // 세션 상태에 따른 UI 변경
        switch (status)
        {
            case "waiting":
                // 대기실 UI 보이기
                break;
                
            case "playing":
                // 게임 시작 시 필요한 UI 변경
                break;
                
            case "finished":
                // 게임 종료 시 필요한 UI 변경
                break;
        }
    }
    
    private void OnGameStatusChanged(string status)
    {
        // 게임 상태에 따른 UI 변경
        Debug.Log($"게임 상태 변경: {status}");
    }
    
    private void OnError(string errorMessage)
    {
        Debug.LogError($"세션 오류: {errorMessage}");
        // 오류 메시지 표시
    }
    
    #endregion
    
    #region UI Management
    
    private void UpdateSessionCodeUI(string code)
    {
        if (_inviteCodeText != null)
        {
            _inviteCodeText.text = code;
        }
        
        if (_inviteCodeWideText != null)
        {
            _inviteCodeWideText.text = code;
        }
    }

    private void UpdateGameModeStatus(GameSettingData gameSetting)
    {
        if (gameSetting.gameMode == "time")
        {
            _gameModeStatusText.text = $"시간제한 {gameSetting.timeLimit}분";
        }
        else if (gameSetting.gameMode == "score"){
            _gameModeStatusText.text = $"목표점수 {gameSetting.scoreLimit}점";
        }

        ContentSizeFitter contentSizeFitter = _gameModeStatusText.GetComponentInParent<ContentSizeFitter>();
        contentSizeFitter.SetLayoutHorizontal();
    }

    private void GenerateQRCode(string code)
    {
        string url = $"https://play.wq.com/session?code={code}";
        string size = "650"; // QR 코드 이미지 크기
        
        Debug.Log($"QR 코드 생성 시작: {url}");
        
        // URL.URLToQRCode.GetQRCodeByURL을 사용하여 QR 코드 생성
        URL.GetQRCodeByURL(url, size, gameObject.name, "OnQRCodeGenerated");
    }
    
    // QR 코드 생성 완료 시 호출되는 콜백 메서드
    public void OnQRCodeGenerated(string qrUrl)
    {
        if (string.IsNullOrEmpty(qrUrl))
        {
            Debug.LogError("QR 코드 생성 실패");
            return;
        }
        
        Debug.Log("QR 코드 URL 수신: " + qrUrl);
        
        // 이미지 로드를 위한 UnityWebRequest 사용
        StartCoroutine(LoadQRCodeFromURL(qrUrl));
    }
    
    private IEnumerator LoadQRCodeFromURL(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("QR 코드 이미지 다운로드 실패: " + webRequest.error);
                yield break;
            }
            
            // 기존 텍스처 해제
            if (_qrCodeTexture != null)
            {
                Destroy(_qrCodeTexture);
            }
            
            // 다운로드한 텍스처 가져오기
            _qrCodeTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
            
            // QR 코드 이미지 적용
            if (_qrCodeImage != null)
            {
                _qrCodeImage.texture = _qrCodeTexture;
            }
            
            if (_qrCodeWideImage != null)
            {
                _qrCodeWideImage.texture = _qrCodeTexture;
            }
            
            Debug.Log("QR 코드 이미지 적용 완료");
        }
    }
    
    private void UpdatePlayerList(List<Player> players)
    {
        if (_playerListContent == null || _playerIconPrefab == null) return;
        
        string hostId = _sessionManager.LocalSessionData?.hostId;
        Debug.Log($"UpdatePlayerList 호출됨 - 플레이어 수: {players?.Count}, 호스트 ID: {hostId}");
        
        if (players == null) 
        {
            Debug.LogWarning("업데이트할 플레이어 목록이 없습니다.");
            return;
        }
        
        // 이미 존재하는 플레이어 아이콘 업데이트
        foreach (var player in players)
        {
            // 디버그: 각 플레이어 정보 출력
            Debug.Log($"플레이어 정보 - ID: {player.id}, 이름: {player.name}, 호스트 여부: {player.id == hostId}");
            
            // 호스트는 플레이어 목록에 표시하지 않음
            if (player.id == hostId)
            {
                Debug.Log($"호스트는 표시하지 않습니다: {player.name}");
                continue;
            }
            
            if (_playerIcons.TryGetValue(player.id, out PlayerIcon icon))
            {
                // 기존 아이콘 업데이트
                Debug.Log($"기존 플레이어 아이콘 업데이트: {player.name}");
                icon.UpdatePlayerData(player);
            }
            else
            {
                // 새 플레이어 아이콘 생성
                Debug.Log($"새 플레이어 아이콘 생성: {player.name}");
                GameObject iconObj = Instantiate(_playerIconPrefab, _playerListContent.transform);
                PlayerIcon playerIcon = iconObj.GetComponent<PlayerIcon>();
                
                if (playerIcon != null)
                {
                    playerIcon.Initialize(player, _localPlayerId, _isLocalPlayerHost, KickPlayer);
                    _playerIcons.Add(player.id, playerIcon);
                }
            }
        }
        
        // 더 이상 존재하지 않는 플레이어 아이콘 제거
        List<string> playersToRemove = new List<string>();
        
        foreach (var kvp in _playerIcons)
        {
            bool playerExists = false;
            foreach (var player in players)
            {
                // 호스트는 건너뛰기
                if (player.id == hostId)
                    continue;
                    
                if (player.id == kvp.Key)
                {
                    playerExists = true;
                    break;
                }
            }
            
            if (!playerExists)
            {
                playersToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var playerId in playersToRemove)
        {
            if (_playerIcons.TryGetValue(playerId, out PlayerIcon icon))
            {
                Debug.Log($"플레이어 아이콘 제거: {playerId}");
                Destroy(icon.gameObject);
                _playerIcons.Remove(playerId);
            }
        }
        
        // 플레이어 수 텍스트 업데이트 (호스트 제외)
        if (_playerCountText != null)
        {
            int participantCount = 0;
            foreach (var player in players)
            {
                if (player.id != hostId)
                    participantCount++;
            }
            
            _playerCountText.text = $"{participantCount}";
            Debug.Log($"플레이어 카운트 업데이트: {participantCount}명");
        }
    }
    
    private void CopySessionCodeToClipboard()
    {
        if (!string.IsNullOrEmpty(_sessionCode))
        {
            string url = $"https://play.wq.com/session?code={_sessionCode}";
            
            // URL.CopyURLToClipboard 함수 호출하여 URL을 클립보드에 복사
            URL.CopyURLToClipboard(url);
            
            Debug.Log("초대 URL이 클립보드에 복사되었습니다: " + url);
        }
    }
    
    #endregion
    
    #region Actions
    
    private void OnStartButtonClicked()
    {
        if (_isLocalPlayerHost)
        {
            // 게임 시작 로직 (Firebase 함수 호출 등)
            Debug.Log("게임 시작 요청");
            
            // 호스트만 시작 가능하므로 여기서 필요한 로직 추가
        }
    }
    
    private void KickPlayer(string playerId)
    {
        if (_isLocalPlayerHost && !string.IsNullOrEmpty(playerId))
        {
            // 플레이어 킥 로직 (Firebase 함수 호출 등)
            Debug.Log($"플레이어 킥 요청: {playerId}");
            
            // 호스트만 킥 가능하므로 여기서 필요한 로직 추가
        }
    }
    
    #endregion
    
    #region Helpers
    
    private string GetLocalPlayerId()
    {
        // AuthManager를 통해 현재 사용자 ID 가져오기
        if (Firebase.AuthManager.Instance != null)
        {
            try
            {
                // Firebase.AuthManager에서 적절한 메서드 호출
                string userId = AuthManager.Instance.GetIdToken(); // 임시 방법: 토큰을 ID로 사용
                
                if (!string.IsNullOrEmpty(userId))
                {
                    Debug.Log($"로컬 사용자 ID: {userId}");
                    return userId;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"사용자 ID 가져오기 오류: {ex.Message}");
            }
        }
        
        Debug.LogWarning("로컬 사용자 ID를 가져올 수 없습니다. 호스트 ID를 대신 사용합니다.");
        return string.Empty;
    }
    
    private void InitializePlayerList()
    {
        // 기존 플레이어 아이콘 모두 제거
        if (_playerListContent != null)
        {
            foreach (Transform child in _playerListContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        _playerIcons.Clear();
        
        // 플레이어 수 초기화
        if (_playerCountText != null)
        {
            _playerCountText.text = "0";
        }
    }
    
    #endregion
}
