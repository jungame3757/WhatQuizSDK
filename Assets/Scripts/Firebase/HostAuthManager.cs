using System;
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Scripts.Objects;
using UnityEngine;
using FullSerializer;

namespace Firebase
{
    public class HostAuthManager : MonoBehaviour
    {
        public static HostAuthManager Instance;

        public string idToken;
        private fsSerializer _serializer;
        private FirebaseUser _currentUser;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _serializer = new fsSerializer();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            InitializeFirebaseAuth();
        }

        public Action OnUserSignedIn;
        public Action OnUserSignedOut;
        
        private void InitializeFirebaseAuth()
        {
            FirebaseAuth.OnAuthStateChanged(gameObject.name, "UserSignedIn", "UserSignedOut");
        }

        private void UserSignedIn(string userJson)
        {
            Debug.Log("사용자 로그인 상태: " + userJson);

            try
            {
                // FullSerializer를 사용하여 사용자 정보 파싱
                fsData data = fsJsonParser.Parse(userJson);
                _currentUser = null;
                fsResult result = _serializer.TryDeserialize(data, ref _currentUser);

                if (result.Succeeded && _currentUser != null && _currentUser.stsTokenManager != null)
                {
                    // 정상적으로 토큰 추출 성공
                    idToken = _currentUser.stsTokenManager.accessToken;
                    Debug.Log("ID 토큰 추출 성공: " + idToken.Substring(0, 20) + "...");
                }
                else
                {
                    // 토큰 추출 실패 시 전체 JSON 저장 (기존 방식)
                    Debug.LogWarning("토큰 추출 실패: " + result.FormattedMessages);
                    idToken = userJson;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("토큰 파싱 오류: " + ex.Message);
                idToken = userJson;
            }

            OnUserSignedIn?.Invoke();
        }

        private void UserSignedOut(string message)
        {
            Debug.Log("사용자 로그아웃 상태: " + message);
            idToken = null;
            _currentUser = null;
            OnUserSignedOut?.Invoke();
            RedirectToLogin();
        }

        private void RedirectToLogin()
        {
            URL.OpenURLInSameWindow("https://whatquiz.store/");
        }
        
        /// <summary>
        /// 현재 인증된 사용자의 ID 토큰을 반환합니다.
        /// </summary>
        /// <returns>ID 토큰 또는 null</returns>
        public string GetIdToken()
        {
            return idToken;
        }
    }
}
