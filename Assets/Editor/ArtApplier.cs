using UnityEditor;
using UnityEngine;

// Assets/Art 의 픽셀아트를 임포트 설정하고 프리팹의 단색 사각형을 교체한다.
// 프리팹 스케일은 1로 되돌리고, 크기는 콜라이더(와 타일 스프라이트 size)로 표현한다.
public static class ArtApplier
{
    const string ArtDir = "Assets/Art";

    [MenuItem("Tools/Swap/Apply Pixel Art")]
    public static void Apply()
    {
        PrefabBuilder.EnsurePrefabs();

        Setup("Prefabs/Player", "player", new Vector2(0.8f, 1f), false);
        Setup("Prefabs/Enemy", "enemy", new Vector2(0.8f, 1f), false);
        Setup("Prefabs/Box", "crate", Vector2.one, false);
        Setup("Prefabs/SwapPad", "pad", new Vector2(0.8f, 1f), false);
        Setup("Prefabs/Goal", "goal", new Vector2(0.8f, 1.5f), false);
        Setup("Prefabs/Block", "tile", Vector2.one, true);
        Setup("Prefabs/Spike", "spike", new Vector2(2f, 0.6f), true);

        AssetDatabase.SaveAssets();
        Debug.Log("Pixel art applied to prefabs");
    }

    static Sprite ImportSprite(string name)
    {
        string path = $"{ArtDir}/{name}.png";
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 16;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.wrapMode = TextureWrapMode.Repeat;

        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; // Tiled 그리기에 필요
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        imp.SetTextureSettings(settings);
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void Setup(string prefab, string spriteName, Vector2 size, bool tiled)
    {
        string path = $"Assets/{prefab}.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);

        root.transform.localScale = Vector3.one;

        var sr = root.GetComponent<SpriteRenderer>();
        sr.sprite = ImportSprite(spriteName);
        sr.color = Color.white;
        if (tiled)
        {
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = size;
        }
        else sr.drawMode = SpriteDrawMode.Simple;

        var col = root.GetComponent<BoxCollider2D>();
        col.size = size;
        col.offset = Vector2.zero;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
