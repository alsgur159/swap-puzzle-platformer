using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SwapController : MonoBehaviour
{
    public float range = 8f;
    public Sprite squareSprite;
    public Material unlitMaterial;

    public int swapsLeft;
    public int maxSwaps;

    Rigidbody2D rb;
    Collider2D col;
    Camera cam;
    LineRenderer line;
    SpriteRenderer highlight;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        cam = Camera.main;

        StageInfo stage = FindFirstObjectByType<StageInfo>();
        maxSwaps = stage.swapCount;
        swapsLeft = maxSwaps;

        GameObject lineObj = new GameObject("AimLine");
        lineObj.transform.SetParent(transform);
        line = lineObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 0.07f;
        line.material = unlitMaterial;
        line.sortingOrder = 20;

        GameObject highlightObj = new GameObject("Highlight");
        highlightObj.transform.SetParent(transform);
        highlight = highlightObj.AddComponent<SpriteRenderer>();
        highlight.sprite = squareSprite;
        highlight.material = unlitMaterial;
        highlight.color = new Color(0.3f, 0.75f, 1f, 0.35f);
        highlight.sortingOrder = 19;
    }

    void Update()
    {
        line.enabled = false;
        highlight.enabled = false;

        if (maxSwaps == 0) return;
        if (GameManager.instance != null && GameManager.instance.IsStopped()) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 myPos = rb.position;

        Collider2D target = FindTarget(mousePos);
        bool canSwap = false;
        if (target != null && swapsLeft > 0)
        {
            float distance = Vector2.Distance(myPos, target.transform.position);
            if (distance <= range && CanSee(myPos, target))
                canSwap = true;
        }

        DrawAim(myPos, mousePos, target, canSwap);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (canSwap)
                Swap(target);
            else if (AudioManager.instance != null)
                AudioManager.instance.PlaySwapFail();
        }
    }

    Collider2D FindTarget(Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponent<ISwappable>() != null)
                return hits[i];
        }
        return null;
    }

    bool CanSee(Vector2 from, Collider2D target)
    {
        Bounds b = target.bounds;
        float left = b.min.x + 0.05f;
        float right = b.max.x - 0.05f;
        float bottom = b.min.y + 0.05f;
        float top = b.max.y - 0.05f;

        Vector2[] points = new Vector2[5];
        points[0] = b.center;
        points[1] = new Vector2(left, bottom);
        points[2] = new Vector2(right, top);
        points[3] = new Vector2(left, top);
        points[4] = new Vector2(right, bottom);

        for (int i = 0; i < points.Length; i++)
        {
            if (!IsBlocked(from, points[i], target))
                return true;
        }
        return false;
    }

    bool IsBlocked(Vector2 from, Vector2 to, Collider2D target)
    {
        RaycastHit2D[] hits = Physics2D.LinecastAll(from, to);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i].collider;
            if (c.isTrigger || c == col || c == target) continue;
            return true;
        }
        return false;
    }

    void DrawAim(Vector2 from, Vector2 mousePos, Collider2D target, bool canSwap)
    {
        Vector2 end = mousePos;
        if (Vector2.Distance(from, mousePos) > range)
            end = from + (mousePos - from).normalized * range;
        if (canSwap)
            end = target.transform.position;

        line.enabled = true;
        line.SetPosition(0, from);
        line.SetPosition(1, end);

        if (canSwap)
        {
            line.startColor = new Color(0.3f, 0.75f, 1f, 1f);
            line.endColor = new Color(0.3f, 0.75f, 1f, 1f);
            highlight.enabled = true;
            highlight.transform.position = target.bounds.center;
            highlight.transform.localScale = new Vector3(target.bounds.size.x + 0.2f, target.bounds.size.y + 0.2f, 1f);
        }
        else
        {
            line.startColor = new Color(1f, 0.35f, 0.35f, 0.6f);
            line.endColor = new Color(1f, 0.35f, 0.35f, 0.6f);
        }
    }

    void Swap(Collider2D target)
    {
        ISwappable swappable = target.GetComponent<ISwappable>();
        Rigidbody2D targetRb = target.attachedRigidbody;

        Vector2 myPos = rb.position;
        Vector2 myVelocity = rb.linearVelocity;
        Vector2 targetPos = target.transform.position;
        Vector2 targetVelocity = Vector2.zero;
        if (targetRb != null)
        {
            targetPos = targetRb.position;
            targetVelocity = targetRb.linearVelocity;
        }

        bool blocked = !IsEmpty(col, targetPos, target);
        if (!swappable.IsAnchor() && !IsEmpty(target, myPos, col))
            blocked = true;

        if (blocked)
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySwapFail();
            return;
        }

        rb.position = targetPos;
        transform.position = targetPos;
        rb.linearVelocity = targetVelocity;

        if (!swappable.IsAnchor())
        {
            targetRb.position = myPos;
            target.transform.position = myPos;
            targetRb.linearVelocity = myVelocity;
        }

        Physics2D.SyncTransforms();
        swapsLeft--;
        if (AudioManager.instance != null) AudioManager.instance.PlaySwap();
    }

    bool IsEmpty(Collider2D mover, Vector2 pos, Collider2D ignore)
    {
        Vector2 offset = mover.bounds.center - mover.transform.position;
        Vector2 size = mover.bounds.size * 0.9f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos + offset, size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].isTrigger || hits[i] == mover || hits[i] == ignore) continue;
            return false;
        }
        return true;
    }
}
