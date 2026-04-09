using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 사용자 입력이 일정 시간 없으면 "첫 화면 복귀" 이벤트를 발생시키는 모듈.
///
/// 사용법:
///   var idle = DCore.GetModule&lt;MD_IdleReturn&gt;();
///   idle.SetTimeout(15f);
///   idle.OnUserActivity += () => Debug.Log("사용자 활동 감지");
///   idle.OnIdleTimeout += () => SceneManager.LoadScene("HomeScene");
///   idle.Activate();
/// </summary>
public class MD_IdleReturn : IDModuleBase
{
	float timeout = 10f;
	float lastActivityTime;
	bool isActive;
	bool isIdleTriggered;
	bool detectMouseMove;
	int lastReportFrame = -1;

	DCore_Object coreObj;
	Coroutine idleCoroutine;

	/// <summary>
	/// 사용자 입력(터치, 키보드, 마우스)이 감지될 때 호출됩니다.
	/// </summary>
	public event UnityAction OnUserActivity;

	/// <summary>
	/// 타임아웃 시간 동안 사용자 입력이 없어 첫 화면으로 복귀해야 할 때 호출됩니다.
	/// </summary>
	public event UnityAction OnIdleTimeout;

	public float Timeout => timeout;
	public bool IsActive => isActive;
	public bool IsIdle => isIdleTriggered;

	/// <summary>
	/// 마지막 입력 이후 경과 시간 (초)
	/// </summary>
	public float ElapsedSinceLastActivity => Time.time - lastActivityTime;

	public void Initialize()
	{
		coreObj = DCore.CreateCoreObject("IdleReturnObject");
		GameObject.DontDestroyOnLoad(coreObj.gameObject);
		lastActivityTime = Time.time;
	}

	/// <summary>
	/// 타임아웃 시간을 설정합니다 (초 단위).
	/// </summary>
	public void SetTimeout(float seconds)
	{
		timeout = Mathf.Max(1f, seconds);
	}

	/// <summary>
	/// 마우스 움직임도 사용자 활동으로 감지할지 설정합니다.
	/// 터치스크린 환경에서는 false 권장 (기본값: false).
	/// </summary>
	public void SetDetectMouseMove(bool enabled)
	{
		detectMouseMove = enabled;
	}

	/// <summary>
	/// Idle 감지를 시작합니다.
	/// </summary>
	public void Activate()
	{
		if (isActive) return;

		isActive = true;
		isIdleTriggered = false;
		lastActivityTime = Time.time;

		if (idleCoroutine != null)
			coreObj.StopCoroutine(idleCoroutine);
		idleCoroutine = coreObj.StartCoroutine(IdleCheckLoop());
	}

	/// <summary>
	/// Idle 감지를 중단합니다.
	/// </summary>
	public void Deactivate()
	{
		isActive = false;

		if (idleCoroutine != null)
		{
			coreObj.StopCoroutine(idleCoroutine);
			idleCoroutine = null;
		}
	}

	/// <summary>
	/// 외부에서 직접 "사용자 활동"을 보고할 때 사용합니다.
	/// (예: 특정 UI 버튼 클릭, 커스텀 입력 등)
	/// </summary>
	public void ReportActivity()
	{
		if (!isActive) return;

		// 같은 프레임에서 중복 호출 방지
		if (lastReportFrame == Time.frameCount) return;
		lastReportFrame = Time.frameCount;

		lastActivityTime = Time.time;

		if (isIdleTriggered)
		{
			isIdleTriggered = false;
		}

		OnUserActivity?.Invoke();
	}

	IEnumerator IdleCheckLoop()
	{
		while (isActive)
		{
			if (DetectAnyInput())
			{
				ReportActivity();
			}

			if (!isIdleTriggered && Time.time - lastActivityTime >= timeout)
			{
				isIdleTriggered = true;
				OnIdleTimeout?.Invoke();
			}

			yield return null;
		}
	}

	bool DetectAnyInput()
	{
		// 키보드/마우스 버튼 입력
		if (Input.anyKeyDown)
			return true;

		// 마우스 이동 (터치스크린 환경에서는 SetDetectMouseMove(false)로 비활성화)
		if (detectMouseMove && (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f))
			return true;

		// 터치 입력
		if (Input.touchCount > 0)
			return true;

		return false;
	}
}
