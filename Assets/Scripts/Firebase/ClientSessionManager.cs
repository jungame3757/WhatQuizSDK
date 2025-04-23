using UnityEngine;
using System;
using System.Collections.Generic;
using FirebaseWebGL.Scripts.FirebaseBridge;
using FullSerializer;
using System.Text.RegularExpressions;

namespace Firebase
{
    public class ClientSessionManager : MonoBehaviour
    {
        // 세션 데이터 및 코드
        private SessionData _localSessionData;
        private string _sessionCode;
        private string _playerName;
        private string _localPlayerId;
        private bool _isJoined = false;
        
        // 직렬화 도구
        private fsSerializer _serializer;
        
        // 이벤트
        public event Action<SessionData> OnSessionJoined;
        public event Action<string> OnError;
        public event Action<List<Player>> OnPlayersChanged;
        public event Action<string> OnSessionStatusChanged;
        public event Action<string> OnGameStatusChanged;
        public event Action<string> OnPlayerKicked;
        
        public SessionData LocalSessionData => _localSessionData;
        public string SessionCode => _sessionCode;
        public bool IsJoined => _isJoined;
        public string LocalPlayerId => _localPlayerId;
        
        private void Awake()
        {
            _serializer = new fsSerializer();
        }
        
        private void OnDestroy()
        {
            // 세션 리스너 제거
            if (_isJoined)
            {
                RemoveAllSessionListeners();
            }
        }
        
        /// <summary>
        /// 세션 존재 여부와 활성 상태를 확인합니다.
        /// </summary>
        /// <param name="sessionCode">확인할 세션 코드</param>
        /// <returns>세션이 존재하고 활성 상태인 경우 true</returns>
        public void CheckSessionExistAndActive(string sessionCode)
        {
            UI.UISystem.DebugManager.Instance.ShowLoading();
            if (string.IsNullOrEmpty(sessionCode))
            {
                Debug.LogError("세션 코드가 비어있습니다.");
                OnError?.Invoke("세션 코드가 비어있습니다.");
            }
            
            // 세션 존재 여부 확인을 위한 경로
            string path = $"sessionCodes/{sessionCode}";
            
            // 세션 데이터 가져오기
            FirebaseDatabase.GetJSON(path, gameObject.name, "OnSessionExistCheck", "OnSessionExistError");
        }
        
        /// <summary>
        /// 세션 존재 확인 결과 처리
        /// </summary>
        /// <param name="data">세션 데이터 JSON</param>
        public void OnSessionExistCheck(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data) || data == "null")
                {
                    Debug.Log("세션이 존재하지 않습니다.");
                    OnError?.Invoke("존재하지 않는 세션 코드입니다.");
                    return;
                }
                
                // 세션 데이터 파싱
                fsData fsData = fsJsonParser.Parse(data);
                SessionData sessionData = null;
                _serializer.TryDeserialize(fsData, ref sessionData);
                
                if (sessionData != null)
                {
                    // 현재 시간 (밀리초)
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    
                    // 만료 시간 검사
                    if (currentTime > sessionData.expiresAt)
                    {
                        Debug.Log("세션이 만료되었습니다.");
                        OnError?.Invoke("세션이 만료되었습니다.");
                        UI.UISystem.DebugManager.Instance.ShowMassage("세션 종료", "세션이 만료되었습니다.", RedirectToHome, RedirectToHome);
                        return;
                    }
                    
                    // 세션 상태 확인
                    if (sessionData.sessionStatus == "waiting" || sessionData.sessionStatus == "playing")
                    {
                        Debug.Log($"세션이 존재하고 활성 상태입니다. 상태: {sessionData.sessionStatus}");
                    }
                    else if (sessionData.sessionStatus == "finished")
                    {
                        Debug.Log("세션이 이미 종료되었습니다.");
                        OnError?.Invoke("세션이 이미 종료되었습니다.");
                        UI.UISystem.DebugManager.Instance.ShowMassage("세션 종료", "세션이 이미 종료되었습니다.", RedirectToHome, RedirectToHome);
                    }
                    else
                    {
                        Debug.Log($"알 수 없는 세션 상태: {sessionData.sessionStatus}");
                        OnError?.Invoke("알 수 없는 세션 상태입니다.");
                        UI.UISystem.DebugManager.Instance.ShowMassage("세션 종료", "알 수 없는 세션 상태입니다.", RedirectToHome, RedirectToHome);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("세션 데이터 파싱 오류: " + ex.Message);
                OnError?.Invoke("세션 데이터를 처리하는 중 오류가 발생했습니다.");
            }
            UI.UISystem.DebugManager.Instance.HideLoading();
        }
        
