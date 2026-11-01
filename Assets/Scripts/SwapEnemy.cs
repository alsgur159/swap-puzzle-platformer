using UnityEngine;

public class SwapEnemy : MonoBehaviour, ISwappable
{
    void Awake()
    {
        GetComponent<Rigidbody2D>().freezeRotation = true;
    }

    public bool IsAnchor()
    {
        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        KillPlayer(collision.collider);
    }

    void KillPlayer(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.Die();
    }
}
