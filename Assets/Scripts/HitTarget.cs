using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitTarget : Hittable
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnHoverEnter()
    {
        Debug.Log($"Hovering over {name}");
        // Optional: change color, play animation, etc.
        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;
    }

    public override void OnHoverExit()
    {
        Debug.Log($"Stopped hovering over {name}");
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public override void OnInteract()
    {
        Debug.Log($"Interacted with {name}!");
        // Whatever action you want
    }
}
