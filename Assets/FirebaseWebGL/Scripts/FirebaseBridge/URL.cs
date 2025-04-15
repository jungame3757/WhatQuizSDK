using System.Runtime.InteropServices;

namespace FirebaseWebGL.Scripts.FirebaseBridge
{
    public static class URL
    {
        /// <summary>
        /// 외부 URL을 토큰과 함께 새 창에서 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 URL</param>
        /// <param name="token">함께 전송할 인증 토큰</param>
        [DllImport("__Internal")]
        public static extern void OpenUrlWithToken(string url, string token);
        
        /// <summary>
        /// 외부 URL을 새 창에서 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 URL</param>
        [DllImport("__Internal")]
        public static extern void OpenURL(string url);
    }
}
