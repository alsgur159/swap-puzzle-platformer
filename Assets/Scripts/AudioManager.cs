using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip jumpClip;
    public AudioClip swapClip;
    public AudioClip swapFailClip;
    public AudioClip deathClip;
    public AudioClip clearClip;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource.volume = PlayerPrefs.GetFloat("BgmVolume", 0.3f);
        sfxSource.volume = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        bgmSource.Play();
    }

    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BgmVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SfxVolume", volume);
    }

    public void PlayJump() { sfxSource.PlayOneShot(jumpClip, 0.6f); }
    public void PlaySwap() { sfxSource.PlayOneShot(swapClip); }
    public void PlaySwapFail() { sfxSource.PlayOneShot(swapFailClip, 0.7f); }
    public void PlayDeath() { sfxSource.PlayOneShot(deathClip); }
    public void PlayClear() { sfxSource.PlayOneShot(clearClip); }
}
