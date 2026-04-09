using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CDPulseScaleObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public float pulseScale = 1.1f;
	public float duration = 0.5f;
	public Ease ease = Ease.InOutSine;

	Tween pulseTween;
	CDPressScaleObject pressScale;

	void Awake()
	{
		pressScale = GetComponent<CDPressScaleObject>();
		StartPulse();
	}

	void StartPulse()
	{
		pulseTween?.Kill();
		transform.localScale = Vector3.one;
		pulseTween = transform.DOScale(pulseScale, duration)
			.SetEase(ease)
			.SetLoops(-1, LoopType.Yoyo)
			.SetLink(gameObject);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pulseTween?.Kill();
		float target = pressScale != null ? pressScale.pressedScale : 0.8f;
		transform.DOScale(target, 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject)
			.OnComplete(StartPulse);
	}
}
