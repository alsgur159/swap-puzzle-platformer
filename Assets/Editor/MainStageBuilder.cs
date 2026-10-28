using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MainStageBuilder
{
    [MenuItem("Tools/Swap/Build All (Art + Title + Stages 1-10)")]
    public static void BuildEverything()
    {
        ArtApplier.Apply();
        BuildTitle();
        TutorialStageBuilder.BuildAll();
        BuildAll();
    }

    [MenuItem("Tools/Swap/Build Title Scene")]
    public static void BuildTitle()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrototypeSceneBuilder.BuildCamera();
        Camera.main.backgroundColor = new Color(0.04f, 0.06f, 0.14f);

        var menu = new GameObject("TitleMenu").AddComponent<TitleMenu>();
        menu.playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/player.png");
        menu.crateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/crate.png");

        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{SceneFlow.TitleScene}.unity");
        PlayerSettings.productName = "자리 바꾸기";
        RegisterStages();
    }

    [MenuItem("Tools/Swap/Build Main Stages (4-10)")]
    public static void BuildAll()
    {
        PrefabBuilder.EnsurePrefabs();
        Directory.CreateDirectory("Assets/Scenes");

        Make(4, 2, "상자를 밀어 떨어뜨린 뒤 교환하세요. 가시는 상자를 막지 못해요", Stage4);
        Make(5, 2, "교환하면 속도도 서로 바뀌어요. 점프 중에 바꾸면 상자가 그 속도로 날아가요", Stage5);
        Make(6, 3, "적과 바꾸면 적은 내가 있던 자리로 가요. 움직이는 적의 타이밍을 노리세요", Stage6);
        Make(7, 2, "공중에서 교환하면 상대가 그 공중에 남아요. 가시 위라면...?", Stage7);
        Make(8, 4, "교환 사거리는 8칸. 무엇을 먼저 바꾸고 무엇을 남겨둘지 생각하세요", Stage8);
        Make(9, 3, "교환 순서와 서 있는 자리가 중요해요. 상자도 시야를 가립니다", Stage9);
        Make(10, 5, "지금까지 배운 전부. 교환 5번, 한 번도 낭비할 수 없어요", Stage10, wide: true);

        RegisterStages();
        AssetDatabase.SaveAssets();
        Debug.Log("Main stages 4-10 built");
    }

    internal static void RegisterStages()
    {
        var title = $"Assets/Scenes/{SceneFlow.TitleScene}.unity";
        var stages = Directory.GetFiles("Assets/Scenes", "Stage_*.unity")
            .Select(p => p.Replace('\\', '/'))
            .OrderBy(p => p);
        var all = (File.Exists(title) ? new[] { title } : new string[0]).Concat(stages);
        EditorBuildSettings.scenes = all.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
    }

    static void Make(int number, int swaps, string hint, System.Action<Transform> content, bool wide = false)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrototypeSceneBuilder.BuildCamera();

        var map = new GameObject("Map").transform;
        if (wide)
        {
            var cam = Camera.main;
            cam.orthographicSize = 7f;
            cam.transform.position = new Vector3(0f, 3f, -10f);
            Solid("Floor", map, new Vector2(0, -4), new Vector2(26, 1));
            Solid("WallL", map, new Vector2(-12.5f, 3f), new Vector2(1, 15));
            Solid("WallR", map, new Vector2(12.5f, 3f), new Vector2(1, 15));
        }
        else
        {
            Solid("Floor", map, new Vector2(0, -4), new Vector2(20, 1));
            Solid("WallL", map, new Vector2(-10, 1.5f), new Vector2(1, 11));
            Solid("WallR", map, new Vector2(10, 1.5f), new Vector2(1, 11));
        }

        content(map);

        var swap = GameObject.Find("Player").GetComponent<SwapController>();
        PrefabBuilder.CreateGameSystem(swap, number, swaps, hint);

        EditorSceneManager.SaveScene(scene, $"Assets/Scenes/Stage_{number:00}.unity");
    }

    static void Stage4(Transform map)
    {
        Player(new Vector2(-8, -3));
        Solid("Shelf", map, new Vector2(-1.75f, 1.25f), new Vector2(6.5f, 0.5f));
        Solid("Divider", map, new Vector2(1.25f, -1f), new Vector2(0.5f, 5));
        Solid("Stopper", map, new Vector2(6.75f, -1.75f), new Vector2(0.5f, 3.5f));
        Pad(new Vector2(-4.5f, 2f));
        Box(new Vector2(-3f, 2f));
        Spike(new Vector2(5.5f, 0.3f), new Vector2(8, 0.6f));
        Goal(new Vector2(5.9f, -2.75f));
    }

    static void Stage5(Transform map)
    {
        Player(new Vector2(-8, -3));
        Box(new Vector2(-5, -3));
        Solid("SpikeBed", map, new Vector2(1.75f, -3f), new Vector2(3.5f, 1));
        Spike(new Vector2(1.75f, -2.2f), new Vector2(3.3f, 0.6f));
        Solid("GoalLedge", map, new Vector2(7, 0.25f), new Vector2(5, 0.5f));
        Goal(new Vector2(8.7f, 1.25f));
    }

    static void Stage6(Transform map)
    {
        Player(new Vector2(-9, -3));
        Spike(new Vector2(3.75f, -3.2f), new Vector2(11.5f, 0.6f));
        Enemy(new Vector2(-5.5f, -3f), 1.5f, 1.5f);

        Solid("Ledge1", map, new Vector2(-2, -0.25f), new Vector2(4, 0.5f));
        Enemy(new Vector2(-2, 0.5f), 1.5f, 2f);

        Solid("Ledge2", map, new Vector2(4, 2.75f), new Vector2(4, 0.5f));
        Enemy(new Vector2(4, 3.5f), 1.5f, 2f);

        Solid("GoalLedge", map, new Vector2(8.5f, 4.25f), new Vector2(2, 0.5f));
        Goal(new Vector2(8.8f, 5.25f));
    }

    static void Stage7(Transform map)
    {
        Player(new Vector2(-8, -3));
        Spike(new Vector2(0, -3.2f), new Vector2(8, 0.6f));
        Solid("Fort", map, new Vector2(6.75f, -2f), new Vector2(5.5f, 3));
        Solid("FortRoof", map, new Vector2(7.25f, 1.05f), new Vector2(2.5f, 0.5f));
        Enemy(new Vector2(5, 0), 0.5f, 1.5f);
        Enemy(new Vector2(7.75f, 0), 1.25f, 2f);
        Goal(new Vector2(9f, 0.25f));
    }

    static void Stage8(Transform map)
    {
        Player(new Vector2(-8.5f, -3));
        Box(new Vector2(-6.5f, -3));
        Solid("Curb", map, new Vector2(-3.25f, -3.25f), new Vector2(0.5f, 0.5f));
        Spike(new Vector2(0, -3.2f), new Vector2(6, 0.6f));
        Enemy(new Vector2(7, -3), 1.5f, 2f);
        Enemy(new Vector2(5, -3), 1.5f, 2f);
        Solid("GoalLedge", map, new Vector2(8.25f, -1.05f), new Vector2(2.5f, 0.5f));
        Goal(new Vector2(8.8f, -0.05f));
    }

    static void Stage9(Transform map)
    {
        Player(new Vector2(-8.5f, -3));
        Spike(new Vector2(1.75f, -3.2f), new Vector2(15.5f, 0.6f));
        Solid("LedgeA", map, new Vector2(-1.5f, 0.25f), new Vector2(5, 0.5f));
        Pad(new Vector2(-3.6f, 1f));
        Pad(new Vector2(0.4f, 1f));
        Solid("Pedestal", map, new Vector2(5, -3f), new Vector2(2, 1));
        Box(new Vector2(5, -2f));
        Solid("GoalLedge", map, new Vector2(3.75f, 3.05f), new Vector2(3.5f, 0.5f));
        Goal(new Vector2(4.9f, 4.05f));
    }

    static void Stage10(Transform map)
    {
        Player(new Vector2(-11, -3));
        Spike(new Vector2(-4, -3.2f), new Vector2(6, 0.6f));
        Enemy(new Vector2(1, -3), 1.5f, 2f);
        Box(new Vector2(9f, -3));

        Solid("LedgeM", map, new Vector2(3.5f, 0.25f), new Vector2(3, 0.5f));
        Pad(new Vector2(2.5f, 1f));

        Solid("LedgeU", map, new Vector2(-4, 2.95f), new Vector2(10, 0.5f));
        Spike(new Vector2(-7, 3.5f), new Vector2(4, 0.6f));

        Solid("LedgeT", map, new Vector2(-9.25f, 6.75f), new Vector2(5.5f, 0.5f));
        Enemy(new Vector2(-9, 7.5f), 2f, 2f);
        Goal(new Vector2(-11.3f, 7.75f));
    }

    static void Solid(string name, Transform parent, Vector2 pos, Vector2 size) =>
        PrefabBuilder.Spawn("Block", parent, pos, size).name = name;

    static void Player(Vector2 pos) => PrefabBuilder.Spawn("Player", null, pos);
    static void Box(Vector2 pos) => PrefabBuilder.Spawn("Box", null, pos);
    static void Pad(Vector2 pos) => PrefabBuilder.Spawn("SwapPad", null, pos);
    static void Spike(Vector2 pos, Vector2 size) => PrefabBuilder.Spawn("Spike", null, pos, size);
    static void Goal(Vector2 pos) => PrefabBuilder.Spawn("Goal", null, pos);

    static void Enemy(Vector2 pos, float range, float speed)
    {
        var patrol = PrefabBuilder.Spawn("Enemy", null, pos).GetComponent<EnemyPatrol>();
        patrol.patrolRange = range;
        patrol.speed = speed;
    }
}
