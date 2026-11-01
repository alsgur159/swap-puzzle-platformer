using UnityEngine;

public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Check(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Check(other);
    }

    void Check(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
            return;
        }

        SwapEnemy enemy = other.GetComponent<SwapEnemy>();
        if (enemy != null)
            Destroy(enemy.gameObject);
    }
}
