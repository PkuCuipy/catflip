using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float smoothTime = 0.2f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 5f;
    public float wasdSensitivity = 3f;
    public float arrowKeySensitivity = 80f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    
    // 针对新的输入系统的变量
    // New Input System variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 arrowKeyInput;
    private bool isRotating;
    
    void Start()
    {
        Vector3 rotation = transform.eulerAngles;
        rotationX = rotation.y;
        rotationY = rotation.x;
        
        targetPosition = transform.position;
        
        Debug.Log("Camera Controls:");
        Debug.Log("- WASD: Move forward/backward/left/right");
        Debug.Log("- Q/E: Move down/up");
        Debug.Log("- Shift/Space: Move down/up");
        Debug.Log("- Arrow Keys: Rotate view (up/down/left/right)");
        Debug.Log("- Hold Control + Drag mouse: Rotate view");  
    }

    void FixedUpdate()
    {
        HandleInput();
        HandleMovement();
        HandleRotation();
    }

    void HandleInput()
    {
        // 获取键盘输入
        // Get keyboard input
        moveInput = Vector2.zero;
        arrowKeyInput = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveInput.y += wasdSensitivity;
            if (keyboard.sKey.isPressed) moveInput.y -= wasdSensitivity;
            if (keyboard.aKey.isPressed) moveInput.x -= wasdSensitivity;
            if (keyboard.dKey.isPressed) moveInput.x += wasdSensitivity;

            // Arrow keys for rotation
            if (keyboard.upArrowKey.isPressed) arrowKeyInput.y += 1f;
            if (keyboard.downArrowKey.isPressed) arrowKeyInput.y -= 1f;
            if (keyboard.leftArrowKey.isPressed) arrowKeyInput.x -= 1f;
            if (keyboard.rightArrowKey.isPressed) arrowKeyInput.x += 1f;
        }

        // 获取鼠标输入
        // Get mouse input
        var mouse = Mouse.current;
        if (mouse != null)
        {
            lookInput = mouse.delta.ReadValue();

            // 检查是否按下 Control 以进行旋转
            // Check if Control key is held for rotation
            isRotating = (keyboard != null && 
                         (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)) ||
                         mouse.rightButton.isPressed;
        }
    }

    void HandleMovement()
    {
        float upDown = 0f;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // Q/E for vertical movement
            if (keyboard.eKey.isPressed) upDown += wasdSensitivity;
            if (keyboard.qKey.isPressed) upDown -= wasdSensitivity;

            // Shift/Space for vertical movement (Minecraft style)
            if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed) upDown -= wasdSensitivity;
            if (keyboard.spaceKey.isPressed) upDown += wasdSensitivity;
        }

        // 计算移动方向
        // Calculate movement direction
        Vector3 direction = new Vector3(moveInput.x, upDown, moveInput.y);
        Vector3 move = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;

        // 平滑移动到目标位置
        // Smoothly move to the target position
        targetPosition += move;
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );
    }

    void HandleRotation()
    {
        // Mouse rotation (when holding Ctrl or right mouse button)
        if (isRotating && lookInput.magnitude > 0.1f)
        {
            rotationX += lookInput.x * mouseSensitivity * Time.deltaTime * 10f;
            rotationY -= lookInput.y * mouseSensitivity * Time.deltaTime * 10f;

            rotationY = Mathf.Clamp(rotationY, -90f, 90f);  // Limit vertical rotation (限制垂直旋转)

            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }

        // Arrow key rotation
        if (arrowKeyInput.magnitude > 0.01f)
        {
            rotationX += arrowKeyInput.x * arrowKeySensitivity * Time.deltaTime;
            rotationY -= arrowKeyInput.y * arrowKeySensitivity * Time.deltaTime;

            rotationY = Mathf.Clamp(rotationY, -90f, 90f);  // Limit vertical rotation (限制垂直旋转)
            
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }
}