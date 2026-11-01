using UnityEngine;
using UnityEngine.InputSystem;

public class TitleMenu : MonoBehaviour
{
    public GameObject stageSelectPanel;
    public GameObject settingsPanel;

    void Start()
    {
        Time.timeScale = 1f;
        stageSelectPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            stageSelectPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }
    }

    public void OnClickStart()
    {
        stageSelectPanel.SetActive(true);
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnClickBack()
    {
        stageSelectPanel.SetActive(false);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
