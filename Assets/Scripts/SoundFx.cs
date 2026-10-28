using UnityEngine;

public class SoundFx : MonoBehaviour
{
    const float MusicBase = 0.35f;

    static SoundFx instance;

    AudioSource sfx, music;
    AudioClip jump, swap, swapFail, death, clear, click, hover;

    public static void Ensure()
    {
        if (instance != null) return;
        var go = new GameObject("SoundFx");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SoundFx>();
    }

    void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.ignoreListenerPause = true;
        jump = Load("jump");
        swap = Load("swap");
        swapFail = Load("swap_fail");
        death = Load("death");
        clear = Load("clear");
        click = Load("ui_click");
        hover = Load("ui_hover");

        music = gameObject.AddComponent<AudioSource>();
        music.clip = Load("bgm");
        music.loop = true;
        ApplyVolumes();
        if (music.clip != null) music.Play();
    }

    static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/" + name);

    void ApplyVolumes()
    {
        sfx.volume = GameSettings.Sfx;
        music.volume = MusicBase * GameSettings.Bgm;
    }

    public static void RefreshVolumes() { if (instance) instance.ApplyVolumes(); }

    void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) sfx.PlayOneShot(clip, volume);
    }

    public static void Jump() { if (instance) instance.Play(instance.jump, 0.6f); }
    public static void Swap() { if (instance) instance.Play(instance.swap); }
    public static void SwapFail() { if (instance) instance.Play(instance.swapFail, 0.7f); }
    public static void Death() { if (instance) instance.Play(instance.death); }
    public static void Clear() { if (instance) instance.Play(instance.clear); }
    public static void Click() { if (instance) instance.Play(instance.click, 0.8f); }
    public static void Hover() { if (instance) instance.Play(instance.hover, 0.6f); }
}
