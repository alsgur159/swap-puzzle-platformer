using UnityEngine;

public interface ISwappable
{
    Transform Transform { get; }
    Rigidbody2D Rb { get; }
    Collider2D Col { get; }

    bool IsAnchor { get; }

    bool CanSwap();
}
