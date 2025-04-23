using UnityEngine;

namespace UI.Session.Host
{
    public class URLCopyComplete : MonoBehaviour
    {
        public void ShowCopyCompleteNotification()
        {
            gameObject.SetActive(true);
            Invoke("HideCopyNotification", 2f);
        }

        void HideCopyNotification()
        {
            gameObject.SetActive(false);
        }
    }
}
