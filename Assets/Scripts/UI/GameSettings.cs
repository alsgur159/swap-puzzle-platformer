using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public const float DefaultMaster = 0.8f, DefaultBgm = 0.6f, DefaultSfx = 0.8f;

    public static float Master { get; private set; }
    public static float Bgm { get; private set; }
    public static float Sfx { get; private set; }
    public static bool Fullscreen { get; private set; }
    public static int ResolutionIndex { get; private set; }

    static readonly Vector2Int[] Candidates =
    {
        new Vector2Int(1280, 720), new Vector2Int(1366, 768), new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160),
    };

    static List<Vector2Int> resolutions;

    public static List<Vector2Int> Resolutions
    {
        get
        {
            if (resolutions != null) return resolutions;
            var max = Screen.currentResolution;
            resolutions = new List<Vector2Int>();
            foreach (var r in Candidates)
                if (r.x <= max.width && r.y <= max.height) resolutions.Add(r);
            if (resolutions.Count == 0) resolutions.Add(Candidates[0]);
            return resolutions;
        }
    }

    public static Vector2Int Resolution => Resolutions[ResolutionIndex];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Load();
        ApplyAudio();
        if (PlayerPrefs.HasKey("res_w")) ApplyDisplay();
    }

    static void Load()
    {
        Master = PlayerPrefs.GetFloat("vol_master", DefaultMaster);
        Bgm = PlayerPrefs.GetFloat("vol_bgm", DefaultBgm);
        Sfx = PlayerPrefs.GetFloat("vol_sfx", DefaultSfx);
        Fullscreen = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        int w = PlayerPrefs.GetInt("res_w", Screen.width), h = PlayerPrefs.GetInt("res_h", Screen.height);
        ResolutionIndex = Closest(w, h);
    }

    static int Closest(int w, int h)
    {
        int best = 0;
        for (int i = 1; i < Resolutions.Count; i++)
            if (Mathf.Abs(Resolutions[i].x - w) < Mathf.Abs(Resolutions[best].x - w)) best = i;
        return best;
    }

    public static void SetVolumes(float master, float bgm, float sfx)
    {
        Master = master; Bgm = bgm; Sfx = sfx;
        PlayerPrefs.SetFloat("vol_master", master);
        PlayerPrefs.SetFloat("vol_bgm", bgm);
        PlayerPrefs.SetFloat("vol_sfx", sfx);
        ApplyAudio();
    }

    public static void SetDisplay(int resolutionIndex, bool fullscreen)
    {
        ResolutionIndex = Mathf.Clamp(resolutionIndex, 0, Resolutions.Count - 1);
        Fullscreen = fullscreen;
        PlayerPrefs.SetInt("res_w", Resolution.x);
        PlayerPrefs.SetInt("res_h", Resolution.y);
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        ApplyDisplay();
    }

    public static void ResetDefaults()
    {
        SetVolumes(DefaultMaster, DefaultBgm, DefaultSfx);
        SetDisplay(Closest(Screen.currentResolution.width, Screen.currentResolution.height), true);
    }

    public static void Save() => PlayerPrefs.Save();

    static void ApplyAudio()
    {
        AudioListener.volume = Master;
        SoundFx.RefreshVolumes();
    }

    static void ApplyDisplay()
    {
        var mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(Resolution.x, Resolution.y, mode);
    }
}
