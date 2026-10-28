using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    static ScreenFader instance;
    Image black;
    bool busy;

    static ScreenFader Get()
    {
        if (instance != null) return instance;
        var canvas = UIKit.CreateCanvas("ScreenFader", 1000);
        DontDestroyOnLoad(canvas.gameObject);
        instance = canvas.gameObject.AddComponent<ScreenFader>();
        instance.black = UIKit.Image(canvas.transform, "Black", null, Color.black.WithAlpha(0f));
        instance.black.rectTransform.Stretch();
        return instance;
    }

    public static void LoadScene(string name)
    {
        var f = Get();
        if (f.busy) return;
        f.StartCoroutine(f.Run(name));
    }

    public static void FadeIn(float duration = 0.4f)
    {
        var f = Get();
        if (f.busy) return;
        f.black.color = Color.black;
        f.StartCoroutine(f.Fade(1f, 0f, duration));
    }

    IEnumerator Run(string scene)
    {
        busy = true;
        black.raycastTarget = true;
        yield return Fade(0f, 1f, 0.22f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
        yield return null;
        yield return Fade(1f, 0f, 0.3f);
        black.raycastTarget = false;
        busy = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            black.color = Color.black.WithAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        black.color = Color.black.WithAlpha(to);
    }
}
