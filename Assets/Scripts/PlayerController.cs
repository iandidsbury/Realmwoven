using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Gamepad virtual cursor (pixels/sec)")]
    [Tooltip("Speed the virtual cursor moves when using a gamepad right stick.")]
    public float gamepadCursorSpeed = 1200f;

    [Range(0f, 1f)]
    public float stickDeadZone = 0.15f;

    [Tooltip("If true, any mouse movement snaps the cursor to the mouse position.")]
    public bool snapToMouseWhenMoved = true;

    private Camera cam;
    private PlayerInput playerInput;
    private InputAction lookAction;      // Right stick (Vector2)
    private Vector2 virtualCursorScreen; // In screen pixels
    private float camToCursorPlaneZ;     // Positive distance from camera to the cursor's z-plane

    void Awake()
    {
        cam = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        if (cam == null)
            Debug.LogError("No Camera.main found. Assign a main camera.");
        if (playerInput == null)
            Debug.LogError("PlayerInput component required on the same GameObject.");
    }

    void OnEnable()
    {
        // Fetch Look action (right stick)
        lookAction = playerInput.actions["Look"];

        // Initialize virtual cursor to current mouse position if available,
        // otherwise to the current world position projected to screen.
        if (Mouse.current != null)
            virtualCursorScreen = Mouse.current.position.ReadValue();
        else
        {
            Vector3 startScreen = cam.WorldToScreenPoint(transform.position);
            virtualCursorScreen = new Vector2(startScreen.x, startScreen.y);
        }

        // Distance from camera to the plane where the cursor lives (its current z)
        camToCursorPlaneZ = Mathf.Abs(transform.position.z - cam.transform.position.z);
        if (camToCursorPlaneZ <= 0.0001f)
            camToCursorPlaneZ = 0.0001f; // avoid zero for ScreenToWorldPoint
    }

    void Update()
    {
        bool mousePresent = Mouse.current != null;
        bool mouseMoved = mousePresent && Mouse.current.delta.ReadValue() != Vector2.zero;

        if (mousePresent && (mouseMoved || !GamepadPresent()))
        {
            // Absolute: snap to mouse position in pixels
            if (snapToMouseWhenMoved)
                virtualCursorScreen = Mouse.current.position.ReadValue();
        }
        else
        {
            // Gamepad: move a virtual cursor in screen space
            Vector2 stick = lookAction.ReadValue<Vector2>();

            // Deadzone
            if (stick.sqrMagnitude > stickDeadZone * stickDeadZone)
            {
                virtualCursorScreen += stick * gamepadCursorSpeed * Time.deltaTime;
            }
        }

        // Clamp to current game view
        virtualCursorScreen.x = Mathf.Clamp(virtualCursorScreen.x, 0f, Screen.width);
        virtualCursorScreen.y = Mathf.Clamp(virtualCursorScreen.y, 0f, Screen.height);

        // Convert screen → world at the cursor’s z-plane
        Vector3 screen3 = new Vector3(virtualCursorScreen.x, virtualCursorScreen.y, camToCursorPlaneZ);
        Vector3 world = cam.ScreenToWorldPoint(screen3);
        world.z = transform.position.z; // preserve your cursor plane
        transform.position = world;
    }

    private bool GamepadPresent()
    {
        return Gamepad.current != null;
    }

    // Optional: call this from your Interaction action (Unity Events or SendMessage)
    public void OnInteract()
    {
        Debug.Log("Interacted with: ");
    }

    public void OnJump()
    {
        Debug.Log("Jump with: ");
    }
}
