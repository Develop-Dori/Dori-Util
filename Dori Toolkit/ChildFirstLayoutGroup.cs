using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ContentSizeFitter가 부모→자식 순서로 계산되어 중앙 정렬이 깨지는 문제를 해결합니다.
/// 부모 오브젝트에 이 컴포넌트를 추가하면, 매 프레임 자식 레이아웃을 먼저 강제 갱신한 뒤
/// 자신의 레이아웃을 갱신하여 올바른 크기가 반영됩니다.
///
/// 사용법:
/// 1. 부모 오브젝트(Score)에 이 컴포넌트 추가
/// 2. 기존 ContentSizeFitter는 그대로 유지
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ChildFirstLayoutGroup : MonoBehaviour
{
	private RectTransform rectTransform;

	void OnEnable()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	void LateUpdate()
	{
		// 자식 ContentSizeFitter를 먼저 강제 갱신
		foreach (Transform child in transform)
		{
			var childFitter = child.GetComponent<ContentSizeFitter>();
			if (childFitter != null)
				LayoutRebuilder.ForceRebuildLayoutImmediate(child as RectTransform);
		}

		// 그 후 자신 갱신
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}
}
