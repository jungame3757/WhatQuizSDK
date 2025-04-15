# Firebase 인증 토큰 연동 가이드

이 가이드는 Firebase 인증을 사용하여 사용자가 로그인하고 인증 토큰을 외부 URL로 전송하는 방법을 설명합니다.

## 사용 방법

1. 유니티 씬에 새로운 GameObject를 추가하고 `AuthTokenHandler` 스크립트를 연결합니다.

2. Inspector에서 다음 값들을 설정합니다:
   - `Email Input Field`: 이메일 입력을 위한 TMP_InputField
   - `Password Input Field`: 비밀번호 입력을 위한 TMP_InputField
   - `Output Text`: 결과 표시를 위한 TextMeshProUGUI
   - `External Url`: 인증 토큰을 전송할 외부 URL

3. 로그인 버튼에 `AuthTokenHandler.SignInWithEmailAndPassword()` 메서드를 OnClick 이벤트로 등록합니다.

## 작동 방식

1. 사용자가 이메일과 비밀번호를 입력하고 로그인 버튼을 클릭하면, Firebase 인증 시스템을 통해 로그인이 진행됩니다.

2. 로그인이 성공하면 Firebase에서 인증 토큰을 가져옵니다.

3. 인증 토큰은 `externalUrl` 매개변수에 지정된 URL로 전송됩니다.

4. WebGL 빌드에서는 새 창이 열리고, 토큰이 URL 파라미터로 추가됩니다(예: `https://your-external-url.com?token=YOUR_TOKEN`).

## 주의사항

1. 이 기능은 WebGL 빌드에서만 완전히 작동합니다.

2. 프로젝트의 Firebase 설정이 올바르게 구성되어 있어야 합니다.

3. 실제 프로덕션 환경에서는 보안을 위해 토큰 교환 방식을 더욱 안전하게 구현해야 합니다.

## JavaScript 플러그인

토큰 획득 및 URL 리디렉션을 위한 JavaScript 플러그인은 `Assets/FirebaseWebGL/Plugins/firebaseauthtoken.jslib`에 구현되어 있습니다. 