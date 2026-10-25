using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SwapBox : MonoBehaviour, ISwappable
{
    public Transform Transform => transform;
    public Rigidbody2D Rb { get; private set; }
    public Collider2D Col { get; private set; }
    public bool IsAnchor => false;

    void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Col = GetComponent<Collider2D>();
        Rb.freezeRotation = true;
        Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public bool CanSwap() => true;
}
