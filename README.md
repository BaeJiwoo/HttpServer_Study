# ASP.NET Core HTTP 게임 서버 학습 프로젝트

ASP.NET Core Minimal API를 사용하여 HTTP 기반 게임 서버의 기본 구조를 단계적으로 학습하는 프로젝트입니다.

로그인, 액세스 토큰 인증, 플레이어 조회, 로그아웃, 스테이지 보상 지급 및 동시성 처리까지 구현하는 것을 목표로 합니다.

## 개발 환경

- Windows
- .NET 9
- ASP.NET Core Minimal API
- C#
- Kestrel
- CMD
- curl

## 실행 방법

프로젝트 디렉터리에서 다음 명령을 실행합니다.

```cmd
dotnet run --no-launch-profile -- --urls http://localhost:5000
```

서버가 정상적으로 실행되면 다음 주소에서 요청을 기다립니다.

```text
http://localhost:5000
```

실행 로그 예시:

```text
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

## 현재 진행 상황

- 현재 레벨: **14 / 20**
- 전체 진행도: **70%**
- 다음 목표: **인증 코드 구조 분리**

## 학습 로드맵

| 레벨 | 달성 목표 | 관련해서 알아볼 것 | 목표 달성 확인 | 상태 |
|---:|---|---|---|---|
| 1 | .NET 개발 환경 확인 | .NET SDK, Runtime, `dotnet` CLI | `dotnet --version`에서 .NET 9 확인 | 완료 |
| 2 | ASP.NET Core 프로젝트 생성 | `.csproj`, `Program.cs`, `appsettings.json` | 프로젝트 생성 및 빌드 성공 | 완료 |
| 3 | HTTP 서버 실행 | Kestrel, localhost, 포트, launch profile | `localhost:5000` 접속 성공 | 완료 |
| 4 | JSON 응답 생성 | `MapGet`, `Results.Ok`, JSON 직렬화 | 브라우저에서 JSON 응답 확인 | 완료 |
| 5 | HTTP Route 이해 | GET, Route, Route Parameter, Route Constraint | URL에 따라 Endpoint가 분기됨 | 완료 |
| 6 | 로그인 API 구현 | POST, JSON Body, `Content-Type` | `POST /login` 호출 성공 | 완료 |
| 7 | HTTP 상태 코드 구분 | 200, 400, 401, 404 | 요청 상황별 상태 코드 확인 | 완료 |
| 8 | 로그인 입력값 검증 | DTO, `record`, 모델 바인딩 | 빈 입력에서 `400 Bad Request` 반환 | 완료 |
| 9 | 액세스 토큰 발급 | `RandomNumberGenerator`, Opaque Token | 로그인 응답에서 난수 토큰 확인 | 완료 |
| 10 | Bearer 토큰 인증 | `Authorization` Header, Bearer Scheme | 토큰으로 `/me` 호출 성공 | 완료 |
| 11 | 토큰과 플레이어 연결 | AccessToken, LoginSession, PlayerId | 토큰에서 PlayerId 조회 성공 | 완료 |
| 12 | 토큰 만료와 로그아웃 | `ExpiresAt`, UTC, 세션 삭제 | 만료되거나 로그아웃한 토큰에서 401 반환 | 완료 |
| 13 | 플레이어 저장소 구현 | PlayerId, Player, 메모리 저장소 | `/me`에서 플레이어 정보 반환 | 완료 |
| 14 | 스테이지 보상과 동시성 처리 | 서버 주도 보상, `TryUpdate`, 갱신 손실 | 동시 요청에서도 골드가 정확히 증가 | 완료 |
| 15 | 인증 코드 구조 분리 | 인증 함수, Middleware, 중복 제거 | Endpoint의 인증 중복 코드 제거 | 예정 |
| 16 | Service와 Repository 분리 | 비즈니스 로직, 데이터 접근 책임 | `Program.cs`에서 게임 로직 분리 | 예정 |
| 17 | Dependency Injection 적용 | Singleton, Scoped, Transient | 서비스를 DI로 주입 | 예정 |
| 18 | 데이터베이스 연결 | EF Core, SQLite, `DbContext`, Migration | 서버 재시작 후 데이터 유지 | 예정 |
| 19 | 계정 및 게임 데이터 영속화 | 비밀번호 해시, 트랜잭션, 중복 보상 방지 | DB 기반 로그인과 보상 처리 | 예정 |
| 20 | 테스트와 배포 준비 | 통합 테스트, HTTPS, 로깅, Docker | 배포 환경에서 API 동작 확인 | 예정 |

## 구현된 API

### 서버 상태 확인

```http
GET /
```

응답 예시:

```json
{
  "message": "Game Server is running",
  "serverTime": "2026-08-01T04:00:00Z"
}
```

### 로그인

```http
POST /login
Content-Type: application/json
```

요청 Body:

```json
{
  "username": "player1234",
  "password": "1234"
}
```

성공 응답:

```json
{
  "message": "로그인에 성공했습니다.",
  "accessToken": "ACCESS_TOKEN",
  "tokenType": "Bearer",
  "expiresAt": "2026-08-01T04:10:00Z"
}
```

CMD 호출 예시:

```cmd
curl.exe -X POST http://localhost:5000/login -H "Content-Type: application/json" -d "{\"username\":\"player1234\",\"password\":\"1234\"}"
```

### 내 플레이어 정보 조회

```http
GET /me
Authorization: Bearer ACCESS_TOKEN
```

응답 예시:

```json
{
  "id": 1,
  "username": "player1234",
  "nickname": "Knight",
  "level": 1,
  "gold": 1000,
  "tokenExpiresAt": "2026-08-01T04:10:00Z"
}
```

CMD 호출 예시:

```cmd
curl.exe -i http://localhost:5000/me -H "Authorization: Bearer %TOKEN%"
```

### 로그아웃

```http
POST /logout
Authorization: Bearer ACCESS_TOKEN
```

응답 예시:

```json
{
  "message": "로그아웃되었습니다."
}
```

로그아웃하면 해당 액세스 토큰이 로그인 세션 저장소에서 제거됩니다.

### 스테이지 완료 보상

```http
POST /stages/{stageId}/complete
Authorization: Bearer ACCESS_TOKEN
```

CMD 호출 예시:

```cmd
curl.exe -i -X POST http://localhost:5000/stages/1/complete -H "Authorization: Bearer %TOKEN%"
```

응답 예시:

```json
{
  "message": "스테이지 보상을 받았습니다.",
  "stageId": 1,
  "rewardGold": 100,
  "currentGold": 1100
}
```

## 서버 동작 흐름

```text
게임 클라이언트
    ↓ HTTP Request
