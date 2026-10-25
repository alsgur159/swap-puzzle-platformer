using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class SwapEnemy : MonoBehaviour, ISwappable
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
    }

    public bool CanSwap() => true;

    void OnCollisionEnter2D(Collision2D c) => KillPlayer(c.collider);
    void OnCollisionStay2D(Collision2D c) => KillPlayer(c.collider);

    void KillPlayer(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p != null) p.Die();
    }
}