        /// <summary>
        /// 세션 존재 확인 오류 처리
        /// </summary>
        /// <param name="error">오류 메시지</param>
        public void OnSessionExistError(string error)
        {
            Debug.LogError("세션 확인 오류: " + error);
            OnError?.Invoke("세션을 확인하는 중 오류가 발생했습니다.");
            UI.UISystem.DebugManager.Instance.HideLoading();
            UI.UISystem.DebugManager.Instance.ShowMassage("세션 존재 확인 오류", error, RedirectToHome, RedirectToHome);
        }
        
        /// <summary>
        /// Firebase 경로에서 사용 가능한 안전한 ID로 변환합니다.
        /// </summary>
        /// <param name="id">원본 ID</param>
        /// <returns>안전한 ID</returns>
        private string GetSafeUserId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "anonymous";
            
            // Firebase 경로에서 사용할 수 없는 문자 제거
            // ".", "#", "$", "[", "]"는 Firebase 경로에서 사용할 수 없음
            string safeId = Regex.Replace(id, @"[\.\#\$\[\]]", "_");
            
            // 접두사 "Success: signed up for" 또는 유사한 패턴 제거
            safeId = Regex.Replace(safeId, @"^Success:\s*signed\s+(?:up|in)\s+for\s+", "");
            
            // "[object Object]" 패턴 제거
            safeId = safeId.Replace("[object Object]", "anonymous");
            
            // 공백 제거 및 길이 제한
            safeId = safeId.Replace(" ", "_").Trim();
            
            // ID가 비어있게 되면 익명 ID 사용
            if (string.IsNullOrEmpty(safeId))
                return "anonymous_" + DateTime.UtcNow.Ticks.ToString();
                
