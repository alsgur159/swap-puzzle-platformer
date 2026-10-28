using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float groundAccel = 90f;
    public float airAccel = 30f;
    public float jumpSpeed = 12f;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    bool jumpQueued;
    bool dead;

    public Rigidbody2D Rb => rb;
    public BoxCollider2D Col => col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.sharedMaterial = new PhysicsMaterial2D("Player") { friction = 0f, bounciness = 0f };
    }

    static bool Locked => GameManager.Instance != null && GameManager.Instance.InputLocked;

    void Update()
    {
        if (Locked) return;
        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame) jumpQueued = true;
    }

    void FixedUpdate()
    {
        var kb = Keyboard.current;
        float input = 0f;
        if (kb != null && !Locked)
        {
            if (kb.aKey.isPressed) input -= 1f;
            if (kb.dKey.isPressed) input += 1f;
        }

        bool grounded = IsGrounded();
        Vector2 v = rb.linearVelocity;
        float accel = grounded ? groundAccel : airAccel;
        v.x = Mathf.MoveTowards(v.x, input * moveSpeed, accel * Time.fixedDeltaTime);

        if (input != 0f && sr != null) sr.flipX = input < 0f;

        if (jumpQueued && grounded)
        {
            v.y = jumpSpeed;
            SoundFx.Jump();
        }
        jumpQueued = false;

        rb.linearVelocity = v;
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - 0.05f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.1f);
        var hits = Physics2D.OverlapBoxAll(center, size, 0f);
        foreach (var h in hits)
        {
            if (h.isTrigger || h == col) continue;
            return true;
        }
        return false;
    }

    public void Die()
    {
        if (dead || (GameManager.Instance != null && GameManager.Instance.Cleared)) return;
        dead = true;
        SoundFx.Death();
        GameManager.Instance.Restart();
    }
}
