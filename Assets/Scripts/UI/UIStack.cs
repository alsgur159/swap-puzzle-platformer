using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIStack
{
    class Entry
    {
        public GameObject panel;
        public Action onBack;
        public GameObject prevSelected;
        public bool destroy;
    }

    static readonly List<Entry> stack = new List<Entry>();

    public static int Count
    {
        get { stack.RemoveAll(e => e.panel == null); return stack.Count; }
    }

    public static bool IsOpen(GameObject panel) => stack.Exists(e => e.panel == panel);

    public static void Open(GameObject panel, Action onBack, GameObject focus, bool destroyOnClose = true)
    {
        var es = EventSystem.current;
        stack.Add(new Entry
        {
            panel = panel,
            onBack = onBack,
            prevSelected = es != null ? es.currentSelectedGameObject : null,
            destroy = destroyOnClose,
        });
        panel.SetActive(true);
        UIKit.Focus(focus);
    }

    public static void Close(GameObject panel)
    {
        int i = stack.FindIndex(e => e.panel == panel);
        if (i < 0) return;
        var e = stack[i];
        stack.RemoveAt(i);
        if (e.destroy) UnityEngine.Object.Destroy(panel);
        else panel.SetActive(false);
        if (e.prevSelected != null && e.prevSelected.activeInHierarchy) UIKit.Focus(e.prevSelected);
    }

    public static bool Back()
    {
        if (Count == 0) return false;
        SoundFx.Click();
        stack[stack.Count - 1].onBack();
        return true;
    }
}
