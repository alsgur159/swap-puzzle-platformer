using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;
    public float patrolRange = 3f;

    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sr;
    float startX;
    int dir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        startX = transform.position.x;
    }

    void FixedUpdate()
    {
        Bounds b = col.bounds;
        float x = rb.position.x;

        if (dir > 0 && x > startX + patrolRange) dir = -1;
        else if (dir < 0 && x < startX - patrolRange) dir = 1;
        else if (HitWall(b) || !HasGroundAhead(b)) dir = -dir;

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        if (sr != null) sr.flipX = dir < 0;
    }

    bool HitWall(Bounds b)
    {
        Vector2 origin = new Vector2(b.center.x + dir * (b.extents.x + 0.02f), b.center.y);
        return Solid(Physics2D.Raycast(origin, Vector2.right * dir, 0.1f));
    }

    bool HasGroundAhead(Bounds b)
    {
        Vector2 origin = new Vector2(b.center.x + dir * (b.extents.x + 0.1f), b.min.y + 0.05f);
        return Solid(Physics2D.Raycast(origin, Vector2.down, 0.4f));
    }

    bool Solid(RaycastHit2D h) => h.collider != null && !h.collider.isTrigger && h.collider != col;
}
