using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CDRotateObject : MonoBehaviour
{
	[Tooltip("-1: 무한 반복")]
	public int loop = -1;
	[Tooltip("한 바퀴(360도) 도는 시간")]
	public float duration = 2f;
	public Ease ease = Ease.Linear;
	public bool autoPlay = true;

	[Header("이벤트")]
	public UnityEvent OnAnimationComplete;

	Tween rotateTween;
	Vector3 originRotation;
	bool needsRecreate;

	void Awake()
	{
		originRotation = transform.localEulerAngles;
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
		rotateTween?.Kill();
		transform.localEulerAngles = originRotation;

		rotateTween = transform
			.DOLocalRotate(originRotation + new Vector3(0, 0, 360f), duration, RotateMode.FastBeyond360)
			.SetEase(ease)
			.SetLoops(loop, LoopType.Restart)
			.SetAutoKill(false)
			.SetLink(gameObject)
			.OnComplete(() => OnAnimationComplete?.Invoke());

		if (!autoPlay)
			rotateTween.Pause();
	}

	void OnValidate()
	{
		if (!Application.isPlaying) return;
		needsRecreate = true;
	}

	public void Play() => rotateTween?.Play();
	public void Pause() => rotateTween?.Pause();
}
