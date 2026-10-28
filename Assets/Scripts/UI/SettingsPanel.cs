using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    Slider master, bgm, sfx;
    Text masterValue, bgmValue, sfxValue, resolutionValue, fullscreenValue;
    Image fullscreenCheck;
    Button closeButton;
    int resIndex;
    bool fullscreen;
    bool refreshing;
    System.Action onClosed;

    public bool IsOpen => gameObject.activeSelf;

    public static SettingsPanel Create(Transform canvas)
    {
        var content = UIKit.Modal(canvas, "Settings", new Vector2(1180, 900), "설정", out var root);
        var panel = root.AddComponent<SettingsPanel>();
        panel.Build(content, root.transform.Find("Panel"));
        root.SetActive(false);
        return panel;
    }

    void Build(RectTransform content, Transform panelRect)
    {
        var close = UIKit.IconButton(panelRect, "close", UIKit.Neutral, Close, 76);
        ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(-30, -30), new Vector2(76, 76));

        Section(content, "사운드");
        master = VolumeRow(content, "전체 음량", out masterValue);
        bgm = VolumeRow(content, "배경 음악", out bgmValue);
        sfx = VolumeRow(content, "효과음", out sfxValue);

        UIKit.Spacer(content, 6);
        Section(content, "화면");
        var resRow = Row(content, "해상도");
        UIKit.Button(resRow, "<", UIKit.Neutral, () => ChangeResolution(-1), null, 40, 80, 72);
        resolutionValue = UIKit.Label(resRow, "", 40, UIKit.TextColor);
        resolutionValue.rectTransform.Size(330, 72);
        UIKit.Button(resRow, ">", UIKit.Neutral, () => ChangeResolution(1), null, 40, 80, 72);

        var fsRow = Row(content, "전체 화면");
        var toggle = UIKit.Button(fsRow, null, UIKit.Neutral, ToggleFullscreen, null, 40, 80, 72);
        fullscreenCheck = UIKit.Icon(toggle.transform, "check", 52, UIKit.Accent);
        fullscreenValue = UIKit.Label(fsRow, "", 36, UIKit.SubText, TextAnchor.MiddleLeft);
        fullscreenValue.rectTransform.Size(410, 72);

        if (Application.isEditor)
        {
            var note = UIKit.Label(content, "해상도·전체 화면은 빌드한 게임에서 적용됩니다 (에디터에서는 저장만 됨)", 26, UIKit.SubText);
            note.rectTransform.Size(-1, 36);
        }

        UIKit.Spacer(content, 4);
        var buttons = UIKit.Rect(content, "Buttons").Size(-1, 100);
        buttons.Layout<HorizontalLayoutGroup>(28);
        UIKit.Button(buttons, "기본값으로", UIKit.Neutral, ResetDefaults, "restart", 38, 360, 92);
        closeButton = UIKit.Button(buttons, "닫기", UIKit.Primary, Close, null, 40, 360, 92);

        var hint = UIKit.Label(content, "Esc  닫기      ←  →  값 조절", 28, UIKit.SubText);
        hint.rectTransform.Size(-1, 40);

        master.onValueChanged.AddListener(_ => OnVolume());
        bgm.onValueChanged.AddListener(_ => OnVolume());
        sfx.onValueChanged.AddListener(_ => OnVolume());
    }

    static void Section(Transform parent, string title)
    {
        var t = UIKit.Label(parent, title, 34, UIKit.Accent, TextAnchor.MiddleLeft, true);
        t.rectTransform.Size(1000, 46);
    }

    static RectTransform Row(Transform parent, string label)
    {
        var row = UIKit.Rect(parent, "Row: " + label).Size(1000, 76);
        row.Layout<HorizontalLayoutGroup>(20, 0, TextAnchor.MiddleLeft);
        var l = UIKit.Label(row, label, 38, UIKit.TextColor, TextAnchor.MiddleLeft);
        l.rectTransform.Size(240, 76);
        return row;
    }

    static Slider VolumeRow(Transform parent, string label, out Text value)
    {
        var row = Row(parent, label);
        var s = UIKit.Slider(row, 560, 0, 20);
        value = UIKit.Label(row, "", 38, UIKit.TextColor, TextAnchor.MiddleRight);
        value.rectTransform.Size(130, 76);
        return s;
    }

    public void Open(System.Action closed = null)
    {
        onClosed = closed;
        Refresh();
        UIStack.Open(gameObject, Close, master.gameObject, false);
    }

    public void Close()
    {
        if (!UIStack.IsOpen(gameObject)) return;
        GameSettings.Save();
        UIStack.Close(gameObject);
        onClosed?.Invoke();
    }

    void Refresh()
    {
        refreshing = true;
        master.value = Mathf.Round(GameSettings.Master * 20);
        bgm.value = Mathf.Round(GameSettings.Bgm * 20);
        sfx.value = Mathf.Round(GameSettings.Sfx * 20);
        resIndex = GameSettings.ResolutionIndex;
        fullscreen = GameSettings.Fullscreen;
        refreshing = false;
        UpdateLabels();
    }

    void UpdateLabels()
    {
        masterValue.text = (int)(master.value * 5) + "%";
        bgmValue.text = (int)(bgm.value * 5) + "%";
        sfxValue.text = (int)(sfx.value * 5) + "%";
        var r = GameSettings.Resolutions[resIndex];
        resolutionValue.text = r.x + " x " + r.y;
        fullscreenCheck.enabled = fullscreen;
        fullscreenValue.text = fullscreen ? "켜짐" : "꺼짐 (창 모드)";
    }

    void OnVolume()
    {
        if (refreshing) return;
        GameSettings.SetVolumes(master.value / 20f, bgm.value / 20f, sfx.value / 20f);
        UpdateLabels();
    }

    void ChangeResolution(int delta)
    {
        int n = GameSettings.Resolutions.Count;
        resIndex = (resIndex + delta + n) % n;
        GameSettings.SetDisplay(resIndex, fullscreen);
        UpdateLabels();
    }

    void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        GameSettings.SetDisplay(resIndex, fullscreen);
        UpdateLabels();
    }

    void ResetDefaults()
    {
        GameSettings.ResetDefaults();
        Refresh();
    }
}