Kestrel 웹 서버
    ↓
ASP.NET Core Routing
    ↓
Endpoint 실행
    ↓
입력값 검증
    ↓
액세스 토큰 검증
    ↓
LoginSession 조회
    ↓
PlayerId 확인
    ↓
게임 로직 실행
    ↓
플레이어 데이터 변경
    ↓ HTTP Response
게임 클라이언트
```

## 인증 흐름

```text
Username + Password
        ↓
로그인 정보 검증
        ↓
보안 난수 액세스 토큰 생성
        ↓
AccessToken → LoginSession 저장
        ↓
클라이언트가 토큰 보관
        ↓
Authorization: Bearer AccessToken
        ↓
서버가 토큰과 만료 시간 검증
        ↓
PlayerId 획득
        ↓
플레이어 데이터 조회
```

## 메모리 저장 구조

### 로그인 세션

```text
AccessToken
└─ LoginSession
   ├─ PlayerId
   └─ ExpiresAt
```

현재 자료 구조:

```csharp
ConcurrentDictionary<string, LoginSession>
```

### 플레이어 데이터

```text
PlayerId
└─ Player
   ├─ Username
   ├─ Nickname
   ├─ Level
   └─ Gold
```

현재 자료 구조:

```csharp
ConcurrentDictionary<int, Player>
```

## 액세스 토큰

액세스 토큰은 플레이어 ID 자체가 아니라 예측하기 어려운 난수 문자열입니다.

```csharp
var accessToken = Convert.ToHexString(
    RandomNumberGenerator.GetBytes(32));
