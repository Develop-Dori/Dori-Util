using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CDPressScaleObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public float pressedScale = 0.8f;

	bool hasPulse;

	void Awake()
	{
		hasPulse = GetComponent<CDPulseScaleObject>() != null;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (hasPulse) return;
		transform.DOScale(pressedScale, 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (hasPulse) return;
		transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject);
	}
}
