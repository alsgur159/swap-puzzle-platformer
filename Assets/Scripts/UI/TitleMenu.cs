using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    public Sprite playerSprite, crateSprite;

    RectTransform canvasRect, stage;
    RectTransform player, crate, beam;
    Image beamImage;
    Vector2 lowSpot, highSpot;
    bool playerLow = true;
    float swapTimer;

    Button startButton;
    Text progressLabel;
    SettingsPanel settings;
    GameObject stageSelect;
    Text stageSelectInfo;

    static Vector2 Bg(float x, float y) => new Vector2(x * 4 - 960, 540 - y * 4);

    void Start()
    {
        SoundFx.Ensure();
        Time.timeScale = 1f;
        Build();

        if (SceneFlow.OpenStageSelectOnTitle)
        {
            SceneFlow.OpenStageSelectOnTitle = false;
            OpenStageSelect();
        }
        else UIKit.Focus(startButton.gameObject);
    }

    void Build()
    {
        var canvas = UIKit.CreateCanvas("TitleCanvas", 0);
        canvasRect = (RectTransform)canvas.transform;

        stage = UIKit.Rect(canvas.transform, "Background").Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1080));
        UIKit.Image(stage, "Sky", UIKit.Sprite("title_bg"), Color.white).rectTransform.Stretch();

        lowSpot = Bg(80, 190);
        highSpot = Bg(400, 120);
        beamImage = UIKit.Image(stage, "Beam", null, UIKit.Accent.WithAlpha(0f));
        beam = beamImage.rectTransform;
        beam.pivot = new Vector2(0, 0.5f);
        player = Actor("Player", playerSprite, lowSpot);
        crate = Actor("Crate", crateSprite, highSpot);

        var vignette = UIKit.Image(canvas.transform, "Shade", null, UIKit.Ink.WithAlpha(0.25f));
        vignette.rectTransform.Stretch();

        var logo = UIKit.Label(canvas.transform, "자리 바꾸기", 150, UIKit.TextColor, TextAnchor.MiddleCenter, true);
        logo.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(1400, 200));
        var outline = logo.gameObject.AddComponent<Outline>();
        outline.effectColor = UIKit.Hex("1b2440");
        outline.effectDistance = new Vector2(6, -6);
        logo.GetComponent<Shadow>().effectDistance = new Vector2(10, -12);
        var tagline = UIKit.Label(canvas.transform, "점프로 못 가는 곳이라면, 자리를 바꾸면 된다.", 40, UIKit.Accent);
        tagline.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, -350), new Vector2(1400, 60));

        var menu = UIKit.Rect(canvas.transform, "Menu").Place(new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(560, 400));
        menu.Layout<VerticalLayoutGroup>(24);
        startButton = UIKit.Button(menu, "게임 시작", UIKit.Primary, OpenStageSelect, "play", 44, 540, 104);
        UIKit.Button(menu, "설정", UIKit.Neutral, OpenSettings, "gear", 44, 540, 104);
        UIKit.Button(menu, "게임 종료", UIKit.Neutral, AskQuit, "exit", 44, 540, 104);

        progressLabel = UIKit.Label(canvas.transform, "", 32, UIKit.SubText);
        progressLabel.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -360), new Vector2(900, 50));
        RefreshProgress();

        var help = UIKit.Pill(canvas.transform, "Help");
        help.rectTransform.Place(new Vector2(0, 0), new Vector2(28, 24), new Vector2(880, 64));
        UIKit.Label(help.transform, "마우스 또는 방향키로 선택  ·  Enter 확인  ·  Esc 뒤로", 28, UIKit.TextColor)
            .rectTransform.Stretch();
        var credit = UIKit.Label(canvas.transform, "v1.0   ·   글꼴: 갈무리 (SIL OFL)", 26, UIKit.SubText, TextAnchor.LowerRight);
        credit.rectTransform.Place(new Vector2(1, 0), new Vector2(-32, 34), new Vector2(800, 40));

        settings = SettingsPanel.Create(canvas.transform);
    }

    RectTransform Actor(string name, Sprite sprite, Vector2 feet)
    {
        var img = UIKit.Image(stage, name, sprite, Color.white);
        img.rectTransform.pivot = new Vector2(0.5f, 0f);
        img.rectTransform.anchoredPosition = feet;
        img.rectTransform.sizeDelta = new Vector2(64, 64);
        return img.rectTransform;
    }

    void Update()
    {
        var size = canvasRect.rect.size;
        float k = Mathf.Max(size.x / 1920f, size.y / 1080f);
        stage.localScale = new Vector3(k, k, 1);

        AnimateSwap();

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame && !UIStack.Back()) AskQuit();
    }

    void AnimateSwap()
    {
        swapTimer += Time.unscaledDeltaTime;
        const float period = 3f, aim = 0.6f;
        Vector2 a = player.anchoredPosition + new Vector2(0, 32), b = crate.anchoredPosition + new Vector2(0, 32);

        float left = period - swapTimer;
        if (left < aim)
        {
            beam.anchoredPosition = a;
            Vector2 d = b - a;
            beam.sizeDelta = new Vector2(d.magnitude, 6);
            beam.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            beamImage.color = UIKit.Accent.WithAlpha(0.35f + 0.35f * Mathf.Sin(swapTimer * 40f));
        }
        else beamImage.color = UIKit.Accent.WithAlpha(0f);

        if (swapTimer >= period)
        {
            swapTimer = 0f;
            playerLow = !playerLow;
            player.anchoredPosition = playerLow ? lowSpot : highSpot;
            crate.anchoredPosition = playerLow ? highSpot : lowSpot;
            player.localScale = crate.localScale = new Vector3(1.3f, 1.3f, 1);
        }
        float s = Mathf.Lerp(player.localScale.x, 1f, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
        player.localScale = crate.localScale = new Vector3(s, s, 1);
    }

    void RefreshProgress()
    {
        int c = SceneFlow.ClearedCount;
        progressLabel.text = c == 0 ? "처음이라면 게임 시작을 눌러 튜토리얼부터 시작하세요"
            : $"진행도  {c} / {SceneFlow.StageCount} 스테이지 클리어";
    }

    void OpenSettings() => settings.Open();

    void AskQuit() => UIKit.Confirm(canvasRect, "게임을 종료할까요?", "진행 상황은 자동으로 저장돼요.",
        "종료", UIKit.Danger, SceneFlow.Quit);

    void OpenStageSelect()
    {
        if (stageSelect != null) Destroy(stageSelect);
        var content = UIKit.Modal(canvasRect, "StageSelect", new Vector2(1640, 960), "스테이지 선택", out stageSelect, 0.55f);

        var legend = UIKit.Rect(content, "Legend").Size(-1, 44);
        legend.Layout<HorizontalLayoutGroup>(14);
        foreach (int n in new[] { 1, 4, 10 })
        {
            var dot = UIKit.Image(legend, "Dot", UIKit.Sprite("fill"), SceneFlow.SectionColor(n), true);
            dot.rectTransform.Size(28, 28);
            var name = n == 1 ? "튜토리얼 1~3" : n == 4 ? "응용 4~9" : "최종 10";
            UIKit.Label(legend, name, 30, UIKit.SubText, TextAnchor.MiddleLeft).rectTransform.Size(210, 44);
        }
        var progress = UIKit.Label(legend, $"클리어  {SceneFlow.ClearedCount} / {SceneFlow.StageCount}", 30, UIKit.Gold);
        progress.rectTransform.Size(260, 44);

        var grid = UIKit.Rect(content, "Grid").Size(5 * 270 + 4 * 24, 2 * 260 + 24);
        var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(270, 260);
        gl.spacing = new Vector2(24, 24);
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 5;

        Button focus = null;
        for (int n = 1; n <= SceneFlow.StageCount; n++)
        {
            var cell = StageCell(grid, n);
            if (n == SceneFlow.NextStageToPlay) focus = cell;
        }

        stageSelectInfo = UIKit.Label(content, "", 32, UIKit.TextColor);
        stageSelectInfo.rectTransform.Size(-1, 50);

        var bottom = UIKit.Rect(content, "Bottom").Size(-1, 96);
        bottom.Layout<HorizontalLayoutGroup>(24);
        UIKit.Button(bottom, "뒤로", UIKit.Neutral, CloseStageSelect, "back", 40, 300, 92);

        UIStack.Open(stageSelect, CloseStageSelect, focus.gameObject);
    }

    Button StageCell(Transform grid, int n)
    {
        bool unlocked = SceneFlow.IsUnlocked(n), cleared = SceneFlow.IsCleared(n);
        var color = cleared ? UIKit.Hex("2f5a4a") : unlocked ? UIKit.Hex("34406a") : UIKit.Hex("232838");
        var b = UIKit.Button(grid, null, color, () => PickStage(n), null, 40, 270, 260);
        var root = b.transform;
        Destroy(root.Find("Content").gameObject);

        var strip = UIKit.Image(root, "Section", UIKit.Sprite("fill"), SceneFlow.SectionColor(n).WithAlpha(unlocked ? 1f : 0.35f), true);
        strip.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(230, 12));

        var num = UIKit.Label(root, n.ToString("00"), 84, unlocked ? UIKit.TextColor : UIKit.SubText.WithAlpha(0.5f), TextAnchor.MiddleCenter, true);
        num.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, -36), new Vector2(250, 96));

        var name = UIKit.Label(root, SceneFlow.StageName(n), 30, unlocked ? UIKit.TextColor : UIKit.SubText.WithAlpha(0.5f));
        name.rectTransform.Place(new Vector2(0.5f, 1f), new Vector2(0, -136), new Vector2(250, 44));

        if (!unlocked)
        {
            var l = UIKit.Icon(root, "lock", 52, UIKit.SubText);
            l.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(52, 52));
        }
        else
        {
            string status = cleared ? "클리어  " + SceneFlow.FormatTime(SceneFlow.BestTime(n)) : "도전!";
            var st = UIKit.Label(root, status, 28, cleared ? UIKit.Good : UIKit.Accent);
            st.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0, 26), new Vector2(250, 50));
            if (cleared)
            {
                var ck = UIKit.Icon(root, "check", 44, UIKit.Good);
                ck.rectTransform.Place(new Vector2(1f, 1f), new Vector2(-14, -26), new Vector2(44, 44));
            }
        }

        var fx = b.gameObject.AddComponent<StageCellInfo>();
        fx.onSelected = () => ShowStageInfo(n);
        return b;
    }

    void ShowStageInfo(int n)
    {
        if (stageSelectInfo == null) return;
        if (!SceneFlow.IsUnlocked(n))
        {
            stageSelectInfo.text = $"{n - 1}스테이지를 클리어하면 열려요";
            stageSelectInfo.color = UIKit.SubText;
            return;
        }
        string best = SceneFlow.IsCleared(n)
            ? $"   ·   최고 기록 {SceneFlow.FormatTime(SceneFlow.BestTime(n))}, 최소 교환 {SceneFlow.BestSwaps(n)}회"
            : "";
        stageSelectInfo.text = $"[{SceneFlow.Section(n)}]  {n}. {SceneFlow.StageName(n)}{best}";
        stageSelectInfo.color = UIKit.TextColor;
    }

    void PickStage(int n)
    {
        if (!SceneFlow.IsUnlocked(n))
        {
            SoundFx.SwapFail();
            ShowStageInfo(n);
            return;
        }
        SceneFlow.LoadStage(n);
    }

    void CloseStageSelect()
    {
        UIStack.Close(stageSelect);
        stageSelect = null;
        RefreshProgress();
        UIKit.Focus(startButton.gameObject);
    }
}
