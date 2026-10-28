using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwapController : MonoBehaviour
{
    public PlayerController player;
    public float range = 8f;
    public Sprite squareSprite;
    public Material unlitMaterial;
    public Color validColor = new Color(0.3f, 0.75f, 1f, 1f);
    public Color invalidColor = new Color(1f, 0.35f, 0.35f, 0.6f);

    public int SwapsLeft { get; private set; }
    public int SwapsMax { get; private set; }

    public event Action Swapped;

    Camera cam;
    LineRenderer line;
    SpriteRenderer highlight;

    void Start()
    {
        cam = Camera.main;
        var info = FindFirstObjectByType<StageInfo>();
        SwapsMax = SwapsLeft = info != null ? info.swapCount : 3;

        var lineGo = new GameObject("AimLine");
        lineGo.transform.SetParent(transform, false);
        line = lineGo.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 0.07f;
        line.sharedMaterial = unlitMaterial;
        line.sortingOrder = 20;

        var hlGo = new GameObject("TargetHighlight");
        hlGo.transform.SetParent(transform, false);
        highlight = hlGo.AddComponent<SpriteRenderer>();
        highlight.sprite = squareSprite;
        highlight.sharedMaterial = unlitMaterial;
        highlight.color = new Color(validColor.r, validColor.g, validColor.b, 0.35f);
        highlight.sortingOrder = 19;
        highlight.enabled = false;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || cam == null || Time.timeScale == 0f) return;

        line.enabled = SwapsMax > 0;
        if (SwapsMax == 0) { highlight.enabled = false; return; }

        Vector2 mouseWorld = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        Vector2 from = player.Rb.position;

        ISwappable target = FindTarget(mouseWorld);
        bool inReach = target != null && SwapsLeft > 0
            && Vector2.Distance(from, target.Transform.position) <= range
            && HasLineOfSight(from, target);

        UpdateAimVisual(from, mouseWorld, target, inReach);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (inReach) TrySwap(target);
            else SoundFx.SwapFail();
        }
    }

    ISwappable FindTarget(Vector2 point)
    {
        foreach (var c in Physics2D.OverlapPointAll(point))
        {
            var s = c.GetComponentInParent<ISwappable>();
            if (s != null && s.CanSwap()) return s;
        }
        return null;
    }

    bool HasLineOfSight(Vector2 from, ISwappable target)
    {
        Bounds b = target.Col.bounds;
        const float inset = 0.05f;
        Vector2 min = (Vector2)b.min + Vector2.one * inset;
        Vector2 max = (Vector2)b.max - Vector2.one * inset;
        Vector2[] samples =
        {
            b.center,
            min, max,
            new Vector2(min.x, max.y),
            new Vector2(max.x, min.y),
        };
        foreach (var p in samples)
            if (!IsBlocked(from, p, target)) return true;
        return false;
    }

    bool IsBlocked(Vector2 from, Vector2 to, ISwappable target)
    {
        foreach (var h in Physics2D.LinecastAll(from, to))
        {
            if (h.collider.isTrigger) continue;
            if (h.collider == player.Col) continue;
            if (h.collider == target.Col) continue;
            return true;
        }
        return false;
    }

    void UpdateAimVisual(Vector2 from, Vector2 mouseWorld, ISwappable target, bool inReach)
    {
        Vector2 end = mouseWorld;
        Vector2 dir = mouseWorld - from;
        if (dir.magnitude > range) end = from + dir.normalized * range;
        if (inReach) end = target.Transform.position;

        line.SetPosition(0, from);
        line.SetPosition(1, end);
        Color c = inReach ? validColor : invalidColor;
        line.startColor = line.endColor = c;

        highlight.enabled = inReach;
        if (inReach)
        {
            Bounds b = target.Col.bounds;
            highlight.transform.position = b.center;
            highlight.transform.localScale = new Vector3(b.size.x + 0.2f, b.size.y + 0.2f, 1f);
        }
    }

    void TrySwap(ISwappable target)
    {
        Rigidbody2D prb = player.Rb;
        Vector2 myPos = prb.position;
        Vector2 myVel = prb.linearVelocity;
        Vector2 tPos = target.Rb != null ? target.Rb.position : (Vector2)target.Transform.position;
        Vector2 tVel = target.Rb != null ? target.Rb.linearVelocity : Vector2.zero;

        if (!IsFree(player.Col, tPos, target.Col) ||
            (!target.IsAnchor && !IsFree(target.Col, myPos, player.Col)))
        {
            SoundFx.SwapFail();
            return;
        }

        prb.position = tPos;
        prb.transform.position = tPos;
        prb.linearVelocity = tVel;

        if (!target.IsAnchor)
        {
            target.Rb.position = myPos;
            target.Rb.transform.position = myPos;
            target.Rb.linearVelocity = myVel;
        }

        Physics2D.SyncTransforms();
        SwapsLeft--;
        SoundFx.Swap();
        Swapped?.Invoke();
    }

    bool IsFree(Collider2D mover, Vector2 pos, Collider2D ignore)
    {
        Vector2 offset = (Vector2)mover.bounds.center - (Vector2)mover.transform.position;
        Vector2 size = mover.bounds.size * 0.9f;
        foreach (var h in Physics2D.OverlapBoxAll(pos + offset, size, 0f))
        {
            if (h.isTrigger || h == mover || h == ignore) continue;
            return false;
        }
        return true;
    }
}
