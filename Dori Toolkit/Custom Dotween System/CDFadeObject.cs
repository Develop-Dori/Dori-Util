using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CDFadeObject : MonoBehaviour
{
	[Tooltip("-1: 무한 반복")]
	public int loop = -1;
	public float duration = 1f;
	public Ease ease = Ease.Linear;
	public bool autoPlay = true;

	[Tooltip("true: 밝은 상태 → 어두워짐, false: 어두운 상태 → 밝아짐")]
	public bool reverse = false;

	[Header("이벤트")]
	public UnityEvent OnAnimationComplete;

	CanvasGroup canvasGroup;
	Tween fadeTween;
	bool needsRecreate;

	void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		CreateTween();
	}

	void Update()
	{
		if (!needsRecreate) return;
		needsRecreate = false;
		CreateTween();
	}

	void CreateTween()
	{
		fadeTween?.Kill();

		float from = reverse ? 1f : 0f;
		float to = reverse ? 0f : 1f;

		canvasGroup.alpha = from;

		fadeTween = canvasGroup
			.DOFade(to, duration)
			.SetEase(ease)
			.SetLoops(loop, LoopType.Yoyo)
			.SetAutoKill(false)
			.SetLink(gameObject)
			.OnComplete(() => OnAnimationComplete?.Invoke());

		if (!autoPlay)
			fadeTween.Pause();
	}

	void OnValidate()
	{
		if (!Application.isPlaying || canvasGroup == null) return;
		needsRecreate = true;
	}

	public void Play() => fadeTween?.Play();
	public void Pause() => fadeTween?.Pause();
}
