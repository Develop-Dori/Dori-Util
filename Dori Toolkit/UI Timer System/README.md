# UI Timer System

카운트다운 타이머에 준비/출발 인트로, 종료 전 숫자 카운트다운(5,4,3,2,1) 연출, 사운드 재생을 지원하는 독립형 컴포넌트입니다.
외부 의존성 없이 단독 동작하며, 모든 연출 기능을 개별적으로 on/off 할 수 있어 기본 타이머로도 사용 가능합니다.

---

## 구성 파일

| 파일 | 설명 |
|------|------|
| **UITimerSystem.cs** | UI 연출 포함 메인 타이머 (준비/출발, 카운트다운) |
| **UITimerOverlay.cs** | 오버레이 프리팹 컴포넌트 (Mask, DisplayText 참조 관리) |

---

## 빠른 시작

1. `UI Timer System` 프리팹을 씬에 배치
2. Inspector에서 원하는 기능 on/off 및 사운드 할당
3. `autoPlay = true`(기본값)이면 활성화 시 자동 시작, 아니면 외부에서 `StartTimer()` 호출

```csharp
// 기본 duration으로 시작
timerSystem.StartTimer();

// 커스텀 시간으로 시작
timerSystem.StartTimer(60f);
```

---

## 오버레이 프리팹

`overlayPrefab`은 필수입니다. Default Resources 폴더에 기본 프리팹이 포함되어 있으며, 프로젝트별로 커스텀 프리팹을 만들어 교체할 수 있습니다.

프리팹에는 `UITimerOverlay` 컴포넌트가 부착되어 있으며, Mask(Image)와 DisplayText(TMP_Text) 참조를 Inspector에서 직접 할당합니다.
오브젝트 이름을 변경해도 참조가 유지되므로 자유롭게 커스텀할 수 있습니다.

---

## 마스크 설정

`maskAlpha` 값으로 마스크 사용 여부가 결정됩니다:
- `maskAlpha = 0` → 마스크 사용 안 함
- `maskAlpha > 0` → 해당 투명도로 마스크 표시

별도의 on/off 토글 없이 투명도 값만으로 제어합니다.

---

## 동작 흐름

```
StartTimer() 호출 (또는 autoPlay로 자동 호출)
    │
    ├─ [useReadyGo = true]
    │   ├─ [maskAlpha > 0] 검은 마스크 표시
    │   ├─ "준비" 텍스트 페이드 + readyClip 재생
    │   ├─ "출발" 텍스트 페이드 + goClip 재생
    │   └─ [maskAlpha > 0] 마스크 페이드 아웃
    │
    ├─ OnTimerStarted 이벤트 발생 ← 실제 타이머 시작 시점
    │
    ├─ 카운트다운 진행 (RemainingTime / RemainingSeconds 감소)
    │
    ├─ [useCountdown = true, 남은 시간 ≤ countdownFrom]
    │   └─ 5, 4, 3, 2, 1 숫자 페이드 + tickClip 재생
    │
    └─ OnTimerEnded 이벤트 발생
```

---

## 기능 조합 예시

| 용도 | autoPlay | useReadyGo | maskAlpha | useCountdown |
|---|---|---|---|---|
| 풀 연출 (게임 타이머) | on | on | 0.5 | on |
| 기본 타이머 (연출 없음) | on | off | - | off |
| 카운트다운만 | on | off | - | on |
| 인트로만 (퀴즈 등) | on | on | 0.5 | off |
| 외부 호출로 시작 | off | on | 0.5 | on |

---

## 리소스 설정

Inspector에서 사운드를 할당하세요. 할당하지 않으면 해당 시점에 소리 없이 진행됩니다.

| 슬롯 | 재생 시점 | 필수 여부 |
|---|---|---|
| **Ready Clip** | "준비" 텍스트 표시 시 | 선택 |
| **Go Clip** | "출발" 텍스트 표시 시 | 선택 |
| **Tick Clip** | 카운트다운 숫자 표시 시 | 선택 |

---

## 요구 사항

- Unity 2021.3 이상 (권장)
- TextMeshPro 패키지 (Unity 기본 포함)

---

<details>
<summary>인스펙터 상세 설명</summary>

### 타이머 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Duration** | 타이머 지속 시간 (초) | `180` |
| **Auto Play** | 활성화 시 자동 타이머 시작 | `true` |

### 오버레이 UI

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Overlay Prefab** | UITimerOverlay 오버레이 프리팹 (필수) | 없음 |

### 준비/출발 인트로

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Use Ready Go** | 인트로 연출 사용 여부 | `true` |
| **Mask Alpha** | 마스크 투명도 (0이면 마스크 미사용) | `0.5` |
| **Ready Text** | 준비 단계 표시 텍스트 | `준비` |
| **Go Text** | 출발 단계 표시 텍스트 | `출발` |
| **Ready Display Time** | 준비 텍스트 표시 시간 | `1.5` |
| **Go Display Time** | 출발 텍스트 표시 시간 | `1.0` |
| **Ready Clip** | 준비 시 재생할 오디오 | 없음 |
| **Go Clip** | 출발 시 재생할 오디오 | 없음 |

### 카운트다운 효과

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Use Countdown** | 카운트다운 연출 사용 여부 | `true` |
| **Countdown From** | 카운트다운 시작 숫자 | `5` |
| **Tick Clip** | 째깍 사운드 | 없음 |

### 사운드 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Volume** | 사운드 볼륨 | `1.0` |

### 이벤트

| 이벤트 | 발생 시점 |
|--------|----------|
| **OnTimerStarted** | 준비/출발 완료 후 실제 타이머가 시작될 때 |
| **OnTimerEnded** | 타이머가 0에 도달했을 때 |

</details>

<details>
<summary>스크립트 API</summary>

### UITimerSystem

```csharp
// 프로퍼티
float RemainingTime      // 남은 시간 (초, float)
int   RemainingSeconds   // 남은 시간 (정수, CeilToInt — 0.1초 남아도 1 반환)
bool  IsRunning          // 실제 카운트 중인지 여부

// 타이머 제어
void StartTimer()                     // 기본 duration으로 시작
void StartTimer(float duration)       // 커스텀 시간으로 시작
void StopTimer()                      // 중지 및 초기화
void PauseTimer()                     // 일시정지
void ResumeTimer()                    // 재개
```

</details>
