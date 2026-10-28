using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class UIKit
{
    public static readonly Color Ink = Hex("080b16");
    public static readonly Color Primary = Hex("3d7bff");
    public static readonly Color Accent = Hex("5ef2ff");
    public static readonly Color Good = Hex("3fcf5e");
    public static readonly Color Gold = Hex("ffd25e");
    public static readonly Color Danger = Hex("e8505b");
    public static readonly Color Neutral = Hex("4a5578");
    public static readonly Color TextColor = Hex("f2f5ff");
    public static readonly Color SubText = Hex("a9b3d1");

    static Font regular, bold;
    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    public static Font Font => regular != null ? regular : regular = Resources.Load<Font>("Fonts/Galmuri11");
    public static Font Bold => bold != null ? bold : bold = Resources.Load<Font>("Fonts/Galmuri11-Bold");

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }

    public static Color WithAlpha(this Color c, float a) { c.a = a; return c; }

    public static Sprite Sprite(string name)
    {
        if (!sprites.TryGetValue(name, out var s) || s == null)
            sprites[name] = s = Resources.Load<Sprite>("UI/" + name);
        return s;
    }

    public static Canvas CreateCanvas(string name, int order)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    public static RectTransform Rect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static RectTransform Stretch(this RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        return rt;
    }

    public static T Layout<T>(this RectTransform rt, float spacing, int padding = 0, TextAnchor align = TextAnchor.MiddleCenter)
        where T : HorizontalOrVerticalLayoutGroup
    {
        var g = rt.gameObject.AddComponent<T>();
        g.spacing = spacing;
        g.padding = new RectOffset(padding, padding, padding, padding);
        g.childAlignment = align;
        g.childControlWidth = g.childControlHeight = true;
        g.childForceExpandWidth = g.childForceExpandHeight = false;
        return g;
    }

    public static RectTransform Size(this RectTransform rt, float w, float h)
    {
        var le = rt.GetComponent<LayoutElement>();
        if (le == null) le = rt.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = w;
        if (h >= 0) le.preferredHeight = h;
        rt.sizeDelta = new Vector2(w >= 0 ? w : rt.sizeDelta.x, h >= 0 ? h : rt.sizeDelta.y);
        return rt;
    }

    public static Image Image(Transform parent, string name, Sprite sprite, Color color, bool sliced = false)
    {
        var img = Rect(parent, name).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sliced) img.type = UnityEngine.UI.Image.Type.Sliced;
        img.raycastTarget = false;
        return img;
    }

    public static Image Panel(Transform parent, string name, float alpha = 1f) =>
        Image(parent, name, Sprite("panel"), Color.white.WithAlpha(alpha), true);

    public static Image Pill(Transform parent, string name) =>
        Image(parent, name, Sprite("pill"), Color.white, true);

    public static Image Icon(Transform parent, string name, float size, Color color)
    {
        var img = Image(parent, "Icon", Sprite("icon_" + name), color);
        img.rectTransform.sizeDelta = new Vector2(size, size);
        img.preserveAspect = true;
        return img;
    }

    public static Text Label(Transform parent, string text, int size, Color color,
        TextAnchor align = TextAnchor.MiddleCenter, bool bold = false, bool shadow = true)
    {
        var t = Rect(parent, "Label").gameObject.AddComponent<Text>();
        t.font = bold ? Bold : Font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 1.15f;
        t.raycastTarget = false;
        if (shadow)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = Ink.WithAlpha(0.85f);
            s.effectDistance = new Vector2(3, -3);
        }
        return t;
    }

    public static ColorBlock Tint(Color baseColor)
    {
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = baseColor;
        cb.highlightedColor = Color.Lerp(baseColor, Color.white, 0.25f);
        cb.selectedColor = Color.Lerp(baseColor, Color.white, 0.25f);
        cb.pressedColor = Color.Lerp(baseColor, Color.black, 0.25f);
        cb.disabledColor = Color.Lerp(baseColor, Hex("2a2f40"), 0.7f);
        cb.fadeDuration = 0.08f;
        return cb;
    }

    public static Button Button(Transform parent, string label, Color color, Action onClick,
        string icon = null, int fontSize = 40, float width = 520, float height = 96)
    {
        var rt = Rect(parent, "Button: " + label).Size(width, height);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Sprite("button");
        img.type = UnityEngine.UI.Image.Type.Sliced;
        img.color = Color.white;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.colors = Tint(color);
        if (onClick != null) btn.onClick.AddListener(() => onClick());
        rt.gameObject.AddComponent<UIButtonFx>();

        var row = Rect(rt, "Content").Stretch();
        row.Layout<HorizontalLayoutGroup>(18, 0).childControlWidth = false;
        if (icon != null)
        {
            var ic = Icon(row, icon, fontSize * 1.1f, Color.white);
            ic.gameObject.AddComponent<LayoutElement>().preferredWidth = fontSize * 1.1f;
        }
        if (!string.IsNullOrEmpty(label))
        {
            var t = Label(row, label, fontSize, TextColor, TextAnchor.MiddleCenter, true);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            var le = t.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = t.preferredWidth;
            t.rectTransform.sizeDelta = new Vector2(t.preferredWidth, height);
        }
        return btn;
    }

    public static Button IconButton(Transform parent, string icon, Color color, Action onClick, float size = 88)
    {
        var b = Button(parent, null, color, onClick, null, 40, size, size);
        var ic = Icon(b.transform, icon, size * 0.62f, Color.white);
        ic.rectTransform.anchoredPosition = Vector2.zero;
        UnityEngine.Object.Destroy(b.transform.Find("Content").gameObject);
        return b;
    }

    public static Slider Slider(Transform parent, float width, float min, float max)
    {
        var rt = Rect(parent, "Slider").Size(width, 56);
        var slider = rt.gameObject.AddComponent<Slider>();

        var track = Image(rt, "Track", Sprite("track"), Color.white, true);
        track.rectTransform.anchorMin = new Vector2(0, 0.3f);
        track.rectTransform.anchorMax = new Vector2(1, 0.7f);
        track.rectTransform.offsetMin = track.rectTransform.offsetMax = Vector2.zero;
        track.raycastTarget = true;

        var fillArea = Rect(rt, "Fill Area");
        fillArea.anchorMin = new Vector2(0, 0.3f);
        fillArea.anchorMax = new Vector2(1, 0.7f);
        fillArea.offsetMin = new Vector2(4, 4);
        fillArea.offsetMax = new Vector2(-4, -4);
        var fill = Image(fillArea, "Fill", Sprite("fill"), Accent, true);
        fill.rectTransform.sizeDelta = Vector2.zero;

        var handleArea = Rect(rt, "Handle Area").Stretch();
        handleArea.offsetMin = new Vector2(16, 0);
        handleArea.offsetMax = new Vector2(-16, 0);
        var handle = Image(handleArea, "Handle", Sprite("knob"), Color.white);
        handle.rectTransform.sizeDelta = new Vector2(40, 0);
        handle.raycastTarget = true;

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        var cb = Tint(Hex("c8d0e4"));
        cb.highlightedColor = cb.selectedColor = Accent;
        slider.colors = cb;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        rt.gameObject.AddComponent<UIButtonFx>().scaleOnSelect = false;
        return slider;
    }

    public static RectTransform Modal(Transform canvas, string name, Vector2 size, string title,
        out GameObject root, float dim = 0.6f)
    {
        var overlay = Image(canvas, name, null, Ink.WithAlpha(dim));
        overlay.rectTransform.Stretch();
        overlay.raycastTarget = true;
        root = overlay.gameObject;

        var panel = Panel(overlay.transform, "Panel");
        panel.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, size);
        panel.raycastTarget = true;
        root.AddComponent<UIPopIn>().target = panel.rectTransform;

        var content = Rect(panel.transform, "Content").Stretch(48);
        content.Layout<VerticalLayoutGroup>(22, 0, TextAnchor.UpperCenter);
        if (title != null)
        {
            var t = Label(content, title, 66, TextColor, TextAnchor.MiddleCenter, true);
            t.rectTransform.Size(-1, 84);
        }
        return content;
    }

    public static void Confirm(Transform canvas, string message, string detail, string okLabel, Color okColor, Action onOk)
    {
        var content = Modal(canvas, "Confirm", new Vector2(900, 470), null, out var root, 0.7f);
        Label(content, message, 50, TextColor, TextAnchor.MiddleCenter, true).rectTransform.Size(-1, 90);
        if (detail != null) Label(content, detail, 33, SubText).rectTransform.Size(-1, 60);
        Spacer(content, 20);

        var row = Rect(content, "Buttons").Size(-1, 100);
        row.Layout<HorizontalLayoutGroup>(28);
        Button cancel = null;
        cancel = Button(row, "취소", Neutral, () => UIStack.Close(root), null, 40, 330, 96);
        Button(row, okLabel, okColor, () => { UIStack.Close(root); onOk(); }, null, 40, 330, 96);
        UIStack.Open(root, () => UIStack.Close(root), cancel.gameObject);
    }

    public static RectTransform Spacer(Transform parent, float h)
    {
        return Rect(parent, "Spacer").Size(-1, h);
    }

    public static void Focus(GameObject go)
    {
        if (go == null || EventSystem.current == null) return;
        UIButtonFx.SuppressSelectSound();
        EventSystem.current.SetSelectedGameObject(go);
    }
}
