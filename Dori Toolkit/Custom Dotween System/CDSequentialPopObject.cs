using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CDSequentialPopObject : MonoBehaviour
{
	[Tooltip("순차적으로 강조할 오브젝트 리스트")]
	public List<Transform> targets = new();

	[Header("애니메이션 설정")]
	[Tooltip("강조 시 커지는 스케일")]
	public float popScale = 1.2f;
	[Tooltip("커지는 시간")]
	public float duration = 0.3f;
	[Tooltip("각 오브젝트 사이 대기 시간")]
	public float interval = 1f;
	[Tooltip("최초 1회 시작 지연 시간 (OnEnable 시 적용)")]
	public float startDelay = 0f;
	public Ease ease = Ease.Linear;

	[Header("루프 설정")]
	[Tooltip("-1: 무한 반복, 1: 1회, 2: 2회 ...")]
	public int loopCount = 1;

	public bool autoPlay = true;

	[Header("이벤트")]
	public UnityEvent OnAnimationComplete;

	Sequence sequence;
	bool needsRecreate;
	bool isFirstPlay;
	int currentLoop;

	void OnEnable()
	{
		isFirstPlay = true;
		CreateSequence();
	}

	void OnDisable()
	{
		sequence?.Kill();
		sequence = null;
	}

	void Update()
	{
		if (!needsRecreate) return;
		needsRecreate = false;
		isFirstPlay = true;
		CreateSequence();
	}

	void OnValidate()
	{
		if (!Application.isPlaying) return;
		needsRecreate = true;
	}

	void CreateSequence()
	{
		sequence?.Kill();
		currentLoop = 0;

		foreach (var target in targets)
			if (target != null) target.localScale = Vector3.one;

		BuildCycle();
	}

	void BuildCycle()
	{
		sequence?.Kill();

		bool isLastLoop = loopCount > 0 && currentLoop >= loopCount - 1;

		sequence = DOTween.Sequence()
			.SetLink(gameObject)
			.SetAutoKill(true);

		if (isFirstPlay && startDelay > 0f)
			sequence.AppendInterval(startDelay);
		isFirstPlay = false;

		foreach (var target in targets)
		{
			if (target == null) continue;

			sequence.Append(target.DOScale(popScale, duration).SetEase(ease));
			sequence.Append(target.DOScale(1f, duration).SetEase(Ease.InOutSine));
			sequence.AppendInterval(interval);
		}

		if (isLastLoop)
		{
			sequence.OnComplete(() => OnAnimationComplete?.Invoke());
		}
		else
		{
			sequence.OnComplete(() =>
			{
				currentLoop++;
				BuildCycle();
			});
		}

		if (!autoPlay)
			sequence.Pause();
	}

	public void Play()
	{
		isFirstPlay = true;
		currentLoop = 0;
		foreach (var target in targets)
			if (target != null) target.localScale = Vector3.one;
		BuildCycle();
	}

	public void Pause() => sequence?.Pause();
}
