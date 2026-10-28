using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler,
    IPointerClickHandler, ISubmitHandler
{
    public bool scaleOnSelect = true;
    public bool deselectOnExit;
    public GameObject tooltip;

    static int suppressFrame = -1;
    Selectable selectable;
    float target = 1f;

    public static void SuppressSelectSound() => suppressFrame = Time.frameCount;

    void Awake() => selectable = GetComponent<Selectable>();

    void OnDisable()
    {
        target = 1f;
        transform.localScale = Vector3.one;
    }

    bool Interactable => selectable != null && selectable.IsInteractable();

    public void OnPointerEnter(PointerEventData e)
    {
        if (Interactable && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (deselectOnExit && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnSelect(BaseEventData e)
    {
        if (tooltip != null) tooltip.SetActive(true);
        if (scaleOnSelect) target = 1.05f;
        if (Time.frameCount - suppressFrame > 1) SoundFx.Hover();
    }

    public void OnDeselect(BaseEventData e)
    {
        target = 1f;
        if (tooltip != null) tooltip.SetActive(false);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && Interactable && selectable is Button) SoundFx.Click();
        OnPointerExit(e);
    }

    public void OnSubmit(BaseEventData e)
    {
        if (Interactable && selectable is Button) SoundFx.Click();
    }

    void Update()
    {
        float s = transform.localScale.x;
        if (Mathf.Abs(s - target) < 0.001f) return;
        s = Mathf.Lerp(s, target, 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }
}
