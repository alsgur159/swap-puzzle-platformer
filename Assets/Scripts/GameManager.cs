using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Text swapText;
    public Text stageText;
    public Text hintText;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject clearPanel;
    public Text clearText;
    public GameObject nextButton;

    StageInfo stage;
    SwapController swap;
    bool isPaused;
    bool isCleared;

    void Awake()
    {
        instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        stage = FindFirstObjectByType<StageInfo>();
        swap = FindFirstObjectByType<SwapController>();

        stageText.text = "STAGE " + stage.stageNumber;
        hintText.text = stage.hint;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        clearPanel.SetActive(false);
    }

    void Update()
    {
        if (stage.swapCount > 0)
            swapText.text = "남은 교환 : " + swap.swapsLeft + " / " + swap.maxSwaps;
        else
            swapText.text = "";

        if (Keyboard.current.escapeKey.wasPressedThisFrame && !isCleared)
        {
            if (settingsPanel.activeSelf)
                CloseSettings();
            else if (isPaused)
                Resume();
            else
                Pause();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && !isPaused)
            Restart();
    }

    public bool IsStopped()
    {
        return isPaused || isCleared;
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        isPaused = true;
        Time.timeScale = 0f;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        if (!pausePanel.activeSelf)
            Resume();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void NextStage()
    {
        int next = stage.stageNumber + 1;
        SceneManager.LoadScene("Stage_" + next.ToString("00"));
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void StageClear()
    {
        if (isCleared) return;
        isCleared = true;

        if (AudioManager.instance != null) AudioManager.instance.PlayClear();

        if (stage.stageNumber > PlayerPrefs.GetInt("ClearStage", 0))
            PlayerPrefs.SetInt("ClearStage", stage.stageNumber);

        clearPanel.SetActive(true);
        if (stage.stageNumber == 10)
        {
            clearText.text = "모든 스테이지 클리어!";
            nextButton.SetActive(false);
        }
    }
}
