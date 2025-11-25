using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float fastMoveSpeed = 10f;
    public float smoothTime = 0.2f;

    [Header("旋转设置")]
    public float mouseSensitivity = 5f;
    public float wasdSensitivity = 3f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    
    // 新 Input System 的输入
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRotating;
    
    void Start()
    {
        Vector3 rotation = transform.eulerAngles;
        rotationX = rotation.y;
        rotationY = rotation.x;
        
        targetPosition = transform.position;
        
        Debug.Log("相机控制:");
        Debug.Log("- WASD: 前后左右移动");
        Debug.Log("- Q/E: 下降/上升");
        Debug.Log("- Shift: 加速移动");
        Debug.Log("- 按住 Control + 触控板拖拽: 旋转视角");
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
        moveInput = Vector2.zero;
        
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) moveInput.y += wasdSensitivity;
            if (keyboard.sKey.isPressed) moveInput.y -= wasdSensitivity;
            if (keyboard.aKey.isPressed) moveInput.x -= wasdSensitivity;
            if (keyboard.dKey.isPressed) moveInput.x += wasdSensitivity;
        }
        
        // 获取鼠标/触控板输入
        var mouse = Mouse.current;
        if (mouse != null)
        {
            lookInput = mouse.delta.ReadValue();
            
            // 检查是否应该旋转（按住 Control 或右键）
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
            if (keyboard.eKey.isPressed) upDown = wasdSensitivity;
            if (keyboard.qKey.isPressed) upDown = -wasdSensitivity;
        }
        
        // 检查是否按住 Shift 加速
        bool isSprinting = keyboard != null && 
                          (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        float speed = isSprinting ? fastMoveSpeed : moveSpeed;
        
        // 计算移动方向
        Vector3 direction = new Vector3(moveInput.x, upDown, moveInput.y);
        Vector3 move = transform.TransformDirection(direction) * speed * Time.deltaTime;
        
        // 平滑移动
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
        if (isRotating && lookInput.magnitude > 0.1f)
        {
            var mouse = Mouse.current;
            float sensitivity = mouseSensitivity;
            rotationX += lookInput.x * sensitivity * Time.deltaTime * 10f;
            rotationY -= lookInput.y * sensitivity * Time.deltaTime * 10f;
            
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);  // 限制垂直旋转角度
            
            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
    }
}