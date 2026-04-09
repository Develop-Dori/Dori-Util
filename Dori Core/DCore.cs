using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DCore
{
    private static readonly Dictionary<System.Type, IDModuleBase> activeModules = new Dictionary<System.Type, IDModuleBase>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeRegistry()
    {
        if (activeModules.Any()) return;

        var moduleTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => typeof(IDModuleBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (System.Type moduleType in moduleTypes)
        {
            IDModuleBase moduleInstance = System.Activator.CreateInstance(moduleType) as IDModuleBase;
            moduleInstance.Initialize();
            activeModules.Add(moduleType, moduleInstance);
        }
    }

    public static T GetModule<T>() where T : class, IDModuleBase
    {
        System.Type targetType = typeof(T);
        if (activeModules.TryGetValue(targetType, out IDModuleBase moduleInstance))
        {
            return moduleInstance as T;
        }

        return null;
    }

    public static DCore_Object CreateCoreObject(string objName)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.position = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        DCore_Object coreObj = obj.AddComponent<DCore_Object>();
        return coreObj;
    }

    public static GameObject CloneObject(GameObject obj, Transform parent = null)
    {
        GameObject cloneObj = GameObject.Instantiate(obj);

        if (parent == null)
            cloneObj.transform.parent = obj.transform.parent;
        else
            cloneObj.transform.parent = parent;

        cloneObj.transform.position = Vector3.zero;
        cloneObj.transform.localRotation = Quaternion.identity;
        cloneObj.transform.localScale = Vector3.one;

        return cloneObj;
    }
}
