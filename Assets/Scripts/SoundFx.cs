using UnityEngine;

// 씬이 재시작돼도 끊기지 않도록 DontDestroyOnLoad로 유지되는 효과음/BGM 재생기.
// 클립은 Resources/Audio 에서 이름으로 불러온다.
public class SoundFx : MonoBehaviour
{
    static SoundFx instance;

    AudioSource sfx;
    AudioClip jump, swap, swapFail, death, clear;

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
        jump = Load("jump");
        swap = Load("swap");
        swapFail = Load("swap_fail");
        death = Load("death");
        clear = Load("clear");

        var music = gameObject.AddComponent<AudioSource>();
        music.clip = Load("bgm");
        music.loop = true;
        music.volume = 0.35f;
        if (music.clip != null) music.Play();
    }

    static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/" + name);

    void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) sfx.PlayOneShot(clip, volume);
    }

    public static void Jump() { if (instance) instance.Play(instance.jump, 0.6f); }
    public static void Swap() { if (instance) instance.Play(instance.swap); }
    public static void SwapFail() { if (instance) instance.Play(instance.swapFail, 0.7f); }
    public static void Death() { if (instance) instance.Play(instance.death); }
    public static void Clear() { if (instance) instance.Play(instance.clear); }
}
