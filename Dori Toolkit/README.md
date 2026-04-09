# Toolkit

각 컴포넌트가 **독립적으로 동작**하는 유틸리티 도구 모음입니다.  
필요한 것만 골라서 프로젝트에 넣으면 바로 사용할 수 있습니다.

> **Core**와의 차이: Core는 중앙 레지스트리(`DCore`)를 통해 모듈들이 연결되는 통합 프레임워크이고,  
> Toolkit은 서로 의존성 없이 개별적으로 동작하는 컴포넌트 모음입니다.

---

## 구성 요소

| 폴더/파일                              | 설명                                                                        | 세부 문서                                     |
| -------------------------------------- | --------------------------------------------------------------------------- | --------------------------------------------- |
| **Custom Dotween System/**             | DOTween 기반 UI 애니메이션 (Fade, Rotate, Pulse, Press, UpDown, Sequential) | [README](Custom%20Dotween%20System/README.md) |
| **Editor/AutoTMPFontCreator.cs**       | 폰트 파일에서 TMP SDF 폰트 에셋을 4096 사이즈로 자동 생성                   | 아래 참조                                     |
| **Editor/CustomTMPGeneratorWindow.cs** | 선택한 UI 오브젝트 옆에 TMP를 일괄 생성하는 에디터 도구                     | 아래 참조                                     |
| **Touch System/**                      | 터치/클릭 위치에 파티클 이펙트 재생 (독립 렌더링 파이프라인)                | [README](Touch%20System/README.md)            |
| **UI Timer System/**                   | 준비/출발 인트로 + 카운트다운 연출 타이머                                   | [README](UI%20Timer%20System/README.md)       |
| **ChildFirstLayoutGroup.cs**           | ContentSizeFitter 자식→부모 계산 순서 보정 컴포넌트                         | 아래 참조                                     |
| **PSDLayerImporter.cs**                | 포토샵 PSD 레이어를 Unity Canvas에 자동 배치하는 에디터 도구                | 아래 참조                                     |

---

## AutoTMPFontCreator (Editor)

폰트 파일(.ttf, .otf)에서 TextMeshPro용 SDF 폰트 에셋을 4096x4096 사이즈로 자동 생성합니다.

**사용법**: Project 창에서 폰트 파일을 선택 → 우클릭 → `Create > TextMeshPro > Auto Create Font Asset (4096)` 클릭. 원본과 동일한 경로에 `폰트명 SDF.asset`이 생성됩니다.

---

## CustomTMPGeneratorWindow (Editor)

선택한 UI 오브젝트들의 바로 옆(동일 계층)에 TextMeshPro를 일괄 생성하는 에디터 도구입니다.

**사용법**: `GameObject > UI > Create Custom TMP Next to Selection` 메뉴로 창을 열고, 하이어라키에서 기준 오브젝트를 다중 선택한 뒤 버튼 클릭.

- Anchor, Pivot 자동 복사
- 너비 10% 확장, 중앙 정렬
- Font Asset 지정 가능
- Undo(Ctrl+Z) 지원

---

## ChildFirstLayoutGroup

ContentSizeFitter가 부모→자식 순서로 계산되어 중앙 정렬이 깨지는 문제를 해결합니다.

**사용법**: 부모 오브젝트에 이 컴포넌트를 추가하면 끝. 기존 ContentSizeFitter는 그대로 유지합니다.

---

## PSDLayerImporter

포토샵 스크립트로 추출한 PSD 레이어 데이터를 Unity Canvas에 자동 배치하는 에디터 도구입니다.

**사용법**: `Tools > PSD Import > Import Extracted Folder` 메뉴를 열고, 포토샵 스크립트로 추출한 폴더 경로(layerData.json 포함)를 선택합니다.

- PSD 문서 크기 기준으로 Canvas 참조 해상도 자동 설정
- 레이어별 위치, 크기, 투명도 자동 반영
- 레이어 순서(앞/뒤) 그대로 유지
- Undo(Ctrl+Z) 지원

### 트러블슈팅: 이펙트가 검은색으로 깨지는 경우

임포트한 이미지 중 파티클이나 이펙트가 검은 배경으로 표시될 경우:

1. 새 Material을 생성
2. 셰이더를 `Mobile/Particles/Additive`로 설정
3. 해당 GameObject의 `Image > Material` 슬롯에 생성한 Material을 할당
