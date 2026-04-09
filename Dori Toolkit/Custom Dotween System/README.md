# Custom DOTween System

DOTween 기반의 간편한 UI 애니메이션 컴포넌트 모음입니다.
오브젝트에 컴포넌트를 붙이기만 하면 바로 동작합니다.

모든 컴포넌트(Press 제외)에 `OnAnimationComplete` UnityEvent가 있어 Inspector에서 완료 콜백을 등록할 수 있습니다.

## 요구 사항

- DOTween (무료) 또는 DOTween Pro
- Unity 2021.3 이상 (권장)

---

## 컴포넌트 목록

### CDFadeObject

CanvasGroup을 이용해 오브젝트와 모든 자식을 함께 페이드 인/아웃합니다.

| 항목          | 설명                  | 기본값   |
| ------------- | --------------------- | -------- |
| **Loop**      | 반복 횟수 (-1: 무한)  | `-1`     |
| **Duration**  | 페이드 한 사이클 시간 | `1`      |
| **Ease**      | 이징 곡선             | `Linear` |
| **Auto Play** | 시작 시 자동 재생     | `true`   |

> CanvasGroup이 없으면 자동으로 추가됩니다. CanvasGroup은 자식 오브젝트까지 한 번에 투명도를 적용합니다.

```csharp
GetComponent<CDFadeObject>().Play();
GetComponent<CDFadeObject>().Pause();
```

---

### CDUpDownObject

오브젝트를 위아래로 반복 이동합니다. RectTransform 기반입니다.

| 항목          | 설명                                         | 기본값      |
| ------------- | -------------------------------------------- | ----------- |
| **Loop**      | 반복 횟수 (-1: 무한)                         | `-1`        |
| **Range**     | 위아래 총 이동 거리 (위로 절반, 아래로 절반) | `100`       |
| **Duration**  | 한 사이클 시간                               | `1`         |
| **Ease**      | 이징 곡선                                    | `InOutSine` |
| **Auto Play** | 시작 시 자동 재생                            | `true`      |

> Range가 100이면 원래 위치에서 위로 50, 아래로 50 이동합니다.

```csharp
GetComponent<CDUpDownObject>().Play();
GetComponent<CDUpDownObject>().Pause();
```

---

### CDPressScaleObject

누르면 축소, 떼면 원래 크기로 복원됩니다.

| 항목              | 설명             | 기본값 |
| ----------------- | ---------------- | ------ |
| **Pressed Scale** | 눌렀을 때 스케일 | `0.8`  |

> `CDPulseScaleObject`가 같은 오브젝트에 있으면 자동으로 비활성화되고, Pulse 쪽에서 통합 처리합니다.

---

### CDPulseScaleObject

커졌다 돌아오는 애니메이션을 반복합니다. 누르면 축소 후 고정, 떼면 복원 후 다시 반복합니다.

| 항목            | 설명               | 기본값      |
| --------------- | ------------------ | ----------- |
| **Pulse Scale** | 커지는 최대 스케일 | `1.1`       |
| **Duration**    | 한 사이클 시간     | `0.5`       |
| **Ease**        | 이징 곡선          | `InOutSine` |

#### 조합 사용법

| 구성           | 동작                                                            |
| -------------- | --------------------------------------------------------------- |
| CDPressScale만 | 누르면 축소, 떼면 복원                                          |
| CDPulseScale만 | 반복 펄스 + 누르면 0.8 축소, 떼면 복원 후 펄스 재개             |
| 둘 다          | CDPulseScale이 통합 처리, CDPressScale의 `pressedScale` 값 사용 |

---

### CDRotateObject

오브젝트를 Z축 기준으로 제자리 회전합니다. UI(RectTransform), 일반 오브젝트(Transform) 모두 사용 가능합니다.

| 항목          | 설명                     | 기본값   |
| ------------- | ------------------------ | -------- |
| **Loop**      | 반복 횟수 (-1: 무한)     | `-1`     |
| **Duration**  | 한 바퀴(360도) 도는 시간 | `2`      |
| **Ease**      | 이징 곡선                | `Linear` |
| **Auto Play** | 시작 시 자동 재생        | `true`   |

```csharp
GetComponent<CDRotateObject>().Play();
GetComponent<CDRotateObject>().Pause();
```

---

### CDSequentialPopObject

리스트에 등록된 오브젝트들을 순차적으로 커졌다 돌아오는 강조 애니메이션을 실행합니다.

| 항목            | 설명                                       | 기본값     |
| --------------- | ------------------------------------------ | ---------- |
| **Targets**     | 순차 강조할 Transform 리스트               | (비어있음) |
| **Pop Scale**   | 강조 시 커지는 스케일 (이후 1로 복원)      | `1.2`      |
| **Duration**    | 커지는/돌아오는 시간                       | `0.3`      |
| **Interval**    | 각 오브젝트 사이 대기 시간                 | `1`        |
| **Start Delay** | 최초 1회 시작 지연 (OnEnable 시 적용)      | `0`        |
| **Ease**        | 이징 곡선                                  | `Linear`   |
| **Loop Count**  | 반복 횟수 (-1: 무한, 1: 1회)               | `1`        |
| **Auto Play**   | 시작 시 자동 재생                          | `true`     |

> `Start Delay`는 OnEnable 시 딱 1회만 적용됩니다. 루프 시에는 `Interval`만 사용됩니다.

```csharp
GetComponent<CDSequentialPopObject>().Play();
GetComponent<CDSequentialPopObject>().Pause();
```

---

### CDSequentialFadeObject

리스트에 등록된 오브젝트들을 순차적으로 하나씩 페이드 인합니다. CanvasGroup 기반이므로 각 target의 자식 오브젝트도 함께 페이드됩니다.

| 항목                 | 설명                                       | 기본값     |
| -------------------- | ------------------------------------------ | ---------- |
| **Targets**          | 순차 페이드할 Transform 리스트             | (비어있음) |
| **Duration**         | 페이드 인 시간                             | `0.3`      |
| **Interval**         | 각 오브젝트 사이 대기 시간                 | `0.5`      |
| **Fade Out Duration**| 루프 시 전체 동시 페이드 아웃 시간         | `0.3`      |
| **Start Delay**      | 최초 1회 시작 지연 (OnEnable 시 적용)      | `0`        |
| **Ease**             | 이징 곡선                                  | `Linear`   |
| **Loop Count**       | 반복 횟수 (-1: 무한, 1: 1회)               | `1`        |
| **Auto Play**        | 시작 시 자동 재생                          | `true`     |

#### 루프 동작 흐름

```
[투명] → A 페이드 인 → B 페이드 인 → C 페이드 인 → [전부 밝음]
    → 전체 동시 페이드 아웃 → [투명] → A 페이드 인 → ... (반복)
    → 마지막 루프: 전부 밝은 상태에서 종료
```

> CanvasGroup이 없으면 자동으로 추가됩니다.

```csharp
GetComponent<CDSequentialFadeObject>().Play();
GetComponent<CDSequentialFadeObject>().Pause();
```

---

## 공통 이벤트

Press/Pulse를 제외한 모든 컴포넌트에 `OnAnimationComplete` UnityEvent가 있습니다.

```csharp
// 코드에서 등록
GetComponent<CDFadeObject>().OnAnimationComplete.AddListener(() => Debug.Log("완료!"));
```

Inspector에서도 UnityEvent로 등록 가능합니다.

> **무한 루프(-1)인 경우**: 종료 시점이 없으므로 OnAnimationComplete가 호출되지 않습니다.
