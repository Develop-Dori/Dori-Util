using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TouchSystem : MonoBehaviour
{
	[Header("파티클 설정")]
	[Tooltip("터치 시 재생할 ParticleSystem 프리팹")]
	public ParticleSystem particlePrefab;

	[Tooltip("카메라로부터의 파티클 거리")]
	public float distanceFromCamera = 10f;

	[Header("사운드 설정")]
	[Tooltip("터치 시 재생할 오디오 클립")]
	public AudioClip touchSound;

	[Tooltip("터치 사운드 볼륨")]
	[Range(0f, 1f)]
	public float soundVolume = 1f;

	[Header("풀링 설정")]
	[Tooltip("동시에 재생 가능한 최대 파티클 수")]
	public int poolSize = 5;

	[Header("입력 설정")]
	[Tooltip("체크 해제하면 PlayAt()을 직접 호출해서 사용")]
	public bool autoDetectInput = true;

	[Header("렌더링 설정")]
	[Tooltip("터치 파티클 전용 레이어 번호 (다른 오브젝트와 겹치지 않는 레이어 사용)")]
	public int particleLayer = 31;

	[Tooltip("RawImage에 적용할 머터리얼 (비워두면 Additive 셰이더 자동 적용)")]
	public Material overlayMaterial;

	ParticleSystem[] pool;
	int poolIndex;
	Camera particleCamera;
	RenderTexture renderTexture;
	AudioSource audioSource;

	void Start()
	{
		if (particlePrefab == null)
		{
			Debug.LogWarning("[TouchSystem] particlePrefab이 할당되지 않았습니다.");
			enabled = false;
			return;
		}

		SetupRenderPipeline();
		SetupPool();
		SetupAudio();
	}

	void SetupRenderPipeline()
	{
		var camGO = new GameObject("TouchParticle_Camera");
		camGO.transform.SetParent(transform);
		particleCamera = camGO.AddComponent<Camera>();
		particleCamera.cullingMask = 1 << particleLayer;
		particleCamera.clearFlags = CameraClearFlags.SolidColor;
		particleCamera.backgroundColor = Color.clear;
		particleCamera.depth = -100;

		if (Camera.main != null)
		{
			var main = Camera.main;
			particleCamera.fieldOfView = main.fieldOfView;
			particleCamera.nearClipPlane = main.nearClipPlane;
			particleCamera.farClipPlane = main.farClipPlane;
			particleCamera.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
			main.cullingMask &= ~(1 << particleLayer);
		}

		renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		renderTexture.Create();
		particleCamera.targetTexture = renderTexture;

		var canvasGO = new GameObject("TouchParticle_Overlay");
		canvasGO.transform.SetParent(transform);
		var canvas = canvasGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32767;

		var rawImageGO = new GameObject("TouchParticle_Display");
		rawImageGO.transform.SetParent(canvasGO.transform, false);
		var rawImage = rawImageGO.AddComponent<RawImage>();
		rawImage.texture = renderTexture;
		rawImage.raycastTarget = false;

		var rt = rawImage.rectTransform;
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;

		if (overlayMaterial == null)
		{
			var shader = Shader.Find("Hidden/TouchParticleOverlay");
			if (shader != null)
				overlayMaterial = new Material(shader);
			else
				Debug.LogWarning("[TouchSystem] Hidden/TouchParticleOverlay 셰이더를 찾을 수 없습니다.");
		}

		if (overlayMaterial != null)
			rawImage.material = overlayMaterial;
	}

	void SetupPool()
	{
		pool = new ParticleSystem[poolSize];
		var poolParent = new GameObject("TouchParticle_Pool").transform;
		poolParent.SetParent(transform);

		for (int i = 0; i < poolSize; i++)
		{
			pool[i] = Instantiate(particlePrefab, poolParent);
			SetLayerRecursive(pool[i].gameObject, particleLayer);
			pool[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			pool[i].gameObject.SetActive(false);
		}
	}

	void SetupAudio()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 0f; // 2D 사운드
	}

	void SetLayerRecursive(GameObject go, int layer)
	{
		go.layer = layer;
		foreach (Transform child in go.transform)
			SetLayerRecursive(child.gameObject, layer);
	}

	void LateUpdate()
	{
		if (particleCamera != null && Camera.main != null)
		{
			var main = Camera.main;
			particleCamera.transform.SetPositionAndRotation(main.transform.position, main.transform.rotation);
			particleCamera.fieldOfView = main.fieldOfView;
		}
	}

	void Update()
	{
		if (!autoDetectInput) return;
		if (Pointer.current == null) return;
		if (!Pointer.current.press.wasPressedThisFrame) return;

		PlayAt(Pointer.current.position.ReadValue());
	}

	/// <summary>
	/// 스크린 좌표에서 파티클을 재생합니다.
	/// </summary>
	public void PlayAt(Vector2 screenPosition)
	{
		if (pool == null || particleCamera == null) return;

		var ps = pool[poolIndex];
		poolIndex = (poolIndex + 1) % pool.Length;

		Vector3 worldPos = particleCamera.ScreenToWorldPoint(
			new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera)
		);

		ps.gameObject.SetActive(true);
		ps.transform.position = worldPos;
		ps.Clear();
		ps.Play();

		if (touchSound != null)
			audioSource.PlayOneShot(touchSound, soundVolume);
	}

	void OnDestroy()
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
			Destroy(renderTexture);
		}
	}
}
