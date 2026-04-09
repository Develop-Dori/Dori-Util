# DCore

모듈 간 종속성을 가지는 **통합 프레임워크**입니다.  
서비스 로케이터 패턴 기반으로, 씬 로드 시 자동으로 모든 모듈을 탐색·등록하며 `DCore.GetModule<T>()`로 접근합니다.

> **Toolkit**과의 차이: Toolkit은 각 컴포넌트가 독립적으로 동작하는 도구 모음이고,  
> Core는 중앙 레지스트리(`DCore`)를 통해 모듈들이 서로 연결되는 통합 시스템입니다.

---

## 구조

```
Core/
├── DCore.cs              # 정적 서비스 레지스트리 (진입점)
├── IDModuleBase.cs       # 모듈 인터페이스
├── DCore_Object.cs       # MonoBehaviour 라이프사이클 콜백 래퍼
├── Module/
│   ├── MD_Error.cs       # 에러 코드 관리
│   ├── MD_Pool.cs        # GameObject 오브젝트 풀링
│   ├── MD_Resource.cs    # Resources 로드 유틸
│   ├── MD_Sound.cs       # 사운드 재생 (BGM/FX, 풀링)
│   ├── MD_Timer.cs       # 이름 기반 타이머 시스템
│   ├── MD_WebNetwork.cs  # HTTP GET/POST 요청
│   └── MD_IdleReturn.cs  # Idle 감지 → 첫 화면 복귀 이벤트
└── SO/
    └── SOD_Sound.cs      # 사운드 에셋 ScriptableObject
```

---

## 사용법

### 기본 패턴

```csharp
// 모든 모듈은 씬 로드 시 자동 초기화됩니다.
// GetModule<T>()로 접근:
var timer = DCore.GetModule<MD_Timer>();
timer.StartTimer("countdown", 5f, () => Debug.Log("완료!"));
```

### 모듈별 예시

#### MD_Timer (타이머)
```csharp
var timer = DCore.GetModule<MD_Timer>();
timer.StartTimer("myTimer", 3f, () => Debug.Log("3초 경과"));
timer.PauseTimer("myTimer", true);   // 일시정지
timer.PauseTimer("myTimer", false);  // 재개
timer.StopTimer("myTimer");          // 중지
float progress = timer.GetNormalizeValue("myTimer", false); // 0~1
float remaining = timer.GetRemainingTime("myTimer");        // 남은 시간 (초)
int remainSec = timer.GetRemainingSeconds("myTimer");       // 남은 시간 (정수, CeilToInt)
bool running = timer.IsTimerRunning("myTimer");             // 실행 중 여부
```

#### MD_Sound (사운드)
```csharp
var sound = DCore.GetModule<MD_Sound>();
sound.SetSoundData("SoundAssetName");            // SO 에셋 로드
sound.Play("bgm_main", MD_Sound.Setting_Bgm);   // BGM 재생
sound.PlayFX("click");                            // FX 재생
sound.Stop("bgm_main");
```

#### MD_Pool (오브젝트 풀)
```csharp
var pool = DCore.GetModule<MD_Pool>();
pool.CreatePool(bulletPrefab);
GameObject bullet = pool.Pop(bulletPrefab);
pool.Push(bullet); // 반환
```

#### MD_WebNetwork (네트워크)
```csharp
var network = DCore.GetModule<MD_WebNetwork>();
network.AddUri("getUser", "https://api.example.com/users/{0}");
string uri = network.GetUri("getUser", "123");
network.GetRequest(uri, (json) => Debug.Log(json));

// POST with form data
var form = new MD_WebNetwork.FormData();
form.Add("name", "Dori");
network.PostRequest(uri, form.GetFormData(), (response) => Debug.Log(response));
```

#### MD_Error (에러 관리)
```csharp
var error = DCore.GetModule<MD_Error>();
error.SetClientErrorCallback(MD_Error.ClientCode.Equipment, () => {
    Debug.LogError("장비 에러 발생!");
});
error.InvokeClientErrorCallback(MD_Error.ClientCode.Equipment);
```

#### MD_IdleReturn (Idle 감지 / 첫 화면 복귀)
```csharp
var idle = DCore.GetModule<MD_IdleReturn>();
idle.SetTimeout(15f);             // 15초 (기본값: 10초)
idle.SetDetectMouseMove(false);   // 마우스 이동 감지 여부 (기본값: false, 터치스크린 환경 권장)

idle.OnUserActivity += () => Debug.Log("사용자 활동 감지");
idle.OnIdleTimeout  += () => {
    Debug.Log("Idle 타임아웃 → 첫 화면으로 이동");
    SceneManager.LoadScene("HomeScene");
};

idle.Activate();   // 감지 시작
// idle.Deactivate(); // 감지 중단
// idle.ReportActivity(); // 외부에서 수동으로 활동 보고 (같은 프레임 중복 호출 자동 방지)
```

---

## 커스텀 모듈 만들기

`IDModuleBase`를 구현하면 자동으로 레지스트리에 등록됩니다:

```csharp
public class MD_MyModule : IDModuleBase
{
    public void Initialize()
    {
        // 초기화 로직
    }
}

// 사용:
var myModule = DCore.GetModule<MD_MyModule>();
```

---

## 유틸리티

```csharp
// MonoBehaviour가 필요한 경우 CoreObject 생성
DCore_Object obj = DCore.CreateCoreObject("MyObject");
obj.updateCallback = () => { /* 매 프레임 실행 */ };

// 오브젝트 복제
GameObject clone = DCore.CloneObject(original, parentTransform);
```
