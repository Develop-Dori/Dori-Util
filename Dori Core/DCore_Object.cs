using UnityEngine;
using UnityEngine.Events;

public class DCore_Object : MonoBehaviour
{
    public UnityAction enableCallback;
    public UnityAction disableCallback;
    public UnityAction awakeCallback;
    public UnityAction startCallback;
    public UnityAction updateCallback;

    void OnEnable() => enableCallback?.Invoke();
    void OnDisable() => disableCallback?.Invoke();
    void Awake() => awakeCallback?.Invoke();
    void Start() => startCallback?.Invoke();
    void Update() => updateCallback?.Invoke();
}
