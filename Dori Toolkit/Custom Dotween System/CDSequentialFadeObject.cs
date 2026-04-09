using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class CDSequentialFadeObject : MonoBehaviour
{
	[Tooltip("순차적으로 페이드할 오브젝트 리스트")]
	public List<Transform> targets = new();

	[Header("애니메이션 설정")]
	[Tooltip("페이드 인 시간")]
	public float duration = 0.3f;
	[Tooltip("각 오브젝트 사이 대기 시간")]
	public float interval = 0.5f;
	[Tooltip("루프 시 전체 페이드 아웃 시간")]
	public float fadeOutDuration = 0.3f;
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
	readonly List<CanvasGroup> canvasGroups = new();

	void OnEnable()
	{
		isFirstPlay = true;
		EnsureCanvasGroups();
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
		EnsureCanvasGroups();
		CreateSequence();
	}

	void OnValidate()
	{
		if (!Application.isPlaying) return;
		needsRecreate = true;
	}

	void EnsureCanvasGroups()
	{
		canvasGroups.Clear();
		foreach (var target in targets)
		{
			if (target == null) { canvasGroups.Add(null); continue; }

			var cg = target.GetComponent<CanvasGroup>();
			if (cg == null)
				cg = target.gameObject.AddComponent<CanvasGroup>();
			canvasGroups.Add(cg);
		}
	}

	void CreateSequence()
	{
		sequence?.Kill();
		currentLoop = 0;

		foreach (var cg in canvasGroups)
			if (cg != null) cg.alpha = 0f;

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

		// 순차 페이드 인
		for (int i = 0; i < canvasGroups.Count; i++)
		{
			var cg = canvasGroups[i];
			if (cg == null) continue;

			sequence.Append(cg.DOFade(1f, duration).SetEase(ease));
			if (i < canvasGroups.Count - 1)
				sequence.AppendInterval(interval);
		}

		if (isLastLoop)
		{
			// 마지막 루프: 밝은 상태로 끝
			sequence.OnComplete(() => OnAnimationComplete?.Invoke());
		}
		else
		{
			// 루프 계속: 대기 → 전체 페이드 아웃 → 다음 사이클
			sequence.AppendInterval(interval);

			for (int i = 0; i < canvasGroups.Count; i++)
			{
				var cg = canvasGroups[i];
				if (cg == null) continue;

				if (i == 0)
					sequence.Append(cg.DOFade(0f, fadeOutDuration).SetEase(ease));
				else
					sequence.Join(cg.DOFade(0f, fadeOutDuration).SetEase(ease));
			}

			sequence.AppendInterval(interval);

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
		foreach (var cg in canvasGroups)
			if (cg != null) cg.alpha = 0f;
		BuildCycle();
	}

	public void Pause() => sequence?.Pause();
}
