using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Sound", menuName = "ScriptableObject/Dori/Sound", order = 1)]
public class SOD_Sound : ScriptableObject
{
    public List<SoundInfo> clipList = new List<SoundInfo>();

    [System.Serializable]
    public class SoundInfo
    {
        public string name;
        public AudioClip clip;
    }

    public AudioClip FindClip(string soundName)
    {
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i].name == soundName)
            {
                return clipList[i].clip;
            }
        }

        Debug.LogError("Null Sound - " + soundName);
        return null;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(SOD_Sound))]
public class SOD_SoundEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SOD_Sound component = (SOD_Sound)target;
        if (UnityEngine.GUILayout.Button("AddList"))
        {
            component.clipList.Add(new SOD_Sound.SoundInfo());
        }
    }
}
#endif
