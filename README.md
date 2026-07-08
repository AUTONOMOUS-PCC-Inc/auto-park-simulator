# Auto Park Simulator 🚗

Unity 6로 제작한 **자동 주차(AutoPark) 시뮬레이션 데모**입니다.
차량이 미리 정의된 경로(웨이포인트)를 따라 장애물 차들 사이로 스스로 주차하는 과정을 보여주며, WebGL로 빌드해 웹 브라우저에서 바로 시연할 수 있습니다.

> 화면 타이틀: **PMS View** (Parking Management System)

---

## ✨ 주요 기능

- 🅿️ **4가지 주차 시나리오** — 키 `1`~`4`로 서로 다른 주차 상황을 실행
- 🧭 **웨이포인트 기반 자동 주행** — 경로를 따라 회전·전진·후진하며 이동
- 🚧 **시나리오별 장애물 차량 배치** — 시나리오마다 주변 차량 위치가 자동 세팅
- 🎥 **카메라 전환** — 팔로우 뷰 ↔ 탑(위에서 내려보는) 뷰 (`V` 키)
- 📱 **반응형 카메라** — 화면 비율(PC 가로 / 모바일 세로)에 맞춰 FOV·높이 자동 조정
- 📈 **경로 시각화** — `LineRenderer`로 바닥에 주행 경로 표시

---

## 🎮 조작법

| 키 | 동작 |
|:--:|------|
| `1` ~ `4` | 주차 시나리오 1~4 실행 |
| `0` | 시작 위치(EntryPoint)로 리셋 |
| `V` | 카메라 뷰 전환 (팔로우 ↔ 탑뷰) |

> WebGL 화면을 한 번 클릭해 포커스를 준 뒤 키를 눌러야 반응합니다.

---

## 🛠️ 요구 사항

- **Unity 6000.3.1f1** (Unity 6) — [Unity Hub](https://unity.com/download)에서 설치
- **Universal Render Pipeline (URP)** — 프로젝트에 포함됨
- WebGL 빌드를 로컬에서 실행하려면 **Python 3.x**

---

## 🚀 시작하기

### 1. 클론

```bash
git clone https://github.com/YUNSUNGWOONG/auto-park-simulator.git
cd auto-park-simulator
```

### 2. Unity에서 열기

1. **Unity Hub** 실행 → `Add` → 클론한 폴더 선택
2. 에디터 버전이 **6000.3.1f1**로 맞는지 확인 (다르면 해당 버전 설치)
3. 프로젝트를 열면 `Assets/Scenes/SampleScene.unity`가 메인 씬입니다
4. 상단 ▶ **Play** 버튼으로 실행 → `1`~`4` 키로 시나리오 확인

> 최초 실행 시 Unity가 `Library/` 등을 다시 생성하므로 몇 분 정도 걸릴 수 있습니다.

---

## 🌐 WebGL로 빌드하고 실행하기

### 빌드

1. Unity에서 `File > Build Profiles` (또는 `Build Settings`) 열기
2. 플랫폼을 **WebGL**로 선택 → `Switch Platform`
3. `Build` 클릭 → 출력 폴더를 **`autopark/`** 로 지정
   - 빌드 결과(`autopark.loader.js`, `autopark.wasm.gz` 등)가 `autopark/Build/`에 생성됩니다

### 로컬 서버로 실행

Unity WebGL은 gzip 압축 파일을 올바른 헤더로 서빙해야 하므로, 저장소에 포함된 `server.py`를 사용하세요.

```bash
cd autopark
python server.py --port 8080
```

브라우저에서 접속:

```
http://localhost:8080/
```

> `server.py`는 `.js.gz`, `.wasm.gz`, `.data.gz`에 알맞은 `Content-Type`과 `Content-Encoding: gzip` 헤더를 자동으로 붙여줍니다.

---

## 📁 프로젝트 구조

```
auto-park-simulator/
├── Assets/
│   ├── Scenes/SampleScene.unity      # 메인 씬
│   ├── Scripts/                      # 게임 로직 (아래 참고)
│   ├── Azerilo/                      # 차량 3D 모델 에셋
│   ├── Materials/ · Images/ · ...    # 리소스
│   └── ...
├── Packages/manifest.json            # 패키지 의존성 (URP, Input System 등)
├── ProjectSettings/                  # Unity 프로젝트 설정
├── autopark/
│   ├── server.py                     # WebGL용 gzip 지원 로컬 서버
│   ├── index.html                    # WebGL 호스팅 페이지
│   └── Build/                        # (git 제외) WebGL 빌드 산출물
└── README.md
```

### 스크립트 개요 (`Assets/Scripts/`)

| 파일 | 역할 |
|------|------|
| `AutoParkController.cs` | 핵심 컨트롤러. 시나리오 실행, 차량 배치, 웨이포인트 경로 주행, 경로 시각화 |
| `AutoParkWaypoint.cs` | 각 경유점 옵션 — `reverse`(후진 구간), `waitSeconds`(도착 후 대기) |
| `CameraToggle.cs` | `V` 키로 팔로우/탑 뷰 전환 |
| `FollowCam.cs` | 차량을 부드럽게 따라가는 추적 카메라 |
| `FitPerspectiveCameraByAspect.cs` | 화면 비율에 맞춰 카메라 FOV·높이 자동 조정 |

---

## 🧩 시나리오 커스터마이징

하나의 시나리오는 3가지 요소로 구성됩니다.

1. **주행 경로** — `Waypoints1`~`Waypoints4` 부모 아래에 빈 오브젝트(`WP01`, `WP02` …)를 순서대로 배치
   - 이름은 두 자리 숫자(`WP01`, `WP02`)로 지어야 자동 정렬 순서가 꼬이지 않습니다
   - 경로 맨 앞에는 `EntryPoint`가 자동으로 추가됩니다
   - 마지막 웨이포인트의 위치·회전이 최종 주차 자세가 됩니다
2. **후진/대기 구간** — 해당 웨이포인트에 `AutoParkWaypoint` 컴포넌트를 붙이고 `reverse` 체크 또는 `waitSeconds` 설정
3. **장애물 차량 배치** — `CarPose1`~`CarPose4` 아래 pose 오브젝트로 차량 위치 지정 (비워두면 그 시나리오에선 해당 차량 비활성화)

마지막으로 `AutoParkController` 인스펙터에서 `Waypoints Root`, `CarSet(s1~s4)`, `EgoRoot`, `EntryPoint` 등을 연결하면 됩니다.

Move 파라미터로 주행감 조절:
- `speed` — 이동 속도
- `turnSpeed` — 회전 반응 속도
- `arriveDist` — 웨이포인트 도착 판정 거리

---

## 📄 라이선스

별도 명시가 없는 한 학습·데모 목적으로 자유롭게 사용하세요.
`Assets/Azerilo`의 차량 모델 등 서드파티 에셋은 각 제공처의 라이선스를 따릅니다.
