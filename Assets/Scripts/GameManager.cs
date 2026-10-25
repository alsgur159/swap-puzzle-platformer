using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SwapController swap;
    public StageInfo stage;

    bool cleared;
    bool paused;
    GUIStyle big, mid, right, small;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame) Restart();
        if (kb.escapeKey.wasPressedThisFrame && !cleared)
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StageClear()
    {
        if (cleared) return;
        cleared = true;
        if (stage != null) PlayerPrefs.SetInt("Clear_" + stage.stageNumber, 1);
        Invoke(nameof(LoadNext), 1.5f);
    }

    void LoadNext()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
    }

    void OnGUI()
    {
        if (big == null)
        {
            big = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            mid = new GUIStyle(big) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
            right = new GUIStyle(big) { alignment = TextAnchor.UpperRight };
            small = new GUIStyle(mid) { fontSize = 24 };
            big.normal.textColor = mid.normal.textColor = right.normal.textColor = small.normal.textColor = Color.white;
        }

        if (swap != null)
            GUI.Label(new Rect(20, 12, 400, 40), "교환 " + swap.SwapsLeft + " / " + swap.SwapsMax, big);
        if (stage != null)
            GUI.Label(new Rect(Screen.width - 420, 12, 400, 40), "STAGE " + stage.stageNumber, right);

        var full = new Rect(0, 0, Screen.width, Screen.height);
        if (cleared) GUI.Label(full, "CLEAR!", mid);
        else if (paused) GUI.Label(full, "일시정지 (Esc)", mid);
        else if (swap != null && swap.SwapsMax > 0 && swap.SwapsLeft == 0)
            GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 50), "교환 횟수 소진 - R 키로 재시작", small);

        if (stage != null && !string.IsNullOrEmpty(stage.hint))
            GUI.Label(new Rect(0, Screen.height - 55, Screen.width, 40), stage.hint, small);
    }
}
