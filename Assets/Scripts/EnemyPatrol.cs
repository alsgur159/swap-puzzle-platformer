using UnityEngine;

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
        float x = rb.position.x;

        if (dir > 0 && x > startX + patrolRange)
            dir = -1;
        else if (dir < 0 && x < startX - patrolRange)
            dir = 1;
        else if (HitWall() || !HasGroundAhead())
            dir = -dir;

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
        sr.flipX = dir < 0;
    }

    bool HitWall()
    {
        Vector2 origin = new Vector2(col.bounds.center.x + dir * (col.bounds.extents.x + 0.02f), col.bounds.center.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * dir, 0.1f);
        return IsSolid(hit);
    }

    bool HasGroundAhead()
    {
        Vector2 origin = new Vector2(col.bounds.center.x + dir * (col.bounds.extents.x + 0.1f), col.bounds.min.y + 0.05f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.4f);
        return IsSolid(hit);
    }

    bool IsSolid(RaycastHit2D hit)
    {
        if (hit.collider == null) return false;
        if (hit.collider.isTrigger) return false;
        if (hit.collider == col) return false;
        return true;
    }
}
