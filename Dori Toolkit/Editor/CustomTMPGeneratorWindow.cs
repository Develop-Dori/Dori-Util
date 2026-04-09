using UnityEngine;
using UnityEditor;
using TMPro;
using System.Linq; // 정렬 기능을 사용하기 위해 추가

/// <summary>
/// [도구 설명] 선택한 UI 오브젝트의 정보를 바탕으로 커스텀 TextMeshPro를 자동 생성하는 에디터 윈도우입니다.
/// [사용 방법]
/// 1. 메뉴 상단 GameObject -> UI -> Create Custom TMP Next to Selection 클릭하여 창 열기.
/// 2. 적용하고 싶은 Font Asset을 창의 필드에 할당 (선택 사항).
/// 3. 하이어라키에서 기준이 될 UI 오브젝트들 다중 선택.
/// 4. 'TMP 생성하기' 버튼 클릭.
/// [주요 기능]
/// - 하이어라키 상에서 선택한 다수의 오브젝트 바로 아래(동일 계층) 일괄 배치
/// - 부모 이름 및 위치 정보(Anchor, Pivot) 자동 복사
/// - 너비(Width)를 기준 오브젝트보다 10% 더 넓게 자동 설정
/// - 텍스트 상하좌우 중앙 정렬 및 Raycast Target 기본 해제
/// - Undo 기능 지원 (Ctrl+Z로 생성 취소 가능)
/// </summary>
public class CustomTMPGeneratorWindow : EditorWindow
{
	public TMP_FontAsset targetFont;

	[MenuItem("GameObject/UI/Create Custom TMP Next to Selection", false, 10)]
	public static void ShowWindow()
	{
		GetWindow<CustomTMPGeneratorWindow>("Custom TMP Tool");
	}

	private void OnGUI()
	{
		GUILayout.Label("선택한 UI의 바로 아래(동일 계층)에 TMP 생성", EditorStyles.boldLabel);

		targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", targetFont, typeof(TMP_FontAsset), false);

		EditorGUILayout.Space();

		if (GUILayout.Button("TMP 생성하기"))
		{
			CreateTMPNextToSelection();
		}
	}

	private void CreateTMPNextToSelection()
	{
		// 1. 선택된 오브젝트 가져오기
		GameObject[] selectedObjects = Selection.gameObjects;

		if (selectedObjects.Length == 0)
		{
			Debug.LogWarning("하이어라키에서 기준이 될 UI 오브젝트를 선택해주세요.");
			return;
		}

		// 2. 다중 선택 시 중간에 오브젝트가 삽입되면서 인덱스가 밀리는 것을 방지하기 위해
		// RectTransform이 있는 오브젝트만 필터링한 후, 하이어라키 순서의 역순(아래에서 위로) 정렬합니다.
		var sortedTargets = selectedObjects
				.Where(go => go.GetComponent<RectTransform>() != null)
				.OrderByDescending(go => go.transform.GetSiblingIndex())
				.ToList();

		if (sortedTargets.Count == 0)
		{
			Debug.LogWarning("선택된 오브젝트 중 UI 요소(RectTransform)가 없습니다.");
			return;
		}

		// 3. 역순으로 정렬된 리스트를 바탕으로 생성
		foreach (GameObject targetGO in sortedTargets)
		{
			RectTransform targetRect = targetGO.GetComponent<RectTransform>();

			// 일반 GameObject 대신 처음부터 RectTransform을 달고 생성하도록 하여 더 안전하게 처리합니다.
			GameObject tmpGO = new GameObject(targetGO.name, typeof(RectTransform));
			Undo.RegisterCreatedObjectUndo(tmpGO, "Create Custom TMP Next");

			// 부모 설정
			tmpGO.transform.SetParent(targetGO.transform.parent, false);

			// 인덱스 설정 (역순 정렬 덕분에 뒤에 생성되는 오브젝트의 인덱스에 영향을 주지 않음)
			int targetIndex = targetGO.transform.GetSiblingIndex();
			tmpGO.transform.SetSiblingIndex(targetIndex + 1);

			// RectTransform 설정 복사
			RectTransform tmpRect = tmpGO.GetComponent<RectTransform>();
			tmpRect.anchorMin = targetRect.anchorMin;
			tmpRect.anchorMax = targetRect.anchorMax;
			tmpRect.pivot = targetRect.pivot;
			tmpRect.anchoredPosition = targetRect.anchoredPosition;
			tmpRect.sizeDelta = new Vector2(targetRect.rect.width * 1.1f, targetRect.rect.height);

			// TMP 컴포넌트 추가 및 설정
			TextMeshProUGUI tmpText = tmpGO.AddComponent<TextMeshProUGUI>();
			tmpText.text = targetGO.name;
			tmpText.alignment = TextAlignmentOptions.Center;
			tmpText.raycastTarget = false;

			if (targetFont != null)
			{
				tmpText.font = targetFont;
			}
		}

		Debug.Log($"[TMP Tool] {sortedTargets.Count}개의 UI 오브젝트 아래에 TMP 생성을 완료했습니다.");
	}
}