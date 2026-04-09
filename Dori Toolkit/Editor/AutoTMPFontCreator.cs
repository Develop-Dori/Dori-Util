using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel; // GlyphRenderMode를 위해 필요

/// <summary>
/// [도구 설명] TextMeshPro용 SDF 폰트 에셋을 4096x4096 사이즈로 자동 생성하는 에디터 스크립트입니다.
/// [사용 방법] 
/// 1. Project 창에서 원본 폰트 파일(.ttf, .otf)을 선택합니다.
/// 2. 마우스 우클릭 -> Create -> TextMeshPro -> Auto Create Font Asset (4096) 클릭.
/// 3. 원본과 동일한 경로에 '폰트명 SDF'라는 에셋이 생성됩니다.
/// </summary>
public class AutoTMPFontCreator
{
	[MenuItem("Assets/Create/TextMeshPro/Auto Create Font Asset (4096)", false, 100)]
	public static void CreateFont4096()
	{
		foreach (Object obj in Selection.objects)
		{
			Font sourceFont = obj as Font;
			if (sourceFont != null)
			{
				// 1. 4096 사이즈로 폰트 에셋 메모리 상에 생성
				TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont, 90, 9, GlyphRenderMode.SDFAA, 4096, 4096);

				if (fontAsset != null)
				{
					string path = AssetDatabase.GetAssetPath(sourceFont);
					string newPath = path.Replace(".ttf", " SDF.asset").Replace(".otf", " SDF.asset");

					// 2. 폰트 에셋 껍데기 파일 생성
					AssetDatabase.CreateAsset(fontAsset, newPath);

					// 3. [핵심 수정] 생성된 텍스처와 머티리얼을 폰트 에셋의 하위(Sub-asset)로 추가하여 저장
					if (fontAsset.atlasTexture != null)
					{
						fontAsset.atlasTexture.name = sourceFont.name + " Atlas";
						AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
					}

					if (fontAsset.material != null)
					{
						fontAsset.material.name = sourceFont.name + " Material";
						AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
					}

					// 4. 최종 디스크 저장
					AssetDatabase.SaveAssets();
					Debug.Log($"[TMP] 4096 사이즈 폰트 에셋 생성 완료 (텍스처 포함): {newPath}");
				}
			}
		}
	}

	[MenuItem("Assets/Create/TextMeshPro/Auto Create Font Asset (4096)", true)]
	public static bool ValidateCreateFont4096()
	{
		return Selection.activeObject is Font;
	}
}