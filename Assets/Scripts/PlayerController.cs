using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite hoverSprite;

    private Camera _camera;
    private Interactable _currentInteractable;
    private SpriteRenderer _spriteRenderer;
 
    private void Awake()
    {
        _camera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        MoveCursor();
        UpdateHover();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            TryInteract();
        }
    }

    private void MoveCursor()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos.x = Mathf.Clamp(mousePos.x, 0f, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0f, Screen.height);

        Vector3 world = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_camera.transform.position.z));
        world.z = 0f;
        transform.position = world;
    }

    private void UpdateHover()
    {
        Interactable newInteractable = GetInteractableUnderCursor();

        if (newInteractable == _currentInteractable)
        {
            return; 
        }

        // Exit previous hover
        if (_currentInteractable != null)
        {
            _currentInteractable.OnHoverExit();
            _spriteRenderer.sprite = normalSprite;
        }

        // Enter new hover
        if (newInteractable != null)
        {
            newInteractable.OnHoverEnter();
            _spriteRenderer.sprite = hoverSprite;
        }

        _currentInteractable = newInteractable;
    }

    private Interactable GetInteractableUnderCursor()
    {
        Vector2 worldPoint = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider == null)
        {
            return null;
        }

        return hit.collider.GetComponent<Interactable>();
    }

    private void TryInteract()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.OnInteract();
        }
    }
}
