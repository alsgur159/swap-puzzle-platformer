using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlow
{
    public const string TitleScene = "Title";
    public const int StageCount = 10;

    public static readonly string[] StageNames =
    {
        "첫 걸음", "스왑 발판", "시야와 사거리", "떨어뜨리기", "던지기",
        "징검다리", "공중 교환", "남길 것과 보낼 것", "순서의 퍼즐", "자리 바꾸기",
    };

    public static bool OpenStageSelectOnTitle;

    public static int LastStartedStage { get; set; }

    public static string StageName(int n) => n >= 1 && n <= StageNames.Length ? StageNames[n - 1] : "";

    public static string Section(int n) => n <= 3 ? "튜토리얼" : n < StageCount ? "응용" : "최종";

    public static Color SectionColor(int n) => n <= 3 ? UIKit.Good : n < StageCount ? UIKit.Primary : UIKit.Gold;

    public static void LoadStage(int n) => ScreenFader.LoadScene($"Stage_{n:00}");

    public static void LoadTitle(bool openStageSelect)
    {
        OpenStageSelectOnTitle = openStageSelect;
        ScreenFader.LoadScene(TitleScene);
    }

    public static void Quit()
    {
        GameSettings.Save();
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static bool IsCleared(int n) => PlayerPrefs.GetInt("Clear_" + n, 0) == 1;
    public static bool IsUnlocked(int n) => n == 1 || IsCleared(n) || IsCleared(n - 1);

    public static int ClearedCount
    {
        get
        {
            int c = 0;
            for (int i = 1; i <= StageCount; i++) if (IsCleared(i)) c++;
            return c;
        }
    }

    public static int NextStageToPlay
    {
        get
        {
            for (int i = 1; i <= StageCount; i++) if (!IsCleared(i)) return i;
            return StageCount;
        }
    }

    public static float BestTime(int n) => PlayerPrefs.GetFloat("BestTime_" + n, -1f);
    public static int BestSwaps(int n) => PlayerPrefs.GetInt("BestSwaps_" + n, -1);

    public static bool RecordClear(int n, float time, int swapsUsed)
    {
        PlayerPrefs.SetInt("Clear_" + n, 1);
        float bt = BestTime(n);
        bool newTime = bt < 0 || time < bt;
        if (newTime) PlayerPrefs.SetFloat("BestTime_" + n, time);
        int bs = BestSwaps(n);
        if (bs < 0 || swapsUsed < bs) PlayerPrefs.SetInt("BestSwaps_" + n, swapsUsed);
        PlayerPrefs.Save();
        return newTime && bt >= 0;
    }

    public static string FormatTime(float t)
    {
        if (t < 0) return "-";
        int m = (int)(t / 60);
        return $"{m:00}:{t - m * 60:00.00}";
    }
}
