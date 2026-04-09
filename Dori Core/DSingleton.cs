using UnityEngine;

/// <summary>
/// MonoBehaviour 기반 싱글톤 베이스 클래스.
/// DontDestroyOnLoad을 적용하려면 자식 Awake에서 base.Awake() 호출 후 DontDestroyOnLoad(gameObject)를 추가하세요.
///
/// 사용법:
///   public class MyManager : DSingleton&lt;MyManager&gt; { }
///   // 접근: MyManager.Instance.SomeMethod();
/// </summary>
public class DSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isQuitting = false;

    public static T Instance
    {
        get
        {
            if (isQuitting)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
            instance = this as T;
        else if (instance != this)
            Destroy(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
