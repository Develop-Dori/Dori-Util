using System;
using System.IO;
using UnityEngine;

/// <summary>
/// JSON 파일 기반 설정 관리 모듈.
/// exe 옆(빌드) 또는 프로젝트 루트(에디터)의 settings.json을 로드/저장합니다.
///
/// 기본 제공 필드: debugMode, timeout
/// 프로젝트별 필드를 추가하려면 DefaultSettings를 참고하여 프로젝트 코드에서 확장하세요.
///
/// 사용법:
///   var setting = DCore.GetModule&lt;MD_Setting&gt;();
///   bool debug = setting.DebugMode;
///   float timeout = setting.Timeout;
///   setting.OnSettingsChanged += () => Debug.Log("설정 변경됨");
/// </summary>
public class MD_Setting : IDModuleBase
{
    [Serializable]
    public class DefaultSettings
    {
        public bool debugMode = false;
        public float timeout = 10f;
    }

    DefaultSettings settings;
    string settingsPath;

    /// <summary>
    /// 설정이 로드되거나 변경될 때 호출됩니다.
    /// </summary>
    public event Action OnSettingsChanged;

    public bool DebugMode => settings.debugMode;
    public float Timeout => settings.timeout;

    /// <summary>
    /// 현재 설정 객체에 직접 접근합니다. 값 변경 후 SaveSettings()를 호출하세요.
    /// </summary>
    public DefaultSettings Settings => settings;

    public void Initialize()
    {
        settingsPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "settings.json"));
        LoadSettings();
    }

    /// <summary>
    /// settings.json에서 설정을 로드합니다. 파일이 없으면 기본값으로 생성합니다.
    /// </summary>
    public void LoadSettings()
    {
        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                settings = JsonUtility.FromJson<DefaultSettings>(json);
                Debug.Log($"<color=cyan>[MD_Setting] 로드 완료: {settingsPath}</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MD_Setting] JSON 파싱 실패, 기본값 사용: {e.Message}");
                settings = new DefaultSettings();
                SaveSettings();
            }
        }
        else
        {
            Debug.Log($"<color=yellow>[MD_Setting] 파일 없음, 기본값으로 생성: {settingsPath}</color>");
            settings = new DefaultSettings();
            SaveSettings();
        }

        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// 현재 설정을 settings.json으로 저장합니다.
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(settingsPath, json);
            Debug.Log($"<color=cyan>[MD_Setting] 저장 완료: {settingsPath}</color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MD_Setting] 저장 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 설정 값을 변경한 후 호출하여 저장하고 이벤트를 발생시킵니다.
    /// </summary>
    public void ApplyAndSave()
    {
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    /// <summary>
    /// 커스텀 설정 클래스로 JSON을 로드합니다.
    /// 프로젝트별 확장 필드가 필요할 때 사용합니다.
    ///
    /// 예: var custom = setting.LoadAs&lt;MyProjectSettings&gt;();
    /// </summary>
    public T LoadAs<T>() where T : class, new()
    {
        if (!File.Exists(settingsPath))
            return new T();

        try
        {
            string json = File.ReadAllText(settingsPath);
            return JsonUtility.FromJson<T>(json);
        }
        catch
        {
            return new T();
        }
    }
}
