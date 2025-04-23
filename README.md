# WhatQuiz SDK

실시간 퀴즈 게임을 쉽게 구현할 수 있는 Unity용 SDK입니다. Firebase 기반 멀티플레이어 세션 관리 시스템을 통해 호스트와 클라이언트 간의 원활한 통신을 지원합니다.

## 특징

- Firebase Realtime Database를 활용한 실시간 세션 관리
- 호스트-클라이언트 구조의 멀티플레이어 시스템
- 플레이어 참가/퇴장 자동 관리
- 다양한 게임 모드 지원 (시간제한, 점수제한)
- QR 코드 기반 세션 초대 시스템
- 플레이어 강퇴 기능

## 시작하기

### 필수 조건

- Unity 2020.3 이상
- Firebase Unity SDK (Realtime Database, Authentication)
- TextMesh Pro

### 설치

1. 이 저장소를 클론하거나 다운로드합니다.
2. Firebase 프로젝트를 생성하고 Unity 프로젝트에 연결합니다.
3. `Assets/Scripts/Firebase` 폴더의 스크립트를 프로젝트에 추가합니다.
4. `Assets/Scripts/UI/Session` 폴더의 UI 스크립트를 필요에 따라 추가합니다.

## 기본 사용법

### 호스트 측 사용법

1. 씬에 `HostAuthManager`와 `HostSessionManager` 컴포넌트가 있는 GameObject를 추가합니다.
2. UI는 `HostSessionUIManager`를 참조하여 구현합니다.
3. 세션 생성 시 `GameSettingData`를 설정합니다.

```csharp
// 게임 설정 구성
GameSettingData gameSettings = new GameSettingData
{
    gameMode = "time",        // "time" 또는 "score"
    timeLimit = 5,            // 시간제한 (분)
    scoreLimit = 100,         // 점수제한
    questionCount = 10        // 질문 수
};

// 세션 생성
hostSessionManager.CreateSession(gameSettings);
```

4. 이벤트 핸들러를 등록하여 세션 상태 변화를 처리합니다.

```csharp
// 이벤트 등록
hostSessionManager.OnSessionCreated += OnSessionCreated;
hostSessionManager.OnPlayersChanged += OnPlayersChanged;
hostSessionManager.OnSessionStatusChanged += OnSessionStatusChanged;
hostSessionManager.OnGameStatusChanged += OnGameStatusChanged;
```

5. 플레이어 강퇴는 다음과 같이 처리합니다.

```csharp
// 플레이어 강퇴
hostSessionManager.KickPlayer(playerId);
```

### 클라이언트 측 사용법

1. 씬에 `ClientAuthManager`와 `ClientSessionManager` 컴포넌트가 있는 GameObject를 추가합니다.
2. UI는 `ClientSessionUIManager`를 참조하여 구현합니다.
3. 세션 참가시 세션 코드와 플레이어 이름이 필요합니다.

```csharp
// 세션 코드와 플레이어 이름으로 참가
clientSessionManager.JoinSession(sessionCode, playerName);
```

4. 이벤트 핸들러를 등록하여 세션 상태 변화를 처리합니다.

```csharp
// 이벤트 등록
clientSessionManager.OnSessionJoined += OnSessionJoined;
clientSessionManager.OnPlayersChanged += OnPlayersChanged;
clientSessionManager.OnSessionStatusChanged += OnSessionStatusChanged;
clientSessionManager.OnGameStatusChanged += OnGameStatusChanged;
clientSessionManager.OnPlayerKicked += OnPlayerKicked;
```

## 주요 클래스 및 기능

### 공통

- `SessionData`: 세션 정보를 포함하는 클래스
- `Player`: 플레이어 정보를 포함하는 클래스
- `GameSettingData`: 게임 설정 정보를 포함하는 클래스

### 호스트 측

- `HostAuthManager`: 호스트 인증 관리
- `HostSessionManager`: 호스트 세션 생성 및 관리
  - `CreateSession(GameSettingData)`: 새 세션 생성
  - `KickPlayer(playerId)`: 플레이어 강퇴
- `HostSessionUIManager`: 호스트 UI 관리

### 클라이언트 측

- `ClientAuthManager`: 클라이언트 인증 관리
- `ClientSessionManager`: 클라이언트 세션 참가 및 관리
  - `JoinSession(sessionCode, playerName)`: 세션 참가
  - `LeaveSession()`: 세션 나가기
  - `RedirectToHome()`: 메인 페이지로 이동
- `ClientSessionUIManager`: 클라이언트 UI 관리

## 이벤트 시스템

### HostSessionManager 이벤트

- `OnSessionCreated`: 세션 생성 시 발생
- `OnPlayersChanged`: 플레이어 목록 변경 시 발생
- `OnSessionStatusChanged`: 세션 상태 변경 시 발생
- `OnGameStatusChanged`: 게임 상태 변경 시 발생
- `OnError`: 오류 발생 시 발생

### ClientSessionManager 이벤트

- `OnSessionJoined`: 세션 참가 시 발생
- `OnPlayersChanged`: 플레이어 목록 변경 시 발생
- `OnSessionStatusChanged`: 세션 상태 변경 시 발생
- `OnGameStatusChanged`: 게임 상태 변경 시 발생
- `OnPlayerKicked`: 플레이어 강퇴 시 발생
- `OnError`: 오류 발생 시 발생

## 주의사항

1. Firebase 프로젝트 설정을 올바르게 구성해야 합니다.
2. 호스트와 클라이언트 모두 인증이 필요합니다.
3. 세션 코드는 URL 매개변수로 전달해야 합니다. 예: `https://yourdomain.com/?code=ABC123`
4. 플레이어 강퇴 시 데이터베이스에서 플레이어 데이터가 삭제되며, 강퇴된 플레이어는 세션에 다시 참가할 수 있습니다.

## Firebase 데이터베이스 구조

```
/sessionCodes
  /[sessionCode]
    /gameStatus: "waiting"|"playing"|"finished"
    /sessionStatus: "waiting"|"playing"|"finished"
    /hostId: "host_user_id"
    /expiresAt: timestamp
    /gameSetting
      /gameMode: "time"|"score"
      /timeLimit: number
      /scoreLimit: number
      /questionCount: number
    /players
      /[playerId]
        /id: "player_id"
        /name: "player_name"
        /isReady: boolean
        /score: number
```

## 라이센스

이 프로젝트는 MIT 라이센스로 배포됩니다. 자세한 내용은 LICENSE 파일을 참조하세요. 