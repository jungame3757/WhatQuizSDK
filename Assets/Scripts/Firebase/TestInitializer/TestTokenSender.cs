using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Scripts.Objects;
using FirebaseWebGL.Examples.Utils;
using TMPro;
using UnityEngine;

public class TestTokenSender : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField emailInputField;
    
    [SerializeField]
    private TMP_InputField passwordInputField;
    
    [SerializeField]
    private TextMeshProUGUI outputText;
    
    [SerializeField]
    private string externalUrl = "https://your-external-url.com";

    private void Start()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            DisplayError("이 코드는 WebGL 빌드에서만 작동합니다. JavaScript 함수가 인식되지 않을 수 있습니다.");
            return;
        }
        
        // 인증 상태 변경 감지
        FirebaseAuth.OnAuthStateChanged(gameObject.name, "HandleAuthStateChanged", "DisplayInfo");
    }

    public void SignInWithEmailAndPassword()
    {
        if (string.IsNullOrEmpty(emailInputField.text) || string.IsNullOrEmpty(passwordInputField.text))
        {
            DisplayError("이메일과 비밀번호를 모두 입력해주세요.");
            return;
        }

        FirebaseAuth.SignInWithEmailAndPassword(
            emailInputField.text, 
            passwordInputField.text, 
            gameObject.name, 
            "OnSignInSuccess", 
            "DisplayErrorObject"
        );
    }

    public void HandleAuthStateChanged(string user)
    {
        if (string.IsNullOrEmpty(user) || user == "null")
        {
            DisplayInfo("로그인되지 않았습니다.");
            return;
        }

        try
        {
            var parsedUser = StringSerializationAPI.Deserialize(typeof(FirebaseUser), user) as FirebaseUser;
            DisplayData($"로그인 성공: {parsedUser.email}, 사용자 ID: {parsedUser.uid}");
        }
        catch (System.Exception e)
        {
            DisplayError($"사용자 정보 파싱 오류: {e.Message}");
        }
    }

    public void OnSignInSuccess(string info)
    {
        DisplayInfo("로그인 성공! 인증 토큰을 가져오는 중...");
        
        // 로그인 후 인증 토큰 가져오기
        FirebaseAuth.GetIdToken(false, gameObject.name, "SendTokenToExternalUrl", "DisplayErrorObject");
    }

    public void SendTokenToExternalUrl(string token)
    {
        DisplayInfo($"인증 토큰을 외부 URL로 전송 중: {externalUrl}");
        
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // WebGL에서는 URL 클래스의 메서드 호출
            URL.OpenUrlWithToken(externalUrl, token);
        }
        else
        {
            // 다른 플랫폼에서는 URL 형태로 토큰 추가하여 연결
            string urlWithToken = $"{externalUrl}?token={token}";
            Application.OpenURL(urlWithToken);
        }
    }

    public void DisplayData(string data)
    {
        outputText.color = Color.green;
        outputText.text = data;
        Debug.Log(data);
    }

    public void DisplayInfo(string info)
    {
        outputText.color = Color.white;
        outputText.text = info;
        Debug.Log(info);
    }

    public void DisplayErrorObject(string error)
    {
        try
        {
            var parsedError = StringSerializationAPI.Deserialize(typeof(FirebaseError), error) as FirebaseError;
            DisplayError(parsedError.message);
        }
        catch (System.Exception)
        {
            DisplayError(error);
        }
    }

    public void DisplayError(string error)
    {
        outputText.color = Color.red;
        outputText.text = error;
        Debug.LogError(error);
    }
}
