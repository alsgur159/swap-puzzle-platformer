using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) => Check(other);
    void OnTriggerStay2D(Collider2D other) => Check(other);

    void Check(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null) { player.Die(); return; }

        var enemy = other.GetComponent<SwapEnemy>();
        if (enemy != null) Destroy(enemy.gameObject);
    }
}
