# AI 스킬 랩 (Unity-AI-Skills-Lab)

Claude Code · Codex에 **Unity 공식 플러그인**을 연결하면 무엇이 달라지는지 Unity 안에서 눈으로 보는 수업용 예제 프로젝트입니다.
Unity 공식 플러그인은 **스킬 묶음**입니다. Unity CLI 프로그램은 따로 설치하고, 에디터 조종에는 CLI · Pipeline 패키지 · 열린 에디터가 추가로 필요합니다(MCP 도 따로).
강의 모드(용어 25개 · 분야 6개)가 함께 들어 있습니다.

> 비교 씬의 **A · 스킬 없이 흔히 나오는 결과(재현)** / **B · 스킬을 쓴 결과(재현)** 는
> 흔히 나오는 결과와 스킬을 쓴 결과를 **우리가 직접 재현한 것**입니다. AI가 실제로 만든 결과가 아닙니다.

## 환경

| 항목 | 값 |
|---|---|
| Unity | 플러그인 최소 조건: **Unity 6.0 이상** · 이 저장소를 그대로 열려면: **6000.6.7f1 + URP** (PC_RPAsset + 0303 용 URP 2D 렌더러) |
| 패키지 | URP 17.6 · Input System · UI Toolkit(내장) · uGUI/TextMeshPro · AI Navigation 2.0.16 · 2D Sprite |
| 한글 글꼴 | Noto Sans KR Regular (`Assets/06_Fonts`, SIL Open Font License — `NotoSansKR - OFL.txt`) |
| 저장소 | https://github.com/jes79/Unity-AI-Skills-Lab (브랜치 `main`, Unity 프로젝트 = 저장소 루트) |

## 씬 목록 (`Assets/00_Scenes`)

비교 씬은 **1 = A · 2 = B · Tab = A ↔ B**. 화면 위에 지금 어느 쪽인지와 원인 한 줄이 나옵니다.

| 씬 | 용어 | 스킬 | A (흔한 결과, 재현) | B (스킬을 쓴 결과, 재현) |
|---|---|---|---|---|
| `00_SkillMap` | 0101~0105 · 0201~0204 · 0601~0604 | — | 그림 지도: 사람 → AI(Claude Code/Codex) → 플러그인(스킬 책) · CLI(손) · MCP(전화선) → Unity 에디터. 용어마다 칸 강조 · 설치 패널 · 조심할 점 (← → 용어, Space 단계) | |
| `0301_UIRouter` | 0301 | `/ui` | UI Toolkit HUD 위에 uGUI Canvas 설정 창 (모양·글꼴 다름, 고정 픽셀이라 겹침) | 같은 UI Toolkit(UXML) + HUD 테마(HudTheme.uss) 설정 화면 |
| `0302_ButtonTransition` | 0302 | `/ui-uitk` | `ButtonTransition_A.uss`: transition 이 `:hover` 에만 → 뗄 때 툭 | `ButtonTransition_B.uss`: transition 이 기본 클래스에 → 양쪽 부드러움 |
| `0303_PixelPerfect` | 0303 | `/2d-pixel-perfect` | Bilinear · 압축 · PPU 13(타일 16) · 카메라 소수점 추적 | Point · 압축 없음 · PPU 16 · URP 2D Pixel Perfect Camera (240x135) |
| `0304_KoreanFont` | 0304 | `/optimize-text-mesh-pro` | TMP 기본 글꼴 LiberationSans SDF 만 → 한글 □ | LiberationSans SDF 복사본 + Noto Sans KR 동적 Fallback |
| `0401_Bloom` | 0401 | `/urp-postprocessing` | Global Volume 에 Bloom 있음, 카메라 Post Processing 꺼짐 | 같은 Volume, 카메라 Post Processing 켬 |
| `0402_FastBullet` | 0402 | `/physics-3d-collision` | 총알 Rigidbody `Discrete` → 0.1m 벽 뚫음 (Space 발사, ↑↓ 속도) | `ContinuousDynamic` → 이 데모 속도(최대 250 m/s)에서는 지나온 길을 검사해 벽을 놓치지 않음 (실제 게임은 크기·속도·물리 설정에 따라 확인 필요) |
| `0403_Trigger` | 0403 | `/physics-3d-collision` | 플레이어·동전 모두 Collider 만 → `OnTriggerEnter` 0번 | 움직이는 플레이어에 Kinematic Rigidbody → 불림 |
| `0404_NavMesh` | 0404 | `/initialize-ai-navigation` | 옛 방식 안내만(Static 표시) → NavMesh 없음 → 에이전트 경고 후 멈춤 | NavMeshSurface 로 구운 NavMesh → 클릭한 곳으로 장애물을 돌아감 |
| `0501_ClassRules` | 0501~0504 | `class-rules` (이 저장소 스킬) | 탭: SKILL.md 보기 · A 결과 코드(`ScoreManagerA.cs`) · B 결과 코드(`ScoreCounterB.cs`) · 실행 결과 (← → 탭, Space 점수) | |

