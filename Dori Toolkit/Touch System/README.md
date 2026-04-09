# Touch Particle System

터치(클릭) 위치에 파티클 이펙트를 재생하는 경량 컴포넌트입니다.
Unity Input System 기반으로 동작하며, 오브젝트 풀링을 내장하고 있습니다.

자체 렌더링 파이프라인(전용 카메라 + RenderTexture + Overlay Canvas)을 런타임에 자동 생성하므로, 기존 Canvas의 Render Mode에 관계없이 독립적으로 동작합니다.

---

## 빠른 시작

1. **레이어 추가**: `Edit > Project Settings > Tags and Layers`에서 빈 레이어(기본값: 31)에 `Touch Particle` 이름 추가
2. **프리팹 배치**: `Touch Particle System` 프리팹을 씬 계층에 배치

---

## 동작 원리

| 구성 요소 | 역할 |
|---|---|
| 전용 카메라 | 지정 레이어만 렌더링하여 RenderTexture에 출력 |
| RenderTexture | 투명 배경 위에 터치 파티클만 담김 |
| Overlay Canvas | sortingOrder 최대값(32767)으로 모든 UI 위에 표시 |
| Additive 셰이더 | RawImage에서 파티클 RGB를 화면에 가산 합성 |

기존 Canvas가 `Screen Space - Overlay`든 `Screen Space - Camera`든 영향을 주지 않습니다.

---

## 커스텀 파티클 교체

프리팹 인스펙터의 **Particle Prefab** 슬롯에 원하는 `ParticleSystem` 프리팹을 드래그해 넣으면 됩니다.

> `Default Particle/` 폴더는 기본 샘플 이펙트의 아트 에셋입니다. 커스텀 파티클로 교체할 경우 해당 폴더를 삭제해도 됩니다.

---

## 요구 사항

- Unity 2021.3 이상 (권장)
- **Unity Input System** 패키지 필요 (`Window > Package Manager > Input System`)

---

<details>
<summary>인스펙터 상세 설명</summary>

### 파티클 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Particle Prefab** | 터치 시 재생할 ParticleSystem 프리팹 | (샘플 파티클) |
| **Distance From Camera** | 카메라로부터 파티클이 생성될 Z 거리 | `10` |

### 풀링 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Pool Size** | 동시에 재생 가능한 최대 파티클 인스턴스 수. 초과 시 가장 오래된 것부터 재사용 | `5` |

빠르게 연속 터치하는 상황이라면 Pool Size를 늘려주세요.

### 입력 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Auto Detect Input** | 체크 시 마우스/터치 입력을 자동 감지하여 재생 | `true` |

`Auto Detect Input`을 끄면 스크립트에서 직접 호출해 사용할 수 있습니다.

```csharp
GetComponent<TouchParticleSystem>().PlayAt(screenPosition);
```

### 렌더링 설정

| 항목 | 설명 | 기본값 |
|------|------|--------|
| **Particle Layer** | 터치 파티클 전용 레이어 번호. 다른 오브젝트와 겹치지 않는 레이어 사용 | `31` |
| **Overlay Material** | RawImage에 적용할 머터리얼. 비워두면 내장 Additive 셰이더 자동 적용 | 없음 |

</details>
