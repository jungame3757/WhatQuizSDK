# FirebaseWebGL

Unity WebGL 환경에서 Firebase 서비스를 쉽게 사용할 수 있는 플러그인입니다.

## 목차

- [소개](#소개)
- [설치 방법](#설치-방법)
- [구조](#구조)
- [주요 모듈](#주요-모듈)
  - [FirebaseAuth](#firebaseauth)
  - [URL](#url)
- [콜백 시스템](#콜백-시스템)
- [사용 예제](#사용-예제)
- [주의사항](#주의사항)
- [에러 처리](#에러-처리)

## 소개

FirebaseWebGL은 Unity WebGL 빌드에서 Firebase의 다양한 기능(인증, 데이터베이스, 스토리지 등)을 쉽게 활용할 수 있게 해주는 플러그인입니다. JavaScript와 C# 사이의 원활한 통신을 지원하여 Unity 게임에서 Firebase 서비스를 사용할 수 있습니다.

## 설치 방법

1. FirebaseWebGL 패키지를 Unity 프로젝트의 Assets 폴더에 복사합니다.
2. Firebase 프로젝트를 설정하고 필요한 구성 파일을 연결합니다.
3. 초기화 스크립트를 통해 Firebase를 초기화합니다.

## 구조

```
FirebaseWebGL/
├── Plugins/            - JavaScript 연동 파일
│   ├── firebaseauth.jslib
│   ├── firebasedatabase.jslib
│   ├── firebasefirestore.jslib
│   ├── firebasestorage.jslib
│   └── url.jslib
├── Scripts/            - C# 스크립트
│   └── FirebaseBridge/ - Firebase 기능 인터페이스
│       ├── FirebaseAuth.cs
│       ├── FirebaseDatabase.cs
│       ├── FirebaseFirestore.cs
│       ├── FirebaseStorage.cs
│       └── URL.cs
└── Examples/           - 사용 예제
```

## 주요 모듈

### FirebaseAuth

사용자 인증 관련 기능을 제공합니다.

```csharp
// 익명 로그인
FirebaseAuth.SignInAnonymously(gameObject.name, "성공콜백", "실패콜백");

// 이메일/비밀번호 로그인
FirebaseAuth.SignInWithEmailAndPassword("이메일", "비밀번호", gameObject.name, "성공콜백", "실패콜백");

// 이메일/비밀번호로 계정 생성
FirebaseAuth.CreateUserWithEmailAndPassword("이메일", "비밀번호", gameObject.name, "성공콜백", "실패콜백");

// 소셜 로그인
FirebaseAuth.SignInWithGoogle(gameObject.name, "성공콜백", "실패콜백");
FirebaseAuth.SignInWithFacebook(gameObject.name, "성공콜백", "실패콜백");

// 커스텀 토큰 로그인
FirebaseAuth.SignInWithCustomToken("토큰", gameObject.name, "성공콜백", "실패콜백");

// 인증 상태 변경 감지
FirebaseAuth.OnAuthStateChanged(gameObject.name, "로그인콜백", "로그아웃콜백");

// ID 토큰 가져오기
FirebaseAuth.GetIdToken(새로고침여부, gameObject.name, "성공콜백", "실패콜백");
```

### URL

URL 및 외부 웹 페이지 접근 기능을 제공합니다.

```csharp
// 단순 URL 열기
URL.OpenURL("https://example.com");

// 인증 토큰과 함께 URL 열기
URL.OpenUrlWithToken("https://example.com", "인증토큰");
```

## 콜백 시스템

FirebaseWebGL은 비동기 작업을 처리하기 위해 콜백 메서드를 사용합니다:

```csharp
// 성공 콜백 예시
public void OnSuccess(string result) {
    Debug.Log("성공: " + result);
}

// 실패 콜백 예시
public void OnError(string error) {
    try {
        var parsedError = StringSerializationAPI.Deserialize(typeof(FirebaseError), error) as FirebaseError;
        Debug.LogError("오류: " + parsedError.message);
    } catch {
        Debug.LogError("오류: " + error);
    }
}
```

## 사용 예제

### 인증 토큰 활용 예시

```csharp
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Scripts.Objects;
using UnityEngine;

public class AuthExample : MonoBehaviour
{
    [SerializeField] private string externalUrl = "https://your-api.com";

    public void Login() {
        FirebaseAuth.SignInWithEmailAndPassword(
            "user@example.com", 
            "password", 
            gameObject.name, 
            "OnLoginSuccess", 
            "OnLoginError"
        );
    }

    public void OnLoginSuccess(string result) {
        Debug.Log("로그인 성공");
        FirebaseAuth.GetIdToken(false, gameObject.name, "SendTokenToAPI", "OnLoginError");
    }
    
    public void SendTokenToAPI(string token) {
        URL.OpenUrlWithToken(externalUrl, token);
        // 또는 UnityWebRequest로 API 요청 시 헤더에 토큰 추가
    }
    
    public void OnLoginError(string error) {
        Debug.LogError("로그인 실패: " + error);
    }
}
```

## 주의사항

> ⚠️ 다음 사항들을 꼭 유의해주세요:

1. **WebGL 전용**: 모든 함수는 WebGL 빌드에서만 작동합니다
2. **테스트 방법**: Editor에서는 오류가 발생할 수 있으므로 다음과 같이 조건부 로직을 사용하세요:
   ```csharp
   if (Application.platform == RuntimePlatform.WebGLPlayer) {
       // Firebase 관련 코드
   }
   ```
3. **콜백 함수**: 모든 콜백 함수는 `public`으로 선언해야 합니다
4. **초기화**: 실제 Firebase 프로젝트 설정 및 초기화가 필요합니다

## 에러 처리

오류 처리를 위한 표준 패턴:

```csharp
public void DisplayErrorObject(string error) {
    try {
        var parsedError = StringSerializationAPI.Deserialize(typeof(FirebaseError), error) as FirebaseError;
        Debug.LogError(parsedError.message);
    }
    catch {
        Debug.LogError(error);
    }
}
```

---

© 2023 FirebaseWebGL. 이 플러그인을 활용하여 Unity WebGL 게임에서 Firebase 서비스를 쉽게 사용하세요! 