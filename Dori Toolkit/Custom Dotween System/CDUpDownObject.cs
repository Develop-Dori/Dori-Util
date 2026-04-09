using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CDUpDownObject : MonoBehaviour
{
	[Tooltip("-1: 무한 반복")]
	public int loop = -1;
	[Tooltip("위아래 총 이동 거리 (위로 절반, 아래로 절반)")]
	public float range = 100f;
	public float duration = 1f;
	public Ease ease = Ease.Linear;
	public bool autoPlay = true;

	[Header("이벤트")]
	public UnityEvent OnAnimationComplete;

	RectTransform rectTransform;
	Tween moveTween;
	float originY;
	bool needsRecreate;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		originY = rectTransform.anchoredPosition.y;
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
		moveTween?.Kill();

		float half = range / 2f;

		var pos = rectTransform.anchoredPosition;
		pos.y = originY - half;
		rectTransform.anchoredPosition = pos;

		moveTween = rectTransform
			.DOAnchorPosY(originY + half, duration)
			.SetEase(ease)
			.SetLoops(loop, LoopType.Yoyo)
			.SetAutoKill(false)
			.SetLink(gameObject)
			.OnComplete(() => OnAnimationComplete?.Invoke());

		if (!autoPlay)
			moveTween.Pause();
	}

	void OnValidate()
	{
		if (!Application.isPlaying || rectTransform == null) return;
		needsRecreate = true;
	}

	public void Play() => moveTween?.Play();
	public void Pause() => moveTween?.Pause();
}
