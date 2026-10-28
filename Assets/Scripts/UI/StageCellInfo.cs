using UnityEngine;
using UnityEngine.EventSystems;

public class StageCellInfo : MonoBehaviour, ISelectHandler
{
    public System.Action onSelected;

    public void OnSelect(BaseEventData e) => onSelected?.Invoke();
}
