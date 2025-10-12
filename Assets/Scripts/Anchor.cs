using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Anchor : Interactable
{
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnHoverEnter()
    {
        _spriteRenderer.color = Color.yellow;
    }

    public override void OnHoverExit()
    {
        _spriteRenderer.color = Color.white;
    }

    public override void OnInteract()
    {
        Debug.Log($"Interacted with {name}!");
    }
}