```

생성된 토큰은 서버 내부에서 플레이어 ID와 연결됩니다.

```text
AccessToken → LoginSession → PlayerId
```

클라이언트는 이후 요청에서 다음 Header를 전송합니다.

```http
Authorization: Bearer ACCESS_TOKEN
```

## 서버 주도 게임 로직

클라이언트가 플레이어 ID나 보상 금액을 직접 결정하지 않도록 구성합니다.

잘못된 설계:

```json
{
  "playerId": 1,
  "rewardGold": 999999
}
```

현재 설계:

```text
클라이언트
→ stageId만 전달

서버
→ 토큰으로 PlayerId 확인
→ stageId로 보상 결정
→ 플레이어 골드 갱신
```

이를 통해 클라이언트 조작으로 다른 플레이어의 데이터나 보상 금액을 변경하는 문제를 방지합니다.

## 동시성 처리

두 요청이 동시에 같은 플레이어의 골드를 변경하면 갱신 손실이 발생할 수 있습니다.

```text
현재 골드: 1000

요청 A가 1000 읽음
요청 B가 1000 읽음

요청 A가 1100 저장
요청 B가 1100 저장

기대 결과: 1200
잘못된 결과: 1100
```

이를 방지하기 위해 `ConcurrentDictionary.TryUpdate`를 사용합니다.

```csharp
while (true)
{
    if (!players.TryGetValue(
            session.PlayerId,
            out var currentPlayer))
    {
        return Results.NotFound();
    }

    var updatedPlayer = currentPlayer with
    {
        Gold = checked(currentPlayer.Gold + rewardGold)
    };

    if (players.TryUpdate(
            currentPlayer.Id,
            updatedPlayer,
            currentPlayer))
    {
        break;
    }
}
```

다른 요청이 먼저 데이터를 변경했다면 최신 값을 다시 읽어 계산합니다.

## 주요 보안 원칙

- 액세스 토큰은 충분히 긴 보안 난수로 생성한다.
- 플레이어 ID를 액세스 토큰으로 사용하지 않는다.
- 클라이언트가 플레이어 ID를 직접 지정하지 않게 한다.
- 클라이언트가 보상 금액을 직접 결정하지 않게 한다.
- 아이디와 비밀번호 중 무엇이 틀렸는지 구분해서 알려주지 않는다.
- 액세스 토큰에는 만료 시간을 설정한다.
- 로그아웃하면 로그인 세션을 제거한다.
- 실제 서비스에서는 반드시 HTTPS를 사용한다.
- 실제 비밀번호는 평문이 아닌 해시값으로 저장한다.
- 토큰 원문을 데이터베이스에 저장할 때는 토큰 해시 저장을 고려한다.

## 현재 프로젝트의 한계

- 플레이어와 세션이 서버 메모리에만 저장된다.
- 서버를 재시작하면 모든 데이터가 사라진다.
- 테스트 계정과 비밀번호가 코드에 직접 작성되어 있다.
- 비밀번호가 평문으로 비교된다.
- 인증 코드가 여러 Endpoint에서 반복된다.
- 모든 코드가 `Program.cs`에 모여 있다.
- 데이터베이스와 트랜잭션이 없다.
- 동일한 스테이지 보상을 반복해서 받을 수 있다.
- 자동화된 테스트가 없다.
- 현재 실습 환경은 HTTP를 사용하며 HTTPS가 적용되지 않았다.

## 다음 목표

### 인증 코드 구조 분리

현재 다음 코드가 여러 Endpoint에서 반복됩니다.

```text
Authorization Header 읽기
→ Bearer Token 추출
→ LoginSession 조회
→ 토큰 만료 검사
→ PlayerId 반환
```

이를 별도의 인증 함수 또는 서비스로 분리합니다.

목표 형태:

```csharp
var authResult = Authenticate(request);
```

이후에는 ASP.NET Core Authentication Middleware와 `ClaimsPrincipal` 기반 구조로 발전시킬 예정입니다.
