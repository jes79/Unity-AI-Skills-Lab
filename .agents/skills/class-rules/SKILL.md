---
name: class-rules
description: 우리 반 Unity 코드 규칙. 이 프로젝트에서 C# 스크립트(MonoBehaviour)·게임 UI·점수판·HUD 를 새로 만들거나 고칠 때 사용한다. 스크립트 폴더, 클래스·필드 이름, 인스펙터 숫자([SerializeField]), 한글 주석, Debug.Log 금지, 새 UI 는 UI Toolkit 규칙을 적용한다.
---

# 우리 반 규칙 (class-rules)

이 프로젝트에서 Unity 스크립트나 UI 를 만들 때는 아래 규칙을 지킨다.
규칙과 다른 요청을 받으면, 바꾸기 전에 먼저 물어본다.

## 1. 폴더

- 스크립트는 `Assets/01_Scripts/<기능>/` 에 둔다. 예: 점수판 → `Assets/01_Scripts/Score/ScoreCounter.cs`
- UI 파일(UXML · USS)은 `Assets/02_UI/<기능>/` 에 둔다.

## 2. 이름

- 클래스 · 파일 이름은 PascalCase. 파일 이름 = 클래스 이름.
- 필드 · 지역 변수는 camelCase. 예: `currentScore`, `pointsPerPress`
- 필드는 `public` 으로 열지 않는다. 인스펙터에 보여야 하면 `[SerializeField]` private 필드로 둔다.

## 3. 숫자는 인스펙터로

- 점수, 속도, 시간처럼 바꿔 볼 만한 숫자는 코드 안에 바로 쓰지 않는다.
- `[SerializeField] int pointsPerPress = 10;` 처럼 필드로 꺼내서 인스펙터에서 고칠 수 있게 한다.
- UI 의 위치 · 크기 · 색은 C# 대신 USS 에 쓴다.

## 4. 주석

- 클래스 위에 한글 주석 한 줄로 "이 스크립트가 하는 일"을 쓴다.
- 코드를 그대로 읽어 주는 주석은 쓰지 않는다.

## 5. 로그

- `Debug.Log` 를 남기지 않는다. 확인용으로 넣은 로그는 작업이 끝나기 전에 지운다.

## 6. 다른 부품 찾기

- `Update` 안에서 `FindObjectOfType` · `GameObject.Find` 를 부르지 않는다.
- 필요한 부품은 `[SerializeField]` 로 받아 인스펙터에서 연결한다.

## 7. UI

- 새 UI 는 UI Toolkit(UXML · USS + `UIDocument`)으로 만든다. `OnGUI` 와 새 uGUI Canvas 는 쓰지 않는다.
- 이미 있는 UI 의 USS 테마를 같이 쓴다.

## 마무리 확인

작업이 끝나면 아래를 스스로 확인하고 결과에 적는다.

- [ ] 파일이 `Assets/01_Scripts/<기능>/` 에 있다
- [ ] public 필드 없음, 숫자는 `[SerializeField]`
- [ ] 클래스 위 한글 주석 한 줄
- [ ] `Debug.Log` 없음, `Update` 에서 Find 없음
- [ ] UI 는 UI Toolkit
- [ ] 에디터에서 사람이 직접 연결해야 하는 것(인스펙터 필드 등)을 따로 적었다
