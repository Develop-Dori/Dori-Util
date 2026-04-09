using UnityEngine;

public class MD_Resource : IDModuleBase
{
    public void Initialize()
    {

    }

    #region 프리팹 생성 및 로드
    public T LoadPrefab<T>(string key) where T : UnityEngine.Object
    {
        T _resource = Resources.Load<T>($"Prefabs/{key}");
        if (_resource == null)
        {
            return null;
        }

        return _resource;
    }

    public GameObject Instantiate(string key, Transform parent = null)
    {
        GameObject prefab = LoadPrefab<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }

        return Instantiate(prefab, parent);
    }

    public GameObject Instantiate(GameObject prefab, Transform parent = null)
    {
        GameObject go = UnityEngine.Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(Object go)
    {
        if (go == null)
            return;
        UnityEngine.Object.Destroy(go);
    }

    #endregion
}
