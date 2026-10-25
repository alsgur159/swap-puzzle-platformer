using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TutorialStageBuilder
{
    [MenuItem("Tools/Swap/Build Tutorial Stages (1-3)")]
    public static void BuildAll()
    {
        PrefabBuilder.EnsurePrefabs();
        Directory.CreateDirectory("Assets/Scenes");

        var paths = new[]
        {
            Make(1, 0, "A/D 이동, Space 점프 - 가시를 넘어 초록색 출구로 가세요", Stage1),
            Make(2, 3, "청록색 발판을 클릭하면 그 자리로 이동해요. 발판은 제자리에 남아요", Stage2),
            Make(3, 3, "벽에 가려지거나 너무 먼 대상은 교환할 수 없어요 (조준선이 빨갛게 변함)", Stage3),
        };

        var list = new EditorBuildSettingsScene[paths.Length];
        for (int i = 0; i < paths.Length; i++) list[i] = new EditorBuildSettingsScene(paths[i], true);
        EditorBuildSettings.scenes = list;

        AssetDatabase.SaveAssets();
        Debug.Log("Tutorial stages built: " + string.Join(", ", paths));
    }

    static string Make(int number, int swaps, string hint, System.Action<Transform> content)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrototypeSceneBuilder.BuildCamera();

        var map = new GameObject("Map").transform;
        Solid("Floor", map, new Vector2(0, -4), new Vector2(20, 1));
        Solid("WallL", map, new Vector2(-10, 1.5f), new Vector2(1, 11));
        Solid("WallR", map, new Vector2(10, 1.5f), new Vector2(1, 11));

        content(map);

        var swap = GameObject.Find("Player").GetComponent<SwapController>();
        PrefabBuilder.CreateGameSystem(swap, number, swaps, hint);

        string path = $"Assets/Scenes/Stage_{number:00}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    static void Stage1(Transform map)
    {
        Player(new Vector2(-8, -3));
        Solid("Step1", map, new Vector2(-4.5f, -3f), new Vector2(1, 1));
        Spike(new Vector2(0, -3.2f), new Vector2(2, 0.6f));
        Solid("Step2", map, new Vector2(4.5f, -2.5f), new Vector2(2, 2));
        Spike(new Vector2(7, -3.2f), new Vector2(1.5f, 0.6f));
        Goal(new Vector2(8.7f, -2.75f));
    }

    static void Stage2(Transform map)
    {
        Player(new Vector2(-6, -3));
        Spike(new Vector2(0, -3.2f), new Vector2(6, 0.6f));
        Solid("FloatingLedge", map, new Vector2(0, 0), new Vector2(2.5f, 0.5f));
        Pad(new Vector2(0, 0.75f));
        Pad(new Vector2(4.5f, -3f));
        Goal(new Vector2(8.5f, -2.75f));
    }

    static void Stage3(Transform map)
    {
        Player(new Vector2(-6, -3));
        Solid("Wall", map, new Vector2(-2, -2.5f), new Vector2(1, 2));
        Box(new Vector2(1, -3f));
        Solid("HighLedge", map, new Vector2(6.25f, 0.5f), new Vector2(6.5f, 0.5f));
        Box(new Vector2(3.5f, 1.25f));
        Goal(new Vector2(8.5f, 1.5f));
    }

    static void Solid(string name, Transform parent, Vector2 pos, Vector2 size) =>
        PrefabBuilder.Spawn("Block", parent, pos, size).name = name;

    static void Player(Vector2 pos) => PrefabBuilder.Spawn("Player", null, pos);
    static void Box(Vector2 pos) => PrefabBuilder.Spawn("Box", null, pos);
    static void Pad(Vector2 pos) => PrefabBuilder.Spawn("SwapPad", null, pos);
    static void Spike(Vector2 pos, Vector2 size) => PrefabBuilder.Spawn("Spike", null, pos, size);
    static void Goal(Vector2 pos) => PrefabBuilder.Spawn("Goal", null, pos);
}