씬은 코드로 다시 만들 수 있습니다: **Tools > Skills Lab > Build All Scenes** (예제 씬 10개 + 강의 허브 + Build Settings).

## 강의 모드

- 첫 씬 `Assets/Lecture/LectureHub.unity` 에서 플레이하면 씬 목록(자유 모드)이 뜹니다. **F4** = 용어 목록 / 강의 모드.
- 용어 자료: `Assets/Lecture/Resources/SkillsLabGlossary.json` (id `skillslab`, 분야 6개 · 용어 25개). 용어로 들어갈 때마다 그 용어의 씬을 새로 엽니다.
- 한 용어 = 분야 소개 → ① 소개 → ② 개념 → ③ 직접 보기 → ④ AI에게 요청 (강의 시작 때 디렉터 인사).

| 키 | 하는 일 |
|---|---|
| PgDn / `]` · 말풍선 클릭 | 다음 단계 (마지막 단계면 다음 용어) |
| PgUp / `[` | 이전 단계 |
| End / Home | 다음 용어 / 이전 용어 |
| F3 | 말풍선 숨기기 · 보이기 |
| Backspace · 왼쪽 위 '목록' | 용어 목록 (강의 중이 아니면 씬 목록) |
| F4 | 강의 모드 켜기 · 끄기 |
| Esc | 강의 프로그램 끝내기 (강의 진행 중이 아닐 때, 빌드에서만) |

### 강의 프로그램 빌드

- 메뉴 **Tools > Skills Lab > Build Lecture Player** → `Builds/LecturePlayer/SkillsLab.exe` (Build Settings: LectureHub + 예제 씬 10개, `Builds/` 는 git 에 올리지 않음)
- 배치 모드: `Unity -batchmode -quit -projectPath . -executeMethod SkillsLabLecturePlayer.BuildFromCommandLine -logFile build.log`
- 실행:
  - `SkillsLab.exe -glossaryStart 0302 -glossaryLecture` — 0302 용어부터 강의 모드
  - `SkillsLab.exe -glossaryStart 0302` — 강의 없이 그 용어의 씬만
  - `SkillsLab.exe` — 자유 모드 (씬 목록)

## 스킬 파일 위치

이 저장소에는 실제 프로젝트 스킬 **class-rules**(우리 반 규칙)가 두 자리에 같은 내용으로 들어 있습니다.

| 도구 | 위치 |
|---|---|
| Claude Code | `.claude/skills/class-rules/SKILL.md` |
| Codex | `.agents/skills/class-rules/SKILL.md` |

