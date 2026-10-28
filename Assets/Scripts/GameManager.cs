using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public SwapController swap;
    public StageInfo stage;

    public bool Cleared { get; private set; }
    public bool Paused { get; private set; }
    public bool InputLocked => Paused || Cleared;
    public float StageTime { get; private set; }
    public bool NewRecord { get; private set; }

    public event System.Action ClearedEvent;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        SoundFx.Ensure();
        gameObject.AddComponent<GameHUD>();
    }

    void Update()
    {
        if (!InputLocked) StageTime += Time.deltaTime;

        var kb = Keyboard.current;
        if (kb != null && kb.rKey.wasPressedThisFrame && UIStack.Count == 0) Restart();
    }

    public void SetPaused(bool paused)
    {
        Paused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StageClear()
    {
        if (Cleared) return;
        Cleared = true;
        SoundFx.Clear();
        if (stage != null)
        {
            int used = swap != null ? swap.SwapsMax - swap.SwapsLeft : 0;
            NewRecord = SceneFlow.RecordClear(stage.stageNumber, StageTime, used);
        }
        ClearedEvent?.Invoke();
    }

    public void LoadNext()
    {
        int next = stage != null ? stage.stageNumber + 1 : 1;
        if (next <= SceneFlow.StageCount) SceneFlow.LoadStage(next);
        else SceneFlow.LoadTitle(false);
    }
}
