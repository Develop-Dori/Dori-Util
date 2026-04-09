using System.Collections.Generic;
using UnityEngine;

public class MD_Sound : IDModuleBase
{
    GameObject soundObj = null;
    SOD_Sound soSound = null;
    Dictionary<string, AudioSource> audioMap = new Dictionary<string, AudioSource>();
    List<AudioSource> poolingList = new List<AudioSource>();

    public static Setting Setting_Bgm = GetDefaultBGMSetting();
    public static Setting Setting_Fx = GetDefaultSESetting();

    public class Setting
    {
        public Transform parent = null;
        public Vector3? pos = null;
        public bool isLoop = false;
        public bool isPooling = false;
        public float volume;

        public Setting(float volume, bool isLoop)
        {
            this.volume = volume;
            this.isLoop = isLoop;
        }
    }

    public void Initialize()
    {
        CreateSoundObject();
    }

    void CreateSoundObject()
    {
        soundObj = new GameObject("SoundObj");
        soundObj.transform.position = Vector3.zero;
        soundObj.transform.localRotation = Quaternion.identity;
        soundObj.transform.localScale = Vector3.one;

        DCore_Object coreObj = soundObj.AddComponent<DCore_Object>();
        coreObj.updateCallback = SetUpdateCallback;
    }

    void SetUpdateCallback()
    {
        for (int i = 0; i < poolingList.Count; i++)
        {
            if (poolingList[i].gameObject.activeSelf && IsAudioFinished(poolingList[i]))
            {
                poolingList[i].gameObject.SetActive(false);
                poolingList[i].transform.SetParent(soundObj.transform, worldPositionStays: false);
            }
        }
    }

    internal void SetSoundData(string name)
    {
        if (soSound == null)
            soSound = Resources.Load<SOD_Sound>($"SO/{name}");
    }

    AudioSource CreateAudioObject(string soundName, Transform parent, Vector3? pos)
    {
        GameObject soundObj = new GameObject();
        soundObj.name = soundName;
        soundObj.transform.parent = parent == null ? this.soundObj.transform : parent;
        soundObj.transform.localPosition = pos == null ? Vector3.zero : (Vector3)pos;
        soundObj.transform.localScale = Vector3.one;
        AudioSource audio = soundObj.AddComponent<AudioSource>();
        audio.gameObject.SetActive(false);

        return audio;
    }

    AudioSource GetPoolingAudio(string soundName, Setting setting)
    {
        for (int i = 0; i < poolingList.Count; i++)
        {
            if (!poolingList[i].gameObject.activeSelf)
            {
                poolingList[i].gameObject.transform.parent = setting.parent;
                poolingList[i].gameObject.transform.localPosition = setting.pos ?? Vector3.zero;
                poolingList[i].gameObject.transform.localScale = Vector3.one;

                return poolingList[i];
            }
        }

        AudioSource audio = CreateAudioObject(soundName, setting.parent, setting.pos);
        poolingList.Add(audio);
        return audio;
    }

    public bool IsAudioFinished(AudioSource audioSource)
    {
        if (!audioSource.isPlaying)
        {
            if (audioSource.time <= 0.001f)
            {
                return true;
            }
        }
        return false;
    }

    public void PlayFX(string soundName)
    {
        Play(soundName, Setting_Fx);
    }

    public void Play(string soundName, Setting setting)
    {
        if (string.IsNullOrEmpty(soundName))
            return;

        AudioClip clip = soSound.FindClip(soundName);
        if (clip == null)
            return;

        AudioSource audio = null;
        if (setting.isPooling)
        {
            setting.isLoop = false;
            audio = GetPoolingAudio(soundName, setting);
        }
        else
        {
            if (audioMap.ContainsKey(soundName))
                audio = audioMap[soundName];
            else
            {
                audio = CreateAudioObject(soundName, setting.parent, setting.pos);
                audioMap.Add(soundName, audio);
            }
        }

        audio.clip = clip;
        audio.loop = setting.isLoop;
        audio.volume = setting.volume;

        audio.gameObject.SetActive(true);

        if (!audio.isPlaying)
            audio.Play();
    }

    public void Stop(string soundName)
    {
        if (audioMap.ContainsKey(soundName))
        {
            audioMap[soundName].Stop();
        }
    }

    public void Pause(string soundName, bool isPause)
    {
        if (audioMap.ContainsKey(soundName))
        {
            if (isPause)
                audioMap[soundName].Pause();
            else
                audioMap[soundName].UnPause();
        }
    }

    public AudioSource GetAudio(string soundName)
    {
        if (audioMap.ContainsKey(soundName))
        {
            return audioMap[soundName];
        }
        return null;
    }

    public static Setting GetDefaultBGMSetting(float volume = 1.0f)
    {
        Setting setting = new Setting(volume, true);
        setting.isPooling = false;
        return setting;
    }

    public static Setting GetDefaultSESetting(float volume = 1.0f)
    {
        Setting setting = new Setting(volume, false);
        setting.isPooling = true;
        return setting;
    }
}
