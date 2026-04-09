using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// =============================================================
// [JSON 파싱용] layerData.json 구조와 동일하게 맞추어야 함
// =============================================================
[System.Serializable]
class PSDLayerInfo
{
    public string fileName;
    public string objectName;
    public int layerIndex;
    public float centerX, centerY;  // 원본 레이어의 중심 좌표
    public float width, height;     // crop 후 이미지 크기
    public int opacity;             // 0 ~ 255
    public bool hasClipping;
}

[System.Serializable]
class PSDFileData
{
    public float psdWidth;
    public float psdHeight;
    public List<PSDLayerInfo> layers;
}

// =============================================================
// [메뉴] Tools > PSD Import > Import Extracted Folder
// [경로] Assets/Editor/ 폴더 안에 배치
// =============================================================
public class PSDLayerImporter
{
    // ── 메뉴 실행 ──
    [MenuItem("Tools/PSD Import/Import Extracted Folder")]
    static void ImportSelectedFolder()
    {
        // ───────────────────────────────────────────────────
        // 0단계: 폴더 선택 다이얼로그
        //        Application.dataPath = .../ProjectName/Assets
        //        projectRoot          = .../ProjectName/
        //        다이얼로그에서 받은 경로를 "Assets/..." 형태로 변환
        // ───────────────────────────────────────────────────
        string dataPath = Application.dataPath.Replace("\\", "/");
        string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);

        string selected = EditorUtility.OpenFolderPanel(
            "Extracted 폴더 선택 (layerData.json 포함)",
            dataPath,   // 기본 경로: Assets 폴더
            ""
        );

        if (string.IsNullOrEmpty(selected)) return;
        selected = selected.Replace("\\", "/");

        // Assets 폴더 안에 있는지 확인
        if (!selected.StartsWith(projectRoot + "Assets"))
        {
            EditorUtility.DisplayDialog("PSD Import", "Assets 폴더 안의 폴더를 선택해야 합니다.", "확인");
            return;
        }

        // layerData.json 존재 확인
        if (!File.Exists(Path.Combine(selected, "layerData.json")))
        {
            EditorUtility.DisplayDialog("PSD Import", "선택한 폴더에 layerData.json이 없습니다.", "확인");
            return;
        }

        // Asset 경로 변환 ("Assets/..." 형태로)
        string assetFolderPath = selected.Substring(projectRoot.Length);
        string fullFolderPath = selected;

        // ───────────────────────────────────────────────────
        // 1단계: JSON 로드 및 파싱
        // ───────────────────────────────────────────────────
        string jsonFullPath = Path.Combine(fullFolderPath, "layerData.json");
        if (!File.Exists(jsonFullPath))
        {
            EditorUtility.DisplayDialog("PSD Import", "layerData.json 파일을 찾을 수 없습니다.", "확인");
            return;
        }

        PSDFileData psdData = JsonUtility.FromJson<PSDFileData>(File.ReadAllText(jsonFullPath));
        if (psdData == null || psdData.layers == null || psdData.layers.Count == 0)
        {
            EditorUtility.DisplayDialog("PSD Import", "JSON 파싱 실패 또는 레이어 데이터가 비어있습니다.", "확인");
            return;
        }

        // ───────────────────────────────────────────────────
        // 2단계: TextureImporter 설정
        //        PNG를 Sprite (Single 모드)로 변경 후 reimport
        // ───────────────────────────────────────────────────
        foreach (var layer in psdData.layers)
        {
            string texPath = assetFolderPath + "/" + layer.fileName + ".png";
            TextureImporter ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (ti == null) continue;

            bool needsReimport = false;

            if (ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }

            if (ti.spriteImportMode != SpriteImportMode.Single)
            {
                ti.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }

            if (needsReimport)
            {
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            }
        }

        // ───────────────────────────────────────────────────
        // 3단계: Undo 등록 시작
        // ───────────────────────────────────────────────────
        Undo.SetCurrentGroupName("PSD Layer Import");

        // ───────────────────────────────────────────────────
        // 4단계: Canvas 생성
        //        참조 해상도 = PSD 문서 크기로 설정
        // ───────────────────────────────────────────────────
        GameObject canvasObj = CreateCanvas(psdData);
        Undo.RegisterCreatedObjectUndo(canvasObj, "PSD Import");

        // EventSystem이 없으면 생성 (Canvas를 코드로 만들면 자동 생성안됨)
        if (GameObject.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esObj, "PSD Import");
        }

        // ───────────────────────────────────────────────────
        // 5단계: 각 레이어를 Image로 생성
        //
        //   JSON 배열 순서 = PSD 상단(앞) → 하단(뒤)
        //   Unity Hierarchy = 아래쪽 자식이 앞에 렌더됨
        //   → 역순으로 추가하여 시각 순서 일치
        // ───────────────────────────────────────────────────
        int successCount = 0;
        int failCount = 0;

        for (int i = psdData.layers.Count - 1; i >= 0; i--)
        {
            var layerInfo = psdData.layers[i];
            string texPath = assetFolderPath + "/" + layerInfo.fileName + ".png";

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
            if (sprite == null)
            {
                Debug.LogWarning("[PSD Import] Sprite 로드 실패: " + texPath);
                failCount++;
                continue;
            }

            // GameObject 생성 및 부모 설정
            GameObject imgObj = new GameObject(layerInfo.objectName);
            Undo.RegisterCreatedObjectUndo(imgObj, "PSD Import");
            imgObj.transform.SetParent(canvasObj.transform, false);

            // Image 컴포넌트 설정
            Image image = imgObj.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            image.color = new Color(1f, 1f, 1f, layerInfo.opacity / 255.0f);

            // RectTransform 배치
            // JSON에 저장된 centerX, centerY는 PSD 원본 레이어의 중심 좌표
            // Canvas 중심 기준 상대 좌표로 변환
            RectTransform rect = imgObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);   // 중심 앵커
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);   // 중심 피벗
            rect.sizeDelta = new Vector2(layerInfo.width, layerInfo.height);

            // Canvas 중심이 (0, 0)이므로 PSD 중심에서 Canvas 중심을 뺀다
            float canvasCenterX = psdData.psdWidth * 0.5f;
            float canvasCenterY = psdData.psdHeight * 0.5f;

            rect.anchoredPosition = new Vector2(
                layerInfo.centerX - canvasCenterX,
                -(layerInfo.centerY - canvasCenterY)  // Y축 반전
            );

            successCount++;
        }

        // ───────────────────────────────────────────────────
        // 6단계: 정리
        // ───────────────────────────────────────────────────
        Selection.activeGameObject = canvasObj;

        string msg = "임포트 완료!\n"
                   + "성공: " + successCount + "개 / 전체: " + psdData.layers.Count + "개\n"
                   + "Canvas 참조 해상도: " + psdData.psdWidth + " x " + psdData.psdHeight;

        if (failCount > 0)
            msg += "\n\n경고: " + failCount + "개 레이어가 실패 → Console 확인 필요";

        EditorUtility.DisplayDialog("PSD Import", msg, "확인");
    }

    // ── Canvas 생성 헬퍼 ──────────────────────────────────
    static GameObject CreateCanvas(PSDFileData psdData)
    {
        GameObject canvasObj = new GameObject("Canvas_PSD");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // CanvasScaler
        // matchWidthOrHeight: 0 = 가로 기준, 1 = 세로 기준, 0.5 = 둘 다
        // 프로젝트 종경비에 따라 수동 조정하면 됨
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(psdData.psdWidth, psdData.psdHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        return canvasObj;
    }
}