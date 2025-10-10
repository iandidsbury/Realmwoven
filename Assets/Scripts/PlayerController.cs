using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Gamepad virtual cursor (pixels/sec)")]
    [Tooltip("Speed the virtual cursor moves when using a gamepad right stick.")]
    public float gamepadCursorSpeed = 1200f;

    [Range(0f, 1f)]
    public float stickDeadZone = 0.15f;

    [Tooltip("If true, any mouse movement snaps the cursor to the mouse position.")]
    public bool snapToMouseWhenMoved = true;

    [Header("Cursor Sprites")]
    [Tooltip("Default sprite when not hovering over anything.")]
    public Sprite normalSprite;

    [Tooltip("Sprite shown when hovering over a hittable object.")]
    public Sprite hoverSprite;

    private Camera cam;
    private PlayerInput playerInput;
    private InputAction lookAction;
    private Vector2 virtualCursorScreen;
    private float camToCursorPlaneZ;
    private SpriteRenderer spriteRenderer;

    // Currently hovered hittable target
    private Hittable currentHover;

    void Awake()
    {
        cam = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (cam == null)
            Debug.LogError("No Camera.main found. Assign a main camera.");
        if (playerInput == null)
            Debug.LogError("PlayerInput component required on the same GameObject.");
    }

    void OnEnable()
    {
        lookAction = playerInput.actions["Look"];

        if (Mouse.current != null)
            virtualCursorScreen = Mouse.current.position.ReadValue();
        else
        {
            Vector3 startScreen = cam.WorldToScreenPoint(transform.position);
            virtualCursorScreen = new Vector2(startScreen.x, startScreen.y);
        }

        camToCursorPlaneZ = Mathf.Abs(transform.position.z - cam.transform.position.z);
        if (camToCursorPlaneZ <= 0.0001f)
            camToCursorPlaneZ = 0.0001f;
    }

    void Update()
    {
        UpdateCursorPosition();
        CheckHoverTarget();
    }

    private void UpdateCursorPosition()
    {
        bool mousePresent = Mouse.current != null;
        bool mouseMoved = mousePresent && Mouse.current.delta.ReadValue() != Vector2.zero;

        if (mousePresent && (mouseMoved || !GamepadPresent()))
        {
            if (snapToMouseWhenMoved)
                virtualCursorScreen = Mouse.current.position.ReadValue();
        }
        else
        {
            Vector2 stick = lookAction.ReadValue<Vector2>();
            if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
                virtualCursorScreen += stick * gamepadCursorSpeed * Time.deltaTime;
        }

        virtualCursorScreen.x = Mathf.Clamp(virtualCursorScreen.x, 0f, Screen.width);
        virtualCursorScreen.y = Mathf.Clamp(virtualCursorScreen.y, 0f, Screen.height);

        Vector3 screen3 = new Vector3(virtualCursorScreen.x, virtualCursorScreen.y, camToCursorPlaneZ);
        Vector3 world = cam.ScreenToWorldPoint(screen3);
        world.z = 0f; // ensure cursor sits on 2D plane
        transform.position = world;
    }

    private bool GamepadPresent()
    {
        return Gamepad.current != null;
    }

    private void CheckHoverTarget()
    {
        Vector2 worldPoint = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        Hittable newHover = null;

        if (hit.collider != null)
            newHover = hit.collider.GetComponent<Hittable>();

        // If hover target changed
        if (newHover != currentHover)
        {
            // Exit old hover
            if (currentHover != null)
            {
                currentHover.OnHoverExit();
                spriteRenderer.sprite = normalSprite;
            }

            // Enter new hover
            if (newHover != null)
            {
                newHover.OnHoverEnter();
                spriteRenderer.sprite = hoverSprite;
            }

            currentHover = newHover;
        }
    }

    // Called from your input system’s Interact action
    public void OnInteract()
    {
        if (currentHover != null)
        {
            currentHover.OnInteract();
        }
        else
        {
            Debug.Log("Interacted with nothing.");
        }
    }

    public void OnJump()
    {
        Debug.Log("Jump with: ");
    }
}
