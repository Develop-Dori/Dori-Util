using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Timer System 오버레이 프리팹에 부착하는 컴포넌트.
/// Mask와 DisplayText 참조를 직접 할당하여 오브젝트 이름 변경에도 참조가 유지됩니다.
/// </summary>
public class UITimerOverlay : MonoBehaviour
{
    [Tooltip("전체 화면 마스크 이미지")]
    public Image mask;

    [Tooltip("타이머 텍스트 (TMP_Text)")]
    public TMP_Text displayText;
}
