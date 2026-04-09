# Dori Util

Unity 유틸리티 프레임워크. **Core**(모듈 기반 통합 시스템)와 **Toolkit**(독립 컴포넌트 모음)으로 구성됩니다.

---

## 사전 준비

### DOTween (Custom Dotween System 사용 시)

Custom Dotween System을 사용하려면 **패키지 설치 전에** DOTween을 먼저 설정해야 합니다.  
사용하지 않을 경우 패키지 설치 후 `Custom Dotween System/` 폴더를 삭제하세요. (삭제하지 않으면 컴파일 에러 발생)

1. [DOTween Free](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) 설치 (Asset Store)
2. `Tools > Demigiant > DOTween Utility Panel` 열기
3. **Setup DOTween...** 클릭
4. **Create ASMDEF** 클릭

> 4단계(Create ASMDEF)까지 완료해야 Custom Dotween System이 정상 컴파일됩니다.

---

## 설치

Unity → `Window > Package Manager` → `+` → **Add package from git URL...**

| 설치 대상 | URL |
|---|---|
| **전체 (Core + Toolkit)** | `https://github.com/YOUR_USERNAME/Dori-Util.git` |
| **Core만** | `https://github.com/YOUR_USERNAME/Dori-Util.git?path=Dori Core` |
| **Toolkit만** | `https://github.com/YOUR_USERNAME/Dori-Util.git?path=Dori Toolkit` |

---

## 설치 후 설정

### 유료 에셋 연결 (개인 사용)

일부 프리팹이 참조하는 사운드, 폰트, 텍스처는 저작권 보호를 위해 git에 포함되지 않습니다.  
`.meta` 파일(GUID)은 커밋되어 있으므로, 에셋 파일을 **같은 경로에 같은 파일명으로** 복사하면 자동 연결됩니다:

```
Dori Toolkit/
├── Touch System/Default Resources/
│   ├── Click 03.wav
│   └── Particle/
│       ├── Glow_6.png, Glow_Particle.png, Hit_16.png
│       └── Glow_6.mat, Glow_Particle.mat, Hit_16.mat
│
└── UI Timer System/Default Resources/
    ├── ready.mp3, start.mp3, tickTock_01.wav
    ├── RiaSans-Bold.otf
    └── RiaSans-Bold SDF.asset
```

> 에셋 파일을 별도 폴더에 보관해두고, 새 프로젝트마다 해당 경로에 복사하면 가장 빠릅니다.

---

## 구조

```
Dori Util/
├── Dori Core/                모듈 기반 통합 프레임워크 (서비스 로케이터 패턴)
│   ├── DCore.cs              정적 서비스 레지스트리 (진입점)
│   ├── IDModuleBase.cs       모듈 인터페이스
│   ├── DCore_Object.cs       MonoBehaviour 라이프사이클 콜백 래퍼
│   ├── Module/
│   │   ├── MD_Timer.cs       이름 기반 타이머
│   │   ├── MD_Sound.cs       사운드 재생 (BGM/FX, 풀링)
│   │   ├── MD_Pool.cs        GameObject 오브젝트 풀링
│   │   ├── MD_Resource.cs    Resources 로드 유틸
│   │   ├── MD_Error.cs       에러 코드 관리
│   │   ├── MD_WebNetwork.cs  HTTP GET/POST 요청
│   │   └── MD_IdleReturn.cs  Idle 감지 → 첫 화면 복귀 이벤트
│   └── SO/
│       └── SOD_Sound.cs      사운드 에셋 ScriptableObject
│
└── Dori Toolkit/             독립 컴포넌트 모음 (각각 단독 사용 가능)
    ├── Custom Dotween System/   DOTween 기반 UI 애니메이션 (DOTween 필요)
    ├── Touch System/            터치 파티클 이펙트
    ├── UI Timer System/         카운트다운 타이머 + 준비/출발 연출
    ├── ChildFirstLayoutGroup.cs ContentSizeFitter 순서 보정
    └── Editor/                  AutoTMPFontCreator, CustomTMPGeneratorWindow, PSDLayerImporter
```

세부 문서: [Dori Core](Dori%20Core/README.md) | [Dori Toolkit](Dori%20Toolkit/README.md)

---

## 의존성

| 패키지 | 필수 여부 | 설치 |
|---|---|---|
| **TextMeshPro** | 필수 | 자동 (package.json) |
| **Input System** | Touch System 사용 시 | 자동 (package.json) |
| **DOTween Free** | Custom Dotween System 사용 시 | [Asset Store](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)에서 수동 설치 + Create ASMDEF (**패키지 설치 전에**) |

---

## 라이선스

코드: MIT License  
유료 에셋(사운드, 폰트, 텍스처): 별도 라이선스 적용, 재배포 불가
