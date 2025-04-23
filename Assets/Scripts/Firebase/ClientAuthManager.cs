using UnityEngine;
using FirebaseWebGL.Scripts.FirebaseBridge;
using System;

namespace Firebase
{
    public class ClientAuthManager : MonoBehaviour
    {
        // 싱글톤 인스턴스
        public static ClientAuthManager Instance { get; private set; }

        // 인증 상태 및 사용자 정보
        private bool _isAuthenticated = false;
        private string _userId = string.Empty;
        
        // 인증 완료 이벤트
        public event Action<string> OnAuthenticationComplete;
        public event Action<string> OnAuthenticationFailed;

        // 현재 인증 상태 확인
        public bool IsAuthenticated => _isAuthenticated;
        public string UserId => _userId;

        private void Awake()
        {
            // 싱글톤 설정
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        // 시작 시 자동으로 익명 로그인 진행
        void Start()
        {
            Debug.Log("ClientAuthManager: 익명 인증 시작...");
            SignInAnonymously();
        }

        // 익명 로그인 메서드
        public void SignInAnonymously()
        {
            UI.UISystem.DebugManager.Instance.ShowLoading();

            try
            {
                Debug.Log("익명 로그인 시도 중...");
                FirebaseAuth.SignInAnonymously(gameObject.name, "OnSignInSuccess", "OnSignInFailure");
            }
            catch (Exception e)
            {
                Debug.LogError($"익명 로그인 시도 중 오류 발생: {e.Message}");
                OnAuthenticationFailed?.Invoke("내부 오류가 발생했습니다.");
                UI.UISystem.DebugManager.Instance.ShowMassage("로그인 실패", e.Message, RedirectToHome, RedirectToHome);
            }

            UI.UISystem.DebugManager.Instance.HideLoading();
        }

        // 로그인 성공 콜백
        public void OnSignInSuccess(string userId)
        {
            _isAuthenticated = true;
            _userId = userId;
            
            Debug.Log($"익명 로그인 성공. 사용자 ID: {userId}");
            OnAuthenticationComplete?.Invoke(userId);
        }

        // 로그인 실패 콜백
        public void OnSignInFailure(string error)
        {
            _isAuthenticated = false;
            _userId = string.Empty;
            
            Debug.LogError($"익명 로그인 실패: {error}");
            OnAuthenticationFailed?.Invoke(error);
            UI.UISystem.DebugManager.Instance.ShowMassage("로그인 실패", error, RedirectToHome, RedirectToHome);
        }
        
        // 현재 사용자의 ID 토큰 반환
        public string GetIdToken()
        {
            return _userId;
        }
        
        // 인증 상태 변경 감지
        public void InitializeAuthStateListener()
        {
            Debug.Log("인증 상태 변경 리스너 초기화");
            FirebaseAuth.OnAuthStateChanged(gameObject.name, "OnUserSignedIn", "OnUserSignedOut");
        }
        
        // 사용자 로그인 상태 변경 콜백
        public void OnUserSignedIn(string userData)
        {
            Debug.Log($"사용자 로그인 상태 변경: {userData}");
            _isAuthenticated = true;
            _userId = userData; // 실제 구현에서는 userData 객체에서 ID 추출 필요
        }
        
        // 사용자 로그아웃 상태 변경 콜백
        public void OnUserSignedOut(string message)
        {
            Debug.Log("사용자 로그아웃됨");
            _isAuthenticated = false;
            _userId = string.Empty;

            RedirectToHome();
        }

        private void RedirectToHome()
        {
            URL.OpenURLInSameWindow("https://whatquiz.store/");
        }
    }
}
