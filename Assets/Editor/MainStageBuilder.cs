using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 스테이지 4~10. 좌표 기준: 바닥 윗면 y=-3.5, 점프 최대 높이 약 2.45, 교환 사거리 8.
// 각 스테이지 위 주석은 의도한 정답 루트.
public static class MainStageBuilder
{
    [MenuItem("Tools/Swap/Build All (Art + Stages 1-10)")]
    public static void BuildEverything()
    {
        ArtApplier.Apply();
        TutorialStageBuilder.BuildAll();
        BuildAll();
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

    // Assets/Scenes/Stage_XX.unity 를 번호 순으로 빌드 세팅에 등록한다.
    internal static void RegisterStages()
    {
        var scenes = Directory.GetFiles("Assets/Scenes", "Stage_*.unity")
            .Select(p => p.Replace('\\', '/'))
            .OrderBy(p => p)
            .Select(p => new EditorBuildSettingsScene(p, true))
            .ToArray();
        EditorBuildSettings.scenes = scenes;
    }

    static void Make(int number, int swaps, string hint, System.Action<Transform> content, bool wide = false)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        PrototypeSceneBuilder.BuildCamera();

        var map = new GameObject("Map").transform;
        if (wide)
        {
            // 화면 1.3배 크기: x -12~12, y -3.5~10
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

    // 발판으로 선반에 올라가(1) 상자를 오른쪽 끝으로 밀어 떨어뜨린다.
    // 상자는 공중 가시를 통과해 아래 방에 떨어지고, 위에서 내려다보며 상자와 교환(2).
    // 함정: 바닥에서 점프해 상자와 먼저 바꾸면 상자를 위로 올릴 방법이 없다.
    // 상자가 천천히 떨어지면 선반 끝에 서서는 칸막이에 가려 안 보이므로 점프하며 교환해야 한다.
    // Stopper: 세게 밀어도 상자가 사거리 밖으로 미끄러지지 않게 막는 벽.
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

    // 가시 깔린 단 위를 점프로 지나가며 공중에서 상자와 교환(1) -> 상자가 내 속도로 날아가 단 위에 착지.
    // 너무 빠르면 상자가 미끄러져 떨어지니 공중에서 감속 후 교환. 상자를 밟고 출구 선반으로.
    // 예비 1회. 단 위 상자와 직접 교환하면 가시 위로 순간이동해 사망.
    static void Stage5(Transform map)
    {
        Player(new Vector2(-8, -3));
        Box(new Vector2(-5, -3));
        Solid("SpikeBed", map, new Vector2(1.75f, -3f), new Vector2(3.5f, 1));
        Spike(new Vector2(1.75f, -2.2f), new Vector2(3.3f, 0.6f));
        Solid("GoalLedge", map, new Vector2(7, 0.25f), new Vector2(5, 0.5f));
        Goal(new Vector2(8.7f, 1.25f));
    }

    // 바닥 순찰 적을 피하며, 1층 선반 적이 왼쪽 끝에 왔을 때 교환(1) -> 2층 적이 왼쪽에 올 때 교환(2) -> 점프로 출구.
    // 선반 모서리에 시야가 가려지므로 적의 위치·점프 타이밍이 핵심. 예비 1회.
    static void Stage6(Transform map)
    {
        Player(new Vector2(-9, -3));
        Spike(new Vector2(3.75f, -3.2f), new Vector2(11.5f, 0.6f));
        Enemy(new Vector2(-5.5f, -3f), 1.5f, 1.5f);

        Solid("Ledge1", map, new Vector2(-2, -0.25f), new Vector2(4, 0.5f));
        Enemy(new Vector2(-2, 0.5f), 1.5f, 2f);

        Solid("Ledge2", map, new Vector2(4, 2.75f), new Vector2(4, 0.5f));
        Enemy(new Vector2(4, 3.5f), 1.5f, 2f);

        Solid("GoalLedge", map, new Vector2(8.5f, 4.75f), new Vector2(2, 0.5f));
        Goal(new Vector2(8.8f, 5.75f));
    }

    // 땅에서는 사거리 밖. 가시밭 위로 점프해 공중에서 요새 위 적과 교환(1) -> 적은 가시에 떨어져 소멸.
    // 낮은 통로를 막은 두 번째 적과 교환(2) 후, 뒤쫓아오는 적보다 먼저 출구로.
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

    // 1) 상자를 가시밭 왼쪽 끝까지 밀어둔다 (안 하면 나중에 사거리 밖).
    // 2) 가시 위 공중에서 '먼 쪽' 적과 교환(1) -> 그 적은 가시로. 착지하면 바로 선반 밑으로 피한다.
    // 3) 남은 적이 왼쪽 끝에 왔을 때 교환(2) -> 쫓아오기 전에 바로 상자와 교환(3) -> 상자가 적을 막는다.
    // 4) 왼쪽에서 다시 가시 위로 뛰어 공중에서 남은 적과 교환(4) -> 상자 밟고 출구. 예비 없음.
    // 가까운 적을 먼저 없애면 남은 적이 사거리 밖이라 오른쪽으로 건너갈 수 없다.
    static void Stage8(Transform map)
    {
        Player(new Vector2(-8.5f, -3));
        Box(new Vector2(-6.5f, -3));
        Solid("Curb", map, new Vector2(-3.25f, -3.25f), new Vector2(0.5f, 0.5f)); // 상자가 가시 위로 밀려나지 않게
        Spike(new Vector2(0, -3.2f), new Vector2(6, 0.6f));
        Enemy(new Vector2(7, -3), 1.5f, 2f);
        Enemy(new Vector2(5, -3), 1.5f, 2f);
        Solid("GoalLedge", map, new Vector2(8.25f, -1.05f), new Vector2(2.5f, 0.5f));
        Goal(new Vector2(8.8f, -0.05f));
    }

    // 왼쪽 발판(1)으로 선반 A에 오른다 -> 선반 가운데서 점프해 받침대 위 상자와 교환(2)
    // -> 받침대에서 오른쪽 발판과 교환(3) -> 상자를 선반 오른쪽 끝으로 밀고 밟아 출구 선반으로.
    // 함정: 발판 위나 바로 옆에 서서 상자를 부르면 돌아갈 발판 자리가 막혀 교환이 거부된다.
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

    // (1) 가시 구덩이 끝에서 너머의 적과 교환해 가운데로 -> (2) 선반 M 왼쪽 발판
    // -> (3) 발판 오른쪽에서 점프해 상자 호출 (땅에 서 있으면 M에 가려 안 보임) -> 상자가 M 위에 떨어짐
    // -> (4) M 밑으로 걸어가 다시 발판 -> 상자 밟고 왼쪽 위 선반 U로
    // -> (5) 꼭대기 적이 오른쪽 끝에 왔을 때 교환 -> 출구. 예비 없음.
    // 함정: 발판 바로 옆에서 상자를 부르면 돌아갈 발판 자리가 막힌다.
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
