using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MD_Pool : IDModuleBase
{
    class Pool
    {
        GameObject _prefab;
        IObjectPool<GameObject> _pool;

        Transform _root;
        Transform Root
        {
            get
            {
                if (_root == null)
                {
                    GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
                    _root = go.transform;
                }

                return _root;
            }
        }

        public Pool(GameObject prefab, Transform parent = null)
        {
            _prefab = prefab;
            _pool = new ObjectPool<GameObject>(() => OnCreate(parent), OnGet, OnRelease, OnDestroy);
        }

        public void Push(GameObject go)
        {
            if (go.activeSelf)
                _pool.Release(go);
        }

        public GameObject Pop()
        {
            return _pool.Get();
        }

        #region Funcs
        GameObject OnCreate(Transform parent = null)
        {
            GameObject go = GameObject.Instantiate(_prefab);

            parent = (parent == null) ? Root : parent;
            go.transform.SetParent(parent, false);

            go.name = _prefab.name;
            return go;
        }

        void OnGet(GameObject go)
        {
            go.SetActive(true);
        }

        void OnRelease(GameObject go)
        {
            go.SetActive(false);
        }

        void OnDestroy(GameObject go)
        {
            GameObject.Destroy(go);
        }
        #endregion
    }

    Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    public void Initialize()
    {

    }

    public GameObject Pop(GameObject prefab, Transform parent = null)
    {
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        GameObject go = _pools[prefab.name].Pop();

        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }

        return go;
    }

    public bool Push(GameObject go)
    {
        if (_pools.ContainsKey(go.name) == false)
            return false;

        _pools[go.name].Push(go);
        return true;
    }

    public void Clear()
    {
        _pools?.Clear();
    }

    public void CreatePool(GameObject prefab, Transform parent = null, bool isCanvas = false)
    {
        if (_pools.ContainsKey(prefab.name))
            return;

        Pool pool = new Pool(prefab, parent);
        _pools.Add(prefab.name, pool);
    }
}
