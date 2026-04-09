using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 독립형 UI 타이머 시스템.
/// 준비/출발 인트로, 카운트다운(5,4,3,2,1) 표시, 사운드 재생을 지원합니다.
/// 반드시 오버레이 프리팹을 할당해야 합니다.
///
/// 사용법:
/// 1. UI Timer System 프리팹을 씬에 배치
/// 2. Inspector에서 원하는 기능 on/off 및 사운드 할당
/// 3. 외부에서 StartTimer() 호출 (또는 autoPlay = true로 자동 시작)
/// 4. Default Resources 폴더의 오버레이 프리팹을 커스텀하여 UI를 변경 가능
/// </summary>
public class UITimerSystem : MonoBehaviour
{
	[Header("타이머 설정")]
	[Tooltip("타이머 지속 시간 (초)")]
	public float duration = 180f;

	[Tooltip("활성화 시 자동으로 타이머를 시작합니다")]
	public bool autoPlay = true;

	[Header("오버레이 UI")]
	[Tooltip("타이머 오버레이 프리팹 (필수)\n" +
		"프리팹에 UITimerOverlay 컴포넌트가 있어야 합니다")]
	public UITimerOverlay overlayPrefab;

	[Header("준비/출발 인트로")]
	[Tooltip("시작 전 준비/출발 연출 사용 여부")]
	public bool useReadyGo = true;
	[Tooltip("마스크 투명도 (0: 마스크 사용 안 함, 0 초과: 해당 투명도로 마스크 표시)")]
	[Range(0f, 1f)]
	public float maskAlpha = 0.5f;
	public string readyText = "준비";
	public string goText = "출발";
	[Tooltip("준비 텍스트 표시 시간")]
	public float readyDisplayTime = 1.5f;
	[Tooltip("출발 텍스트 표시 시간")]
	public float goDisplayTime = 1f;
	public AudioClip readyClip;
	public AudioClip goClip;

	[Header("카운트다운 효과 (5,4,3,2,1)")]
	[Tooltip("종료 전 숫자 카운트다운 연출 사용 여부")]
	public bool useCountdown = true;
	[Tooltip("카운트다운 시작 숫자")]
	public int countdownFrom = 5;
	public AudioClip tickClip;

	[Header("사운드 설정")]
	[Range(0f, 1f)]
	public float volume = 1f;

	[Header("이벤트")]
	[Tooltip("준비/출발 후 실제 타이머가 시작될 때")]
	public UnityEvent OnTimerStarted;
	[Tooltip("타이머가 0에 도달했을 때")]
	public UnityEvent OnTimerEnded;

	/// <summary>남은 시간 (초, float)</summary>
	public float RemainingTime => remainingTime;

	/// <summary>남은 시간 (정수, CeilToInt — 0.1초 남아도 1 반환)</summary>
	public int RemainingSeconds => isRunning ? Mathf.CeilToInt(remainingTime) : 0;

	/// <summary>타이머 실행 중 여부 (준비/출발 구간 제외, 실제 카운트 중일 때만 true)</summary>
	public bool IsRunning => isRunning;

	private bool UseMask => maskAlpha > 0f;

	private float remainingTime;
	private bool isRunning;
	private bool isPaused;

	private TMP_Text displayText;
	private Image maskImage;
	private AudioSource audioSource;
	private Coroutine activeRoutine;

	void Awake()
	{
		SetupAudio();
		SetupOverlay();
	}

	void Start()
	{
		if (autoPlay)
			StartTimer();
	}

