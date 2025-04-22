using UnityEngine;
using System;
using System.Collections.Generic;
using FirebaseWebGL.Scripts.FirebaseBridge;
using FullSerializer;
using System.Collections;

namespace Firebase{
    public class SessionManager : MonoBehaviour
    {
        public SessionData LocalSessionData => _localSessionData;
        private SessionData _localSessionData;
        
        public string SessionCode => _sessionCode;
        private string _sessionCode;
        
        [SerializeField] private string gameStatus = "default";        
        private fsSerializer _serializer;
        
        public event Action<string, SessionData> OnSessionCreated;
        public event Action<string> OnError;
        
        // 새로운 이벤트 추가
        public event Action<List<Player>> OnPlayersChanged;
        public event Action<string> OnSessionStatusChanged;
        public event Action<string> OnGameStatusChanged;
        
        private void Awake()
        {
            _serializer = new fsSerializer();
        }
        
        private void OnDestroy()
        {
            // 객체가 파괴될 때 모든 리스너 제거
            RemoveAllSessionListeners();
        }
        
        #region Session Management
        
        /// <summary>
        /// 세션을 생성합니다.
        /// </summary>
        /// <param name="gameSettings">게임 설정 데이터</param>
        public void CreateSession(GameSettingData gameSettings)
        {
            // 인증 토큰 가져오기
            string token = null;
            if (AuthManager.Instance != null)
            {
                token = AuthManager.Instance.GetIdToken();
            }
            
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("인증 토큰이 없습니다. 로그인이 필요합니다.");
                OnError?.Invoke("인증 토큰이 없습니다. 로그인이 필요합니다.");
                return;
            }
            
            // 세션 생성 요청을 위한 파라미터 구성
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "gameStatus", gameStatus },
                { "idToken", token }
            };
            
            // GameSettingData 추가
            if (gameSettings != null)
            {
                fsData data;
                _serializer.TrySerialize(gameSettings, out data);
                string gameSettingJson = fsJsonPrinter.CompressedJson(data);
                parameters.Add("gameSetting", gameSettingJson);
            }
            
            // Cloud Function 호출
            FirebaseFunctions.CallCloudFunction(
                "createSessionCode", 
                parameters,
                response => {
                    HandleCreateSessionResponse(response.Text);
                },
                exception => {
                    Debug.LogError("세션 생성 오류: " + exception.Message);
                    OnError?.Invoke("세션 생성에 실패했습니다: " + exception.Message);
                }
            );
        }
        
        private void HandleCreateSessionResponse(string responseJson)
        {
            try
            {
                // 응답 JSON을 파싱
                fsData data = fsJsonParser.Parse(responseJson);
                CreateSessionResponse response = null;
                _serializer.TryDeserialize(data, ref response);
                
                if (response != null && response.success)
                {
                    _sessionCode = response.sessionCode;
                    
                    // 세션 데이터 변환
                    _localSessionData = response.sessionData;
                    
                    Debug.Log("세션 생성 성공: " + _sessionCode);

                    OnSessionCreated?.Invoke(_sessionCode, _localSessionData);
                    
                    // 세션 데이터 변경을 감지하기 위한 리스너 등록
                    ListenForSessionChanges();
                }
                else
                {
                    Debug.LogError("세션 생성 실패");
                    OnError?.Invoke("세션 생성에 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("응답 처리 오류: " + ex.Message);
                OnError?.Invoke("응답 처리 중 오류가 발생했습니다: " + ex.Message);
            }
        }
        
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
        
        // players 데이터 변경 감지
        public void OnPlayersDataChanged(string data)
        {
            Debug.Log("플레이어 데이터 변경됨: " + data);
            try
            {
                fsData fsData = fsJsonParser.Parse(data);
                List<Player> players = null;
                _serializer.TryDeserialize(fsData, ref players);
                
                if (players != null)
                {
                    _localSessionData.players = players;
                    // 플레이어 관련 UI 업데이트나 게임 로직 처리
                    Debug.Log($"플레이어 수: {players.Count}");
                    OnPlayersChanged?.Invoke(players);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("플레이어 데이터 파싱 오류: " + ex.Message);
            }
        }
        
        // sessionStatus 변경 감지
        public void OnSessionStatusDataChanged(string data)
        {
            Debug.Log("세션 상태 변경됨: " + data);
            try
            {
                // 데이터가 문자열 형태일 수 있으므로 따옴표 제거
                string sessionStatus = data.Replace("\"", "");
                _localSessionData.sessionStatus = sessionStatus;
                
                // 세션 상태 관련 UI 업데이트나 게임 로직 처리
                Debug.Log($"세션 상태: {sessionStatus}");
                OnSessionStatusChanged?.Invoke(sessionStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError("세션 상태 파싱 오류: " + ex.Message);
            }
        }
        
        // gameStatus 변경 감지
        public void OnGameStatusDataChanged(string data)
        {
            Debug.Log("게임 상태 변경됨: " + data);
            try
            {
                // 데이터가 문자열 형태일 수 있으므로 따옴표 제거
                string gameStatus = data.Replace("\"", "");
                _localSessionData.gameStatus = gameStatus;
                
                // 게임 상태 관련 UI 업데이트나 게임 로직 처리
                Debug.Log($"게임 상태: {gameStatus}");
                OnGameStatusChanged?.Invoke(gameStatus);
            }
            catch (Exception ex)
            {
                Debug.LogError("게임 상태 파싱 오류: " + ex.Message);
            }
        }
        
        public void OnSessionListenerError(string error)
        {
            Debug.LogError("세션 리스너 오류: " + error);
        }
        
        /// <summary>
        /// 세션 리스너 제거
        /// </summary>
        public void RemoveAllSessionListeners()
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
        
        public void OnListenerRemoved(string data)
        {
            Debug.Log("리스너 제거됨: " + data);
        }
        
        #endregion
        
        [Serializable]
        private class CreateSessionResponse
        {
            public bool success;
            public string sessionCode;
            public SessionData sessionData;
        }
    }
}