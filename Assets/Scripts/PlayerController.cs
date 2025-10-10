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
    [Tooltip("Dead zone for stick movement.")]
    public float stickDeadZone = 0.15f;

    [Tooltip("If true, mouse motion snaps cursor immediately to mouse position.")]
    public bool snapToMouseWhenMoved = true;

    [Header("Cursor Sprites")]
    [Tooltip("Default sprite when not hovering over anything.")]
    public Sprite normalSprite;

    [Tooltip("Sprite shown when hovering over hittable object.")]
    public Sprite hoverSprite;

    private Camera cam;
    private PlayerInput playerInput;
    private InputAction lookAction;
    private Vector2 virtualCursorScreen;
    private float camToCursorPlaneZ;
    private SpriteRenderer spriteRenderer;

    private Hittable currentHover;
    private bool usingMouse;

    // ─────────────────────────────────────────────

    void Awake()
    {
        cam = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (cam == null)
            Debug.LogError("No Camera.main found. Assign one.");
        if (playerInput == null)
            Debug.LogError("PlayerInput component required on same GameObject.");
    }

    void OnEnable()
    {
        lookAction = playerInput.actions["Look"];

        // Initialize virtual cursor position
        if (Mouse.current != null)
            virtualCursorScreen = Mouse.current.position.ReadValue();
        else
            virtualCursorScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);

        camToCursorPlaneZ = Mathf.Abs(transform.position.z - cam.transform.position.z);
        if (camToCursorPlaneZ <= 0.0001f)
            camToCursorPlaneZ = 0.0001f;

        // Hide system cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        UpdateInputMode();
        UpdateCursorPosition();
        CheckHoverTarget();
    }

    // ─────────────────────────────────────────────
    // Detect if player is using mouse or gamepad

    private void UpdateInputMode()
    {
        bool mouseMoved = Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero;
        bool stickUsed = Gamepad.current != null && lookAction.ReadValue<Vector2>().sqrMagnitude > 0.01f;

        if (mouseMoved) usingMouse = true;
        if (stickUsed) usingMouse = false;
    }

    // ─────────────────────────────────────────────
    // Move cursor sprite based on active input type

    private void UpdateCursorPosition()
    {
        if (usingMouse && Mouse.current != null)
        {
            if (snapToMouseWhenMoved)
                virtualCursorScreen = Mouse.current.position.ReadValue();
        }
        else if (Gamepad.current != null)
        {
            Vector2 stick = lookAction.ReadValue<Vector2>();
            if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
                virtualCursorScreen += stick * gamepadCursorSpeed * Time.deltaTime;
        }

        // Clamp to screen bounds
        virtualCursorScreen.x = Mathf.Clamp(virtualCursorScreen.x, 0f, Screen.width);
        virtualCursorScreen.y = Mathf.Clamp(virtualCursorScreen.y, 0f, Screen.height);

        // Convert screen → world
        Vector3 screen3 = new Vector3(virtualCursorScreen.x, virtualCursorScreen.y, camToCursorPlaneZ);
        Vector3 world = cam.ScreenToWorldPoint(screen3);
        world.z = 0f;
        transform.position = world;
    }

    // ─────────────────────────────────────────────
    // Check what the cursor is hovering over

    private void CheckHoverTarget()
    {
        Vector2 worldPoint = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        Hittable newHover = hit.collider != null ? hit.collider.GetComponent<Hittable>() : null;

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

    // ─────────────────────────────────────────────
    // Interaction input from Input System

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
        Debug.Log("Jump pressed");
    }
}
