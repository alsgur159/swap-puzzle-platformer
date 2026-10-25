using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SwapPad : MonoBehaviour, ISwappable
{
    public Transform Transform => transform;
    public Rigidbody2D Rb => null;
    public Collider2D Col { get; private set; }
    public bool IsAnchor => true;

    void Awake() => Col = GetComponent<Collider2D>();

    public bool CanSwap() => true;
}