            return safeId;
        }
        
        /// <summary>
        /// 세션에 참가합니다.
        /// </summary>
        /// <param name="sessionCode">참가할 세션 코드</param>
        /// <param name="playerName">플레이어 이름</param>
        public void JoinSession(string sessionCode, string playerName)
        {
            if (string.IsNullOrEmpty(sessionCode))
            {
                Debug.LogError("세션 코드가 비어있습니다.");
                OnError?.Invoke("세션 코드가 비어있습니다.");
                UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "세션 코드가 비어있습니다.", RedirectToHome, RedirectToHome);
                return;
            }
            
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("플레이어 이름이 비어있습니다.");
                OnError?.Invoke("플레이어 이름을 입력해주세요.");
                UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "플레이어 이름을 입력해주세요.", RedirectToHome, RedirectToHome);
                return;
            }
            
            _sessionCode = sessionCode;
            _playerName = playerName;
            
            // 사용자 ID 가져오기
            if (ClientAuthManager.Instance != null)
            {
                string originalUserId = ClientAuthManager.Instance.UserId;
                
                if (string.IsNullOrEmpty(originalUserId))
                {
                    Debug.LogError("로그인되지 않았습니다.");
                    OnError?.Invoke("로그인이 필요합니다.");
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "로그인이 필요합니다.", RedirectToHome, RedirectToHome);
                    return;
                }
                
                // 안전한 사용자 ID로 변환
                _localPlayerId = GetSafeUserId(originalUserId);
                Debug.Log($"원본 사용자 ID: {originalUserId}, 변환된 ID: {_localPlayerId}");
            }
            else
            {
                Debug.LogError("ClientAuthManager를 찾을 수 없습니다.");
                OnError?.Invoke("인증 관리자를 찾을 수 없습니다.");
                UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "인증 관리자를 찾을 수 없습니다.", RedirectToHome, RedirectToHome);
                return;
            }

            UI.UISystem.DebugManager.Instance.ShowLoading();
            
            // 세션 존재 여부 확인을 위한 경로
            string path = $"sessionCodes/{sessionCode}";
            
            // 세션 데이터 가져오기
            FirebaseDatabase.GetJSON(path, gameObject.name, "OnJoinSessionCheck", "OnJoinSessionError");
        }
        
        /// <summary>
        /// 세션 참가 확인 결과 처리
        /// </summary>
        /// <param name="data">세션 데이터 JSON</param>
        public void OnJoinSessionCheck(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data) || data == "null")
                {
                    Debug.Log("세션이 존재하지 않습니다.");
                    OnError?.Invoke("존재하지 않는 세션 코드입니다.");
                    UI.UISystem.DebugManager.Instance.HideLoading();
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "존재하지 않는 세션 코드입니다.", RedirectToHome, RedirectToHome);
                    return;
                }
                
                // 세션 데이터 파싱
                fsData fsData = fsJsonParser.Parse(data);
                SessionData sessionData = null;
                _serializer.TryDeserialize(fsData, ref sessionData);
                
                if (sessionData == null)
                {
                    Debug.LogError("세션 데이터 파싱 실패");
                    OnError?.Invoke("세션 데이터를 처리하는 중 오류가 발생했습니다.");
                    UI.UISystem.DebugManager.Instance.HideLoading();
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "세션 데이터 파싱 실패", RedirectToHome, RedirectToHome);
                    return;
                }
                
                // 현재 시간 (밀리초)
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                
                // 만료 시간 검사
                if (currentTime > sessionData.expiresAt)
                {
                    Debug.Log("세션이 만료되었습니다.");
                    OnError?.Invoke("세션이 만료되었습니다.");
                    UI.UISystem.DebugManager.Instance.HideLoading();
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 종료", "세션이 만료되었습니다.", RedirectToHome, RedirectToHome);
                    return;
                }
                
                // 세션 상태 확인
                if (sessionData.sessionStatus != "waiting")
                {
                    Debug.Log($"세션이 대기 상태가 아닙니다: {sessionData.sessionStatus}");
                    OnError?.Invoke("이미 시작된 세션에는 참가할 수 없습니다.");
                    UI.UISystem.DebugManager.Instance.HideLoading();
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "이미 시작된 세션에는 참가할 수 없습니다.", RedirectToHome, RedirectToHome);
                    return;
                }
                
                // 플레이어가 이미 존재하는지 확인
                var existingPlayers = sessionData.players;
                if (existingPlayers != null && existingPlayers.Exists(p => p.id == _localPlayerId))
                {
                    // 이미 참가된 플레이어
                    _isJoined = true;
                    _localSessionData = sessionData;
                    
                    // 세션 데이터 변경 감지를 위한 리스너 등록
                    ListenForSessionChanges();
                    
                    // 세션 참가 이벤트 발생
                    OnSessionJoined?.Invoke(_localSessionData);
                    
                    Debug.Log("이미 세션에 참가된 플레이어입니다.");
                    UI.UISystem.DebugManager.Instance.HideLoading();
                    return;
                }
                
                // 플레이어 객체 생성
                Player newPlayer = new Player(_localPlayerId, _playerName);
                
                // 플레이어 데이터를 JSON으로 직렬화
                fsData playerData = new fsData();
                _serializer.TrySerialize(newPlayer, out playerData);
                string playerJson = fsJsonPrinter.CompressedJson(playerData);
                
                // 플레이어를 세션에 추가
                string playerPath = $"sessionCodes/{_sessionCode}/players/{_localPlayerId}";
                FirebaseDatabase.PostJSON(playerPath, playerJson, gameObject.name, "OnPlayerAddedSuccess", "OnJoinSessionError");
            }
            catch (Exception ex)
            {
                Debug.LogError("세션 데이터 처리 오류: " + ex.Message);
                OnError?.Invoke("세션 데이터를 처리하는 중 오류가 발생했습니다: " + ex.Message);
                UI.UISystem.DebugManager.Instance.HideLoading();
                UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "세션 데이터를 처리하는 중 오류가 발생했습니다.", RedirectToHome, RedirectToHome);
            }
        }
        
        /// <summary>
        /// 플레이어가 세션에 성공적으로 추가됨
        /// </summary>
        /// <param name="response">응답 데이터</param>
        public void OnPlayerAddedSuccess(string response)
        {
            Debug.Log("플레이어가 세션에 성공적으로 추가됨: " + response);
            
            // 업데이트된 세션 데이터 가져오기
            string path = $"sessionCodes/{_sessionCode}";
            FirebaseDatabase.GetJSON(path, gameObject.name, "OnFinalSessionDataReceived", "OnJoinSessionError");
        }
        
        /// <summary>
        /// 최종 세션 데이터 수신 처리
        /// </summary>
        /// <param name="data">세션 데이터 JSON</param>
        public void OnFinalSessionDataReceived(string data)
        {
            try
            {
                // 세션 데이터 파싱
                fsData fsData = fsJsonParser.Parse(data);
                _serializer.TryDeserialize(fsData, ref _localSessionData);
                
                if (_localSessionData != null)
                {
                    _isJoined = true;
                    
                    // 세션 데이터 변경 감지를 위한 리스너 등록
                    ListenForSessionChanges();
                    
                    // 세션 참가 이벤트 발생
                    OnSessionJoined?.Invoke(_localSessionData);
                    
                    Debug.Log("세션 참가 성공");
                }
                else
                {
                    Debug.LogError("최종 세션 데이터 파싱 실패");
                    OnError?.Invoke("최종 세션 데이터를 처리하는 중 오류가 발생했습니다.");
                    UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "최종 세션 데이터 파싱 실패", RedirectToHome, RedirectToHome);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("최종 세션 데이터 처리 오류: " + ex.Message);
                OnError?.Invoke("최종 세션 데이터를 처리하는 중 오류가 발생했습니다: " + ex.Message);
                UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "최종 세션 데이터를 처리하는 중 오류가 발생했습니다.", RedirectToHome, RedirectToHome);
            }
            
            UI.UISystem.DebugManager.Instance.HideLoading();
        }
        
        /// <summary>
        /// 세션 참가 오류 처리
        /// </summary>
        /// <param name="error">오류 메시지</param>
        public void OnJoinSessionError(string error)
        {
            Debug.LogError("세션 참가 오류: " + error);
            OnError?.Invoke("세션 참가에 실패했습니다: " + error);
            UI.UISystem.DebugManager.Instance.HideLoading();
            UI.UISystem.DebugManager.Instance.ShowMassage("세션 참가 오류", "세션 참가에 실패했습니다: " + error, RedirectToHome, RedirectToHome);
        }
        
        /// <summary>
        /// 세션 변경 감지를 위한 리스너 등록
        /// </summary>
        private void ListenForSessionChanges()
        {
            if (string.IsNullOrEmpty(_sessionCode)) return;
            
            // 전체 세션 데이터 리스닝
            string path = $"sessionCodes/{_sessionCode}";
            FirebaseDatabase.ListenForValueChanged(path, gameObject.name, "OnSessionDataChanged", "OnSessionListenerError");
            
            // 개별 세션 데이터 리스닝
            string playersPath = $"sessionCodes/{_sessionCode}/players";
            FirebaseDatabase.ListenForValueChanged(playersPath, gameObject.name, "OnPlayersDataChanged", "OnSessionListenerError");
            
            string sessionStatusPath = $"sessionCodes/{_sessionCode}/sessionStatus";
            FirebaseDatabase.ListenForValueChanged(sessionStatusPath, gameObject.name, "OnSessionStatusDataChanged", "OnSessionListenerError");
            
            string gameStatusPath = $"sessionCodes/{_sessionCode}/gameStatus";
            FirebaseDatabase.ListenForValueChanged(gameStatusPath, gameObject.name, "OnGameStatusDataChanged", "OnSessionListenerError");
        }
        
        /// <summary>
        /// 세션에서 나갑니다.
        /// </summary>
        public void LeaveSession()
        {
            if (!_isJoined || string.IsNullOrEmpty(_sessionCode) || string.IsNullOrEmpty(_localPlayerId))
            {
                Debug.LogWarning("세션에 참가하지 않은 상태입니다.");
                return;
            }
            
            // 세션 나가기 요청을 위한 파라미터 구성
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "sessionCode", _sessionCode },
                { "playerId", _localPlayerId }
            };
            
            // 세션 나가기 요청 보내기
            FirebaseFunctions.CallCloudFunction(
                "leaveSession",
                parameters,
                response => {
                    Debug.Log("세션 나가기 성공");
                    // 세션 리스너 제거
                    RemoveAllSessionListeners();
                    _isJoined = false;
                    _sessionCode = null;
                    _localSessionData = null;
                },
                exception => {
                    Debug.LogError("세션 나가기 오류: " + exception.Message);
                    OnError?.Invoke("세션 나가기에 실패했습니다: " + exception.Message);
                }
            );
        }
        
        /// <summary>
        /// 세션 데이터 변경 처리
        /// </summary>
        /// <param name="data">세션 데이터 JSON</param>
        public void OnSessionDataChanged(string data)
        {
            Debug.Log("세션 데이터 변경됨: " + data);
            try
            {
                fsData fsData = fsJsonParser.Parse(data);
                _serializer.TryDeserialize(fsData, ref _localSessionData);
                // 필요한 UI 업데이트나 게임 로직 처리
            }
            catch (Exception ex)
            {
                Debug.LogError("세션 데이터 파싱 오류: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 플레이어 데이터 변경 처리
        /// </summary>
        /// <param name="data">플레이어 데이터 JSON</param>
        public void OnPlayersDataChanged(string data)
        {
            Debug.Log("플레이어 데이터 변경됨: " + data);
            try
            {
                fsData fsData = fsJsonParser.Parse(data);
                
                // Dictionary<string, Player>로 먼저 역직렬화
                Dictionary<string, Player> playerDict = null;
                _serializer.TryDeserialize(fsData, ref playerDict);
                
                // 자신의 데이터가 플레이어 목록에서 사라졌는지 확인 (강퇴 감지)
                if (playerDict != null)
                {
                    bool playerExists = playerDict.ContainsKey(_localPlayerId);
                    
                    // 이전에 세션에 참가했었고, 현재 플레이어 목록에 자신이 없으면 강퇴된 것으로 처리
                    if (_isJoined && !playerExists && !string.IsNullOrEmpty(_localPlayerId))
                    {
                        Debug.Log($"플레이어 '{_playerName}'(ID: {_localPlayerId})가 강퇴되었습니다.");
                        
                        // 세션 리스너 제거
                        RemoveAllSessionListeners();
                        
                        // 강퇴 이벤트 발생
                        OnPlayerKicked?.Invoke(_localPlayerId);
                        return;
                    }
                    
                    // Dictionary 값들을 List<Player>로 변환
                    List<Player> players = new List<Player>(playerDict.Values);
                    
                    _localSessionData.players = players;
                    Debug.Log($"플레이어 수: {players.Count}");
                    OnPlayersChanged?.Invoke(players);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("플레이어 데이터 파싱 오류: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 세션 상태 변경 처리
        /// </summary>
        /// <param name="data">세션 상태 데이터</param>
        public void OnSessionStatusDataChanged(string data)
        {
            Debug.Log("세션 상태 변경됨: " + data);
            try
            {
                // 데이터가 문자열 형태일 수 있으므로 따옴표 제거
                string sessionStatus = data.Replace("\"", "");
                _localSessionData.sessionStatus = sessionStatus;
                
                // 세션 상태 관련 UI 업데이트
                Debug.Log($"세션 상태: {sessionStatus}");
                OnSessionStatusChanged?.Invoke(sessionStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError("세션 상태 파싱 오류: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 게임 상태 변경 처리
        /// </summary>
        /// <param name="data">게임 상태 데이터</param>
        public void OnGameStatusDataChanged(string data)
        {
            Debug.Log("게임 상태 변경됨: " + data);
            try
            {
                // 데이터가 문자열 형태일 수 있으므로 따옴표 제거
                string gameStatus = data.Replace("\"", "");
                _localSessionData.gameStatus = gameStatus;
                
                // 게임 상태 관련 UI 업데이트
                Debug.Log($"게임 상태: {gameStatus}");
                OnGameStatusChanged?.Invoke(gameStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError("게임 상태 파싱 오류: " + ex.Message);
            }
        }
        
        /// <summary>
        /// 세션 리스너 오류 처리
        /// </summary>
        /// <param name="error">오류 메시지</param>
        public void OnSessionListenerError(string error)
        {
            Debug.LogError("세션 리스너 오류: " + error);
        }
        
        /// <summary>
        /// 모든 세션 리스너 제거
        /// </summary>
        private void RemoveAllSessionListeners()
        {
            if (string.IsNullOrEmpty(_sessionCode)) return;
            
            // 전체 세션 데이터 리스너 제거
            string path = $"sessionCodes/{_sessionCode}";
            FirebaseDatabase.StopListeningForValueChanged(path, gameObject.name, "OnListenerRemoved", "OnSessionListenerError");
            
            // 개별 세션 데이터 리스너 제거
            string playersPath = $"sessionCodes/{_sessionCode}/players";
            FirebaseDatabase.StopListeningForValueChanged(playersPath, gameObject.name, "OnListenerRemoved", "OnSessionListenerError");
            
            string sessionStatusPath = $"sessionCodes/{_sessionCode}/sessionStatus";
            FirebaseDatabase.StopListeningForValueChanged(sessionStatusPath, gameObject.name, "OnListenerRemoved", "OnSessionListenerError");
            
            string gameStatusPath = $"sessionCodes/{_sessionCode}/gameStatus";
            FirebaseDatabase.StopListeningForValueChanged(gameStatusPath, gameObject.name, "OnListenerRemoved", "OnSessionListenerError");
        }
        
        /// <summary>
        /// 리스너 제거 콜백
        /// </summary>
        /// <param name="data">콜백 데이터</param>
        public void OnListenerRemoved(string data)
        {
            Debug.Log("리스너 제거됨: " + data);
        }
        
        [Serializable]
        private class JoinSessionResponse
        {
            public bool success;
            public string message;
            public SessionData sessionData;
        }

        public void RedirectToHome()
        {
            URL.OpenURLInSameWindow("https://whatquiz.store/");
        }
    }
}
