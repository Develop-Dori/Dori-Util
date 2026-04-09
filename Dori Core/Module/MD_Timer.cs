using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MD_Timer : IDModuleBase
{
	DCore_Object timerObj;
	Dictionary<string, TimerInfo> timerMap = new Dictionary<string, TimerInfo>();
	List<TimerInfo> poolingTimer = new List<TimerInfo>();
	bool isSystemActive = true;

	public class TimerInfo
	{
		public float duration;
		public float timeElapsed;
		public UnityAction endCallback;
		public GameObject linkedObj;
		public bool isActive;
		public bool isPaused;

		public TimerInfo()
		{
			Init();
		}

		public void Init()
		{
			duration = 0f;
			timeElapsed = 0f;
			isActive = false;
			isPaused = false;
			endCallback = null;
			linkedObj = null;
		}

		public void EndTimer()
		{
			timeElapsed = 0;
			isActive = false;
			isPaused = false;
			endCallback?.Invoke();
		}

		public void SetLinkedObject(GameObject obj)
		{
			this.linkedObj = obj;
		}
	}

	public void Initialize()
	{
		if (timerObj == null)
			timerObj = DCore.CreateCoreObject("TimerObject");
		timerObj.StartCoroutine(TimerUpdateLoop());
	}

	public void SystemActive(bool isActive)
	{
		isSystemActive = isActive;
	}

	public void StartTimer(string timerName, float duration, UnityAction endCallback, GameObject linkedObject = null)
	{
		if (timerMap.ContainsKey(timerName))
		{
			StopTimer(timerName);
		}

		TimerInfo info;

		if (poolingTimer.Count > 0)
		{
			int lastIndex = poolingTimer.Count - 1;
			info = poolingTimer[lastIndex];
			poolingTimer.RemoveAt(lastIndex);
			info.Init();
		}
		else
		{
			info = new TimerInfo();
		}

		info.isActive = true;
		info.duration = duration;
		info.endCallback = endCallback;
		SetLinkedObject(timerName, linkedObject);

		timerMap.Add(timerName, info);
	}

	public void SetLinkedObject(string timerName, GameObject obj)
	{
		if (timerMap.ContainsKey(timerName))
		{
			timerMap[timerName].SetLinkedObject(obj);
		}
	}

	public void StopTimer(string timerName, bool invokeCallback = false)
	{
		if (timerMap.ContainsKey(timerName))
		{
			TimerInfo info = timerMap[timerName];
			timerMap.Remove(timerName);

			if (invokeCallback)
				info.EndTimer();

			info.Init();
			poolingTimer.Add(info);
		}
	}

	public void PauseTimer(string timerName, bool isPaused)
	{
		if (timerMap.TryGetValue(timerName, out TimerInfo info))
		{
			info.isPaused = isPaused;
		}
	}

	public void AllRemoveTimer()
	{
		isSystemActive = false;
		foreach (var key in new List<string>(timerMap.Keys))
		{
			TimerInfo info = timerMap[key];
			info.Init();
			poolingTimer.Add(info);
		}
		timerMap.Clear();

		if (timerObj != null)
		{
			GameObject.Destroy(timerObj.gameObject);
			Object.Destroy(timerObj);
			timerObj = null;
		}

		poolingTimer.Clear();
	}

	public float GetNormalizeValue(string timerName, bool isReverse)
	{
		float value = 0;

		if (timerMap.ContainsKey(timerName))
		{
			value = timerMap[timerName].timeElapsed / timerMap[timerName].duration;
			if (isReverse)
				value = 1 - value;
		}
		return value;
	}

	public float GetCurrentTimerValue(string timerName)
	{
		float value = 0;

		if (timerMap.ContainsKey(timerName))
		{
			value = timerMap[timerName].timeElapsed;
		}

		return value;
	}

	public float GetRemainingTime(string timerName)
	{
		if (timerMap.TryGetValue(timerName, out TimerInfo info))
			return Mathf.Max(0f, info.duration - info.timeElapsed);
		return 0f;
	}

	public int GetRemainingSeconds(string timerName)
	{
		float remaining = GetRemainingTime(timerName);
		return remaining > 0f ? Mathf.CeilToInt(remaining) : 0;
	}

	public bool IsTimerRunning(string timerName)
	{
		return timerMap.ContainsKey(timerName) && timerMap[timerName].isActive;
	}

	IEnumerator TimerUpdateLoop()
	{
		List<string> keysToRemove = new List<string>();
		List<UnityAction> deferredCallbacks = new List<UnityAction>();

		while (isSystemActive)
		{
			float deltaTime = Time.deltaTime;

			keysToRemove.Clear();
			deferredCallbacks.Clear();

			foreach (string key in new List<string>(timerMap.Keys))
			{
				if (!timerMap.TryGetValue(key, out TimerInfo info))
					continue;

				if (info.linkedObj != null)
				{
					if (info.linkedObj == null || !info.linkedObj.activeInHierarchy)
					{
						info.endCallback = null;
						info.isActive = false;
						keysToRemove.Add(key);
						continue;
					}
				}

				if (info.isActive && !info.isPaused)
				{
					if (info.timeElapsed < info.duration)
					{
						info.timeElapsed += deltaTime;
					}
					else
					{
						if (info.endCallback != null)
							deferredCallbacks.Add(info.endCallback);
						info.isActive = false;
						keysToRemove.Add(key);
					}
				}
			}

			for (int i = 0; i < keysToRemove.Count; i++)
			{
				if (timerMap.TryGetValue(keysToRemove[i], out TimerInfo info))
				{
					info.Init();
					poolingTimer.Add(info);
					timerMap.Remove(keysToRemove[i]);
				}
			}

			// 콜백은 딕셔너리 열거 완료 후 호출 (콜백에서 StartTimer 호출 시 안전)
			for (int i = 0; i < deferredCallbacks.Count; i++)
			{
				deferredCallbacks[i].Invoke();
			}

			yield return null;
		}
	}
}
