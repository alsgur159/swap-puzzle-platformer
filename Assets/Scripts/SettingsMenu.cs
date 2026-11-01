using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    int[] widths = { 1280, 1600, 1920 };
    int[] heights = { 720, 900, 1080 };

    void OnEnable()
    {
        bgmSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("BgmVolume", 0.3f));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SfxVolume", 0.8f));
        resolutionDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("Resolution", 2));
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    public void OnBgmChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetBgmVolume(value);
    }

    public void OnSfxChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSfxVolume(value);
    }

    public void OnResolutionChanged(int index)
    {
        Screen.SetResolution(widths[index], heights[index], Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", index);
    }

    public void OnFullscreenChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