규칙: 스크립트는 `Assets/01_Scripts/<기능>/` · 클래스 PascalCase · 필드 camelCase · 숫자는 `[SerializeField]` · 한글 주석 한 줄 · `Debug.Log` 남기지 않기 · 새 UI 는 UI Toolkit.
`0501_ClassRules` 씬이 이 파일과 A/B 결과 코드(`Assets/01_Scripts/ClassRules/`)를 탭으로 보여 줍니다. 두 코드 모두 컴파일되고 실행됩니다
(A 의 `FindObjectOfType` 은 Unity 6 에서 사용 중단 경고 CS0618 을 남깁니다 — '흔한 결과'의 일부로 일부러 둔 것).

## 직접 실습: 플러그인 전/후 같은 요청 해 보기

> **실습 전에:** 새 브랜치(`git switch -c practice`) 또는 복사한 폴더에서 합니다. 원본 `main` 에서는 AI 에게 파일을 바꾸게 하지 않습니다.

1. **플러그인 설치**
   - Claude Code (세션 안): `/plugin marketplace add Unity-Technologies/unity-agent-plugin` → `/plugin install unity@unity-agent-plugin`
     (터미널: `claude plugin install unity@unity-agent-plugin --scope local`, VS Code 확장은 `/plugins`)
   - Codex (터미널): `codex plugin marketplace add Unity-Technologies/unity-agent-plugin` → `codex plugin add unity@unity-agent-plugin`
   - 확인: `/unity:` 를 쳐서 스킬 목록이 뜨는지 (Claude Code `/plugin`, Codex `codex plugin list`)
2. **플러그인 끄기(전)**
   - Claude Code: `claude plugin disable unity@unity-agent-plugin` (세션 안: `/plugin disable`, 또는 `/plugin` → Installed 탭)
   - Codex: 끄기 대신 `codex plugin remove unity@unity-agent-plugin`
3. 실습용 브랜치(`practice`)나 복사본에서 같은 요청을 합니다. 예:
   - "이 프로젝트 HUD 옆에 설정 화면 만들어줘" (→ `0301`)
   - "UI Toolkit 버튼에 마우스를 올리면 0.4초 동안 커지게 해줘" (→ `0302`)
   - "총알이 0.1m 벽을 뚫고 지나가, 고쳐줘" (→ `0402`)
4. **플러그인 켜기(후)**
   - Claude Code: `claude plugin enable unity@unity-agent-plugin` (세션 안: `/plugin enable`)
   - Codex: `codex plugin add unity@unity-agent-plugin`
5. 같은 요청을 다시 하고, 결과를 해당 비교 씬의 A/B 와 견줘 봅니다. 질문을 먼저 하는지(0601), 어떤 스킬을 읽었는지(0204)도 봅니다.
   자동으로 안 불리면 `/unity:ui-uitk` (Codex 는 `$스킬이름`) 처럼 이름을 직접 부르고, 결과가 이상하면 "스킬의 references 폴더를 먼저 읽고 다시 해 줘" 라고 요청합니다.

> 에디터를 직접 조종하는 스킬은 Unity CLI 설치(`winget install Unity.CLI`) + `unity pipeline install` + 에디터 열림이 필요합니다. 연결 확인은 `unity status`.
> 단순한 uGUI 작업은 스킬이 있어도 차이가 작을 수 있습니다.

## 폴더

| 폴더 | 내용 |
|---|---|
| `Assets/00_Scenes` | 예제 씬 10개 (+ `0404_NavMesh/` 구운 NavMesh 데이터) |
| `Assets/01_Scripts` | 씬 스크립트(기능별 폴더) · `Editor/` 씬 빌더(`SkillsLabSceneBuilder`, `SkillsLabSceneKit`) |
| `Assets/02_UI` | UXML · USS · 예제 씬 PanelSettings · 테마 |
| `Assets/04_Materials` · `Assets/05_Sprites/Generated` | 씬 빌더가 만든 머티리얼 · 코드로 그린 도트 그림 |
| `Assets/06_Fonts` | Noto Sans KR (OFL) · UI Toolkit / TMP 글꼴 에셋 |
| `Assets/Lecture` | 강의 모드 (LectureHub 씬 · LectureOverlay · 용어 JSON · 팀원 그림) |
