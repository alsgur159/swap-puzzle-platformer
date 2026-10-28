using UnityEngine;

public class UIPopIn : MonoBehaviour
{
    public RectTransform target;
    public float duration = 0.18f;

    CanvasGroup group;
    float t;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        t = 0f;
        Apply(0f);
    }

    void Update()
    {
        if (t >= duration) return;
        t += Time.unscaledDeltaTime;
        Apply(Mathf.Clamp01(t / duration));
    }

    void Apply(float k)
    {
        if (group != null) group.alpha = k;
        if (target != null)
        {
            float e = 1f - Mathf.Pow(1f - k, 3f);
            float s = Mathf.Lerp(0.92f, 1f, e);
            target.localScale = new Vector3(s, s, 1f);
        }
    }
}
