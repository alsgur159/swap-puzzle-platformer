using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    GameManager gm;
    StageInfo info;
    RectTransform canvas;

    Image[] pips;
    Text swapCount;
    int shownSwaps = -1;
    CanvasGroup exhausted;
    CanvasGroup hints;
    float hintTimer;

    GameObject pausePanel;
    Button resumeButton;
    SettingsPanel settings;
    GameObject clearPanel;
    CanvasGroup banner;

    int Stage => info != null ? info.stageNumber : 0;
    int SwapsMax => info != null ? info.swapCount : 0;
    int SwapsLeft => gm.swap != null && gm.swap.SwapsMax > 0 ? gm.swap.SwapsLeft : SwapsMax;

    void Start()
    {
        gm = GetComponent<GameManager>();
        info = gm.stage;
        Build();
        gm.ClearedEvent += () => StartCoroutine(ShowClearDelayed());

        if (SceneFlow.LastStartedStage != Stage) StartCoroutine(PlayBanner());
        SceneFlow.LastStartedStage = Stage;
    }

    void Build()
    {
        canvas = (RectTransform)UIKit.CreateCanvas("HUD", 10).transform;

        if (SwapsMax > 0) BuildSwapCounter();
        BuildTopRight();
        BuildHints();

        settings = SettingsPanel.Create(canvas);
        BuildPause();
    }

    void BuildSwapCounter()
    {
        var panel = UIKit.Panel(canvas, "Swaps", 0.92f);
        panel.rectTransform.Place(new Vector2(0, 1), new Vector2(24, -22), new Vector2(0, 92));
        var row = panel.rectTransform;
        row.Layout<HorizontalLayoutGroup>(16, 0, TextAnchor.MiddleLeft).padding = new RectOffset(26, 30, 0, 0);
        var fit = panel.gameObject.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        UIKit.Icon(row, "swap", 52, UIKit.Accent).rectTransform.Size(52, 52);
        UIKit.Label(row, "교환", 36, UIKit.TextColor, TextAnchor.MiddleLeft, true).rectTransform.Size(86, 60);

        pips = new Image[SwapsMax];
        for (int i = 0; i < SwapsMax; i++)
        {
            pips[i] = UIKit.Image(row, "Pip", UIKit.Sprite("fill"), UIKit.Accent, true);
            pips[i].rectTransform.Size(30, 30);
        }
        swapCount = UIKit.Label(row, "", 36, UIKit.TextColor, TextAnchor.MiddleLeft, true);
        swapCount.rectTransform.Size(100, 60);
    }

    void BuildTopRight()
    {
        var bar = UIKit.Rect(canvas, "TopRight").Place(new Vector2(1, 1), new Vector2(-24, -22), new Vector2(0, 120));
        bar.Layout<HorizontalLayoutGroup>(16, 0, TextAnchor.UpperRight).childAlignment = TextAnchor.UpperRight;
        bar.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var title = UIKit.Panel(bar, "StageTitle", 0.92f);
        title.rectTransform.Size(-1, 92);
        title.rectTransform.Layout<HorizontalLayoutGroup>(18, 0).padding = new RectOffset(30, 30, 0, 0);
        title.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        var stripe = UIKit.Image(title.transform, "Section", UIKit.Sprite("fill"), SceneFlow.SectionColor(Stage), true);
        stripe.rectTransform.Size(12, 44);
        Fit(UIKit.Label(title.transform, $"STAGE {Stage:00}", 38, UIKit.TextColor, TextAnchor.MiddleCenter, true), 60);
        Fit(UIKit.Label(title.transform, SceneFlow.StageName(Stage), 32, UIKit.SubText), 60);

        HudButton(bar, "restart", "다시 시작 (R)", gm.Restart);
        HudButton(bar, "gear", "설정", OpenSettingsFromHud);
    }

    static void Fit(Text t, float h)
    {
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.rectTransform.Size(t.preferredWidth, h);
    }

    void HudButton(Transform parent, string icon, string tooltip, System.Action onClick)
    {
        var b = UIKit.IconButton(parent, icon, UIKit.Neutral, onClick, 84);
        ((RectTransform)b.transform).Size(84, 84);
        var fx = b.GetComponent<UIButtonFx>();
        fx.deselectOnExit = true;
        var tip = UIKit.Pill(b.transform, "Tooltip");
        var t = UIKit.Label(tip.transform, tooltip, 28, UIKit.TextColor);
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        tip.rectTransform.Place(new Vector2(0.5f, 0f), new Vector2(0, -60), new Vector2(t.preferredWidth + 40, 50));
        tip.rectTransform.pivot = new Vector2(0.5f, 1f);
        t.rectTransform.Stretch();
        tip.gameObject.SetActive(false);
        fx.tooltip = tip.gameObject;
    }

    void BuildHints()
    {
        var stack = UIKit.Rect(canvas, "Hints").Place(new Vector2(0.5f, 1), new Vector2(0, -122), new Vector2(1400, 0));
        stack.Layout<VerticalLayoutGroup>(8, 0, TextAnchor.UpperCenter);
        stack.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var tips = UIKit.Rect(stack, "Tips");
        tips.Layout<VerticalLayoutGroup>(8, 0, TextAnchor.UpperCenter);
        tips.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        hints = tips.gameObject.AddComponent<CanvasGroup>();
        if (info != null && !string.IsNullOrEmpty(info.hint))
            Pill(tips, info.hint, UIKit.TextColor);
        if (Stage <= 3)
            Pill(tips, "A / D  이동    Space  점프    마우스 클릭  교환    R  다시 시작    Esc  메뉴", UIKit.Accent);

        var ex = Pill(stack, "교환 횟수를 모두 사용했어요.  막혔다면 R 키로 다시 시작하세요", UIKit.Gold);
        exhausted = ex.gameObject.AddComponent<CanvasGroup>();
        ex.gameObject.SetActive(false);
    }

    Image Pill(Transform parent, string text, Color color)
    {
        var pill = UIKit.Pill(parent, "Pill");
        var t = UIKit.Label(pill.transform, text, 30, color);
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        float w = Mathf.Min(t.preferredWidth + 64, 1400);
        pill.rectTransform.Size(w, 56);
        t.rectTransform.Stretch();
        return pill;
    }

    IEnumerator PlayBanner()
    {
        var root = UIKit.Rect(canvas, "Banner").Place(new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(1200, 300));
        banner = root.gameObject.AddComponent<CanvasGroup>();
        banner.blocksRaycasts = false;
        var sec = UIKit.Label(root, SceneFlow.Section(Stage), 34, SceneFlow.SectionColor(Stage), TextAnchor.MiddleCenter, true);
        sec.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(1200, 50));
        var big = UIKit.Label(root, $"STAGE {Stage:00}", 110, UIKit.TextColor, TextAnchor.MiddleCenter, true);
        big.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(1200, 130));
        big.gameObject.AddComponent<Outline>().effectColor = UIKit.Hex("1b2440");
        var name = UIKit.Label(root, SceneFlow.StageName(Stage), 50, UIKit.Accent);
        name.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(1200, 70));

        for (float t = 0; t < 2.2f; t += Time.unscaledDeltaTime)
        {
            banner.alpha = t < 0.25f ? t / 0.25f : t > 1.6f ? 1f - (t - 1.6f) / 0.6f : 1f;
            yield return null;
        }
        Destroy(root.gameObject);
    }

    void BuildPause()
    {
        var content = UIKit.Modal(canvas, "Pause", new Vector2(780, 920), "일시정지", out pausePanel);
        resumeButton = UIKit.Button(content, "계속하기", UIKit.Primary, Resume, "play", 40, 560, 96);
        UIKit.Button(content, "다시 시작", UIKit.Neutral, gm.Restart, "restart", 40, 560, 96);
        UIKit.Button(content, "설정", UIKit.Neutral, () => settings.Open(), "gear", 40, 560, 96);
        UIKit.Button(content, "스테이지 선택", UIKit.Neutral, () => SceneFlow.LoadTitle(true), "grid", 40, 560, 96);
        UIKit.Button(content, "게임 종료", UIKit.Neutral, AskQuit, "exit", 40, 560, 96);
        UIKit.Spacer(content, 4);
        UIKit.Label(content, "A / D 이동   Space 점프   클릭 교환\nR 다시 시작   Esc 돌아가기", 28, UIKit.SubText)
            .rectTransform.Size(-1, 80);
        pausePanel.SetActive(false);
    }

    void OpenPause()
    {
        gm.SetPaused(true);
        UIStack.Open(pausePanel, Resume, resumeButton.gameObject, false);
    }

    void Resume()
    {
        UIStack.Close(pausePanel);
        gm.SetPaused(false);
    }

    void OpenSettingsFromHud()
    {
        if (gm.Cleared || settings.IsOpen) return;
        gm.SetPaused(true);
        settings.Open(() => { if (!UIStack.IsOpen(pausePanel)) gm.SetPaused(false); });
    }

    void AskQuit() => UIKit.Confirm(canvas, "게임을 종료할까요?", "진행 상황은 자동으로 저장돼요.",
        "종료", UIKit.Danger, SceneFlow.Quit);

    IEnumerator ShowClearDelayed()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        ShowClear();
    }

    void ShowClear()
    {
        bool last = Stage >= SceneFlow.StageCount;
        var content = UIKit.Modal(canvas, "Clear", new Vector2(1100, last ? 820 : 760), null, out clearPanel, 0.45f);

        var title = UIKit.Label(content, last ? "ALL CLEAR!" : "STAGE CLEAR!", 92, UIKit.Gold, TextAnchor.MiddleCenter, true);
        title.rectTransform.Size(-1, 120);
        title.gameObject.AddComponent<Outline>().effectColor = UIKit.Hex("4a2511");
        UIKit.Label(content, $"STAGE {Stage:00}  ·  {SceneFlow.StageName(Stage)}", 40, UIKit.TextColor).rectTransform.Size(-1, 56);
        UIKit.Spacer(content, 6);

        if (SwapsMax > 0) Stat(content, "사용한 교환", $"{SwapsMax - SwapsLeft} / {SwapsMax}", UIKit.Accent);
        Stat(content, "클리어 시간", SceneFlow.FormatTime(gm.StageTime) + (gm.NewRecord ? "   NEW!" : ""),
            gm.NewRecord ? UIKit.Gold : UIKit.TextColor);
        Stat(content, "최고 기록", SceneFlow.FormatTime(SceneFlow.BestTime(Stage)), UIKit.SubText);

        if (last)
            UIKit.Label(content, "모든 스테이지를 클리어했어요!  플레이해 주셔서 고마워요.", 32, UIKit.Good).rectTransform.Size(-1, 60);

        UIKit.Spacer(content, 10);
        var row = UIKit.Rect(content, "Buttons").Size(-1, 100);
        row.Layout<HorizontalLayoutGroup>(22);
        UIKit.Button(row, "다시 하기", UIKit.Neutral, gm.Restart, "restart", 36, 290, 92);
        UIKit.Button(row, "스테이지 선택", UIKit.Neutral, () => SceneFlow.LoadTitle(true), "grid", 36, 340, 92);
        Button next = last
            ? UIKit.Button(row, "타이틀로", UIKit.Primary, () => SceneFlow.LoadTitle(false), "back", 36, 280, 92)
            : UIKit.Button(row, "다음 스테이지", UIKit.Primary, gm.LoadNext, "play", 36, 340, 92);
        UIKit.Label(content, "Enter  선택한 버튼 실행", 26, UIKit.SubText).rectTransform.Size(-1, 36);

        clearPanel.SetActive(true);
        UIKit.Focus(next.gameObject);
    }

    static void Stat(Transform parent, string label, string value, Color valueColor)
    {
        var row = UIKit.Rect(parent, "Stat: " + label).Size(700, 58);
        row.Layout<HorizontalLayoutGroup>(0, 0, TextAnchor.MiddleCenter);
        UIKit.Label(row, label, 36, UIKit.SubText, TextAnchor.MiddleLeft).rectTransform.Size(300, 58);
        UIKit.Label(row, value, 40, valueColor, TextAnchor.MiddleRight, true).rectTransform.Size(400, 58);
    }

    void Update()
    {
        UpdateSwaps();

        hintTimer += Time.unscaledDeltaTime;
        hints.alpha = Mathf.Lerp(1f, 0.45f, Mathf.InverseLerp(7f, 8f, hintTimer));

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame && !gm.Cleared && !UIStack.Back()) OpenPause();
    }

    void UpdateSwaps()
    {
        if (pips == null || swapCount == null) return;
        int left = SwapsLeft;
        if (left != shownSwaps)
        {
            for (int i = 0; i < pips.Length; i++)
            {
                bool on = i < left;
                pips[i].color = on ? UIKit.Accent : UIKit.Neutral.WithAlpha(0.6f);
                if (i == left && shownSwaps > left) pips[i].rectTransform.localScale = Vector3.one * 1.8f;
            }
            swapCount.text = $"{left} / {SwapsMax}";
            swapCount.color = left == 0 ? UIKit.Gold : UIKit.TextColor;
            shownSwaps = left;
        }
        foreach (var p in pips)
        {
            float s = Mathf.Lerp(p.rectTransform.localScale.x, 1f, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
            p.rectTransform.localScale = new Vector3(s, s, 1);
        }

        bool showEx = left == 0 && !gm.Cleared;
        if (exhausted.gameObject.activeSelf != showEx) exhausted.gameObject.SetActive(showEx);
        if (showEx) exhausted.alpha = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 4f);
    }
}
