using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float groundAccel = 90f;
    public float airAccel = 30f;
    public float jumpSpeed = 12f;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;
    bool jumpPressed;
    bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PhysicsMaterial2D noFriction = new PhysicsMaterial2D();
        noFriction.friction = 0f;
        col.sharedMaterial = noFriction;
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.IsStopped())
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;
    }

    void FixedUpdate()
    {
        float input = 0f;
        if (GameManager.instance == null || !GameManager.instance.IsStopped())
        {
            if (Keyboard.current.aKey.isPressed) input = -1f;
            if (Keyboard.current.dKey.isPressed) input = 1f;
        }

        bool grounded = IsGrounded();
        Vector2 velocity = rb.linearVelocity;

        float accel = groundAccel;
        if (!grounded) accel = airAccel;
        velocity.x = Mathf.MoveTowards(velocity.x, input * moveSpeed, accel * Time.fixedDeltaTime);

        if (input < 0) sr.flipX = true;
        else if (input > 0) sr.flipX = false;

        if (jumpPressed && grounded)
        {
            velocity.y = jumpSpeed;
            if (AudioManager.instance != null) AudioManager.instance.PlayJump();
        }
        jumpPressed = false;

        rb.linearVelocity = velocity;
    }

    bool IsGrounded()
    {
        Vector2 center = new Vector2(col.bounds.center.x, col.bounds.min.y - 0.05f);
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, 0.1f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].isTrigger || hits[i] == col) continue;
            return true;
        }
        return false;
    }

    public void Die()
    {
        if (isDead) return;
        if (GameManager.instance != null && GameManager.instance.IsStopped()) return;

        isDead = true;
        if (AudioManager.instance != null) AudioManager.instance.PlayDeath();
        GameManager.instance.Restart();
    }
}
