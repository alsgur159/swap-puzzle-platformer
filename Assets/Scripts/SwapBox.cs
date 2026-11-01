using UnityEngine;

public class SwapBox : MonoBehaviour, ISwappable
{
    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public bool IsAnchor()
    {
        return false;
    }
}
