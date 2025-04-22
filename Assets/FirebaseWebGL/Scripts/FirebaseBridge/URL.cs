using System.Runtime.InteropServices;

namespace FirebaseWebGL.Scripts.FirebaseBridge
{
    public static class URL
    {
        /// <summary>
        /// 외부 URL을 새 창에서 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 URL</param>
        [DllImport("__Internal")]
        public static extern void OpenURL(string url);

        /// <summary>
        /// 외부 URL을 같은 창에서 엽니다.
        /// </summary>
        /// <param name="url">열고자 하는 URL</param>
        [DllImport("__Internal")]
        public static extern void OpenURLInSameWindow(string url);

        [DllImport("__Internal")]
        public static extern void GetQRCodeByURL(string url, string size, string callbackGameObject, string callbackMethod);

        [DllImport("__Internal")]
        public static extern void CopyURLToClipboard(string text);
    }
}
