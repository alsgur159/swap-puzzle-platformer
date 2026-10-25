using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PrototypeSceneBuilder
{
    const string SpritePath = "Assets/Sprites/Square.png";
    const string MaterialPath = "Assets/Materials/SpriteUnlit.mat";
    const string ScenePath = "Assets/Scenes/Prototype.unity";

    internal static Sprite square;
    internal static Material unlit;

    [MenuItem("Tools/Swap/Build Prototype Scene")]
    public static void Build()
    {
        PrepareAssets();
        PrefabBuilder.EnsurePrefabs();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildCamera();

        var map = new GameObject("Map").transform;
        PrefabBuilder.Spawn("Block", map, new Vector2(0, -4), new Vector2(20, 1)).name = "Floor";
        PrefabBuilder.Spawn("Block", map, new Vector2(-10, 1.5f), new Vector2(1, 11)).name = "WallL";
        PrefabBuilder.Spawn("Block", map, new Vector2(10, 1.5f), new Vector2(1, 11)).name = "WallR";
        PrefabBuilder.Spawn("Block", map, new Vector2(-7.75f, 0f), new Vector2(3.5f, 0.5f)).name = "LedgeLeft";
        PrefabBuilder.Spawn("Block", map, new Vector2(6.75f, 0.5f), new Vector2(5.5f, 0.5f)).name = "LedgeRight";

        var player = PrefabBuilder.Spawn("Player", null, new Vector2(-3, -3));
        PrefabBuilder.Spawn("Box", null, new Vector2(4.5f, 1.25f));
        PrefabBuilder.Spawn("Enemy", null, new Vector2(7f, -3f));
        PrefabBuilder.Spawn("SwapPad", null, new Vector2(-6.6f, 0.75f));
        PrefabBuilder.Spawn("Spike", null, new Vector2(1f, -3.2f));
        PrefabBuilder.Spawn("Goal", null, new Vector2(8.5f, 1.5f));

        PrefabBuilder.CreateGameSystem(player.GetComponent<SwapController>(), 1, 3, "");

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("Prototype scene built: " + ScenePath);
    }

    internal static void PrepareAssets()
    {
        Directory.CreateDirectory("Assets/Sprites");
        Directory.CreateDirectory("Assets/Materials");

        if (!File.Exists(SpritePath))
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var px = new Color[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SpritePath);

            var imp = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            imp.textureType = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 16;
            imp.filterMode = FilterMode.Point;
            imp.SaveAndReimport();
        }
        square = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);

        unlit = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (unlit == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            unlit = new Material(shader);
            AssetDatabase.CreateAsset(unlit, MaterialPath);
        }
    }

    internal static void BuildCamera()
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.transform.position = new Vector3(0f, 1.5f, -10f);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.13f, 0.14f, 0.18f);
        go.AddComponent<AudioListener>();
    }

    internal static GameObject Sprited(string name, Vector2 size, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.sharedMaterial = unlit;
        sr.color = color;
        sr.sortingOrder = order;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go;
    }
}