	private void SetupAudio()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
	}

	private void SetupOverlay()
	{
		if (overlayPrefab == null)
		{
			Debug.LogError("[UITimerSystem] overlayPrefab이 할당되지 않았습니다! Default Resources 폴더의 프리팹을 할당해 주세요.");
			return;
		}

		var instance = Instantiate(overlayPrefab, transform);

		maskImage = instance.mask;
		displayText = instance.displayText;

		if (maskImage != null)
			maskImage.gameObject.SetActive(false);
		if (displayText != null)
			displayText.gameObject.SetActive(false);
	}

	/// <summary>
	/// 기본 duration으로 타이머를 시작합니다.
	/// </summary>
	public void StartTimer()
	{
		StartTimer(duration);
	}

	/// <summary>
	/// 지정된 시간으로 타이머를 시작합니다.
	/// </summary>
	public void StartTimer(float customDuration)
	{
		StopTimer();
		activeRoutine = StartCoroutine(TimerRoutine(customDuration));
	}

	/// <summary>
	/// 타이머를 중지합니다.
	/// </summary>
	public void StopTimer()
	{
		if (activeRoutine != null)
		{
			StopCoroutine(activeRoutine);
			activeRoutine = null;
		}
		isRunning = false;
		isPaused = false;
		remainingTime = 0f;
		HideText();
	}

	public void PauseTimer() => isPaused = true;
	public void ResumeTimer() => isPaused = false;

	#region 타이머 코루틴

	private IEnumerator TimerRoutine(float totalDuration)
	{
		// 1. 준비/출발 인트로
		if (useReadyGo)
		{
			if (UseMask) ShowMask();
			yield return TextFadeRoutine(readyText, readyDisplayTime, readyClip);
			yield return TextFadeRoutine(goText, goDisplayTime, goClip);
			if (UseMask) yield return FadeMask(maskAlpha, 0f, 0.3f);
			HideMask();
		}

		// 2. 타이머 시작
		isRunning = true;
		remainingTime = totalDuration;
		OnTimerStarted?.Invoke();

		int lastTick = -1;

		// 3. 카운트다운 루프
		while (remainingTime > 0f)
		{
			if (!isPaused)
			{
				remainingTime -= Time.deltaTime;

				if (useCountdown && remainingTime <= countdownFrom && remainingTime > 0f)
				{
					int tick = Mathf.CeilToInt(remainingTime);
					if (tick != lastTick && tick > 0 && tick <= countdownFrom)
					{
						lastTick = tick;
						StartCoroutine(CountdownNumberRoutine(tick));
					}
				}
			}
			yield return null;
		}

		// 4. 종료
		remainingTime = 0f;
		isRunning = false;
		HideText();
		OnTimerEnded?.Invoke();
		activeRoutine = null;
	}

	private IEnumerator TextFadeRoutine(string text, float showTime, AudioClip clip)
	{
		PlayClip(clip);

		displayText.text = text;
		displayText.rectTransform.localScale = Vector3.one;
		displayText.gameObject.SetActive(true);

		float fadeIn = 0.2f;
		float hold = Mathf.Max(0f, showTime - 0.5f);
		float fadeOut = 0.3f;

		yield return LerpAlpha(0f, 1f, fadeIn);
		yield return new WaitForSeconds(hold);
		yield return LerpAlpha(1f, 0f, fadeOut);

		displayText.gameObject.SetActive(false);
	}

	private IEnumerator CountdownNumberRoutine(int number)
	{
		PlayClip(tickClip);

		displayText.text = number.ToString();
		displayText.gameObject.SetActive(true);

		float animTime = 0.8f;
		float elapsed = 0f;

		while (elapsed < animTime)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / animTime;

			float scale = Mathf.Lerp(1.5f, 1f, t);
			displayText.rectTransform.localScale = Vector3.one * scale;

			float alpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
			SetAlpha(alpha);

			yield return null;
		}

		displayText.gameObject.SetActive(false);
		displayText.rectTransform.localScale = Vector3.one;
	}

	#endregion

	#region 유틸

	private IEnumerator LerpAlpha(float from, float to, float time)
	{
		float elapsed = 0f;
		while (elapsed < time)
		{
			elapsed += Time.deltaTime;
			SetAlpha(Mathf.Lerp(from, to, elapsed / time));
			yield return null;
		}
		SetAlpha(to);
	}

	private void SetAlpha(float alpha)
	{
		var c = displayText.color;
		c.a = alpha;
		displayText.color = c;
	}

	private void HideText()
	{
		if (displayText != null)
			displayText.gameObject.SetActive(false);
	}

	private void ShowMask()
	{
		if (maskImage == null) return;
		maskImage.color = new Color(0, 0, 0, maskAlpha);
		maskImage.gameObject.SetActive(true);
	}

	private void HideMask()
	{
		if (maskImage != null)
			maskImage.gameObject.SetActive(false);
	}

	private IEnumerator FadeMask(float from, float to, float time)
	{
		if (maskImage == null) yield break;
		float elapsed = 0f;
		while (elapsed < time)
		{
			elapsed += Time.deltaTime;
			float a = Mathf.Lerp(from, to, elapsed / time);
			maskImage.color = new Color(0, 0, 0, a);
			yield return null;
		}
		maskImage.color = new Color(0, 0, 0, to);
	}

	private void PlayClip(AudioClip clip)
	{
		if (clip != null && audioSource != null)
		{
			audioSource.volume = volume;
			audioSource.PlayOneShot(clip);
		}
	}

	#endregion
}
