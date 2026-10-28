using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrefabBuilder
{
    const string Dir = "Assets/Prefabs";

    [MenuItem("Tools/Swap/Build Prefabs")]
    public static void EnsurePrefabs()
    {
        PrototypeSceneBuilder.PrepareAssets();
        Directory.CreateDirectory(Dir);

        Make("Block", Vector2.one, new Color(0.55f, 0.57f, 0.62f), 0, _ => { });

        Make("Player", new Vector2(0.8f, 1f), new Color(0.3f, 0.6f, 1f), 10, go =>
        {
            go.AddComponent<PlayerController>();
            var sc = go.AddComponent<SwapController>();
            sc.player = go.GetComponent<PlayerController>();
            sc.squareSprite = PrototypeSceneBuilder.square;
            sc.unlitMaterial = PrototypeSceneBuilder.unlit;
        });

        Make("Box", Vector2.one, new Color(1f, 0.55f, 0.25f), 5, go =>
        {
            go.AddComponent<SwapBox>();
            go.GetComponent<Rigidbody2D>().gravityScale = 3f;
        });

        Make("Enemy", new Vector2(0.8f, 1f), new Color(0.7f, 0.3f, 0.9f), 5, go =>
        {
            go.AddComponent<SwapEnemy>();
            go.AddComponent<EnemyPatrol>();
            go.GetComponent<Rigidbody2D>().gravityScale = 3f;
        });

        Make("SwapPad", new Vector2(0.8f, 1f), new Color(0.2f, 1f, 0.9f, 0.45f), 4, go =>
        {
            go.AddComponent<SwapPad>();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
        });

        Make("Spike", new Vector2(2f, 0.6f), new Color(0.95f, 0.25f, 0.25f), 5, go =>
        {
            go.AddComponent<Hazard>();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
        });

        Make("Goal", new Vector2(0.8f, 1.5f), new Color(0.3f, 0.9f, 0.4f), 4, go =>
        {
            go.AddComponent<Goal>();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
        });

        AssetDatabase.SaveAssets();
    }

    static void Make(string name, Vector2 size, Color color, int order, System.Action<GameObject> setup)
    {
        string path = $"{Dir}/{name}.prefab";
        if (File.Exists(path)) return;

        var go = PrototypeSceneBuilder.Sprited(name, size, color, order);
        setup(go);
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    public static GameObject Spawn(string prefabName, Transform parent, Vector2 pos, Vector2? size = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir}/{prefabName}.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        if (size.HasValue)
        {
            // 타일 스프라이트(블록·가시)는 늘리지 않고 반복해서 그리도록 크기만 바꾼다.
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.drawMode == SpriteDrawMode.Tiled)
            {
                sr.size = size.Value;
                go.GetComponent<BoxCollider2D>().size = size.Value;
            }
            else go.transform.localScale = new Vector3(size.Value.x, size.Value.y, 1f);
        }
        return go;
    }

    public static void CreateGameSystem(SwapController swap, int stageNumber, int swapCount, string hint)
    {
        var sys = new GameObject("GameSystem");
        var info = sys.AddComponent<StageInfo>();
        info.stageNumber = stageNumber;
        info.swapCount = swapCount;
        info.hint = hint;
        var gm = sys.AddComponent<GameManager>();
        gm.swap = swap;
        gm.stage = info;
    }
}
