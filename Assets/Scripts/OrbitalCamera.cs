using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// Orbital camera that always looks at the origin (0,0,0)
// Supports both desktop (WASD/Arrow keys/Mouse) and mobile (touch) controls
public class OrbitalCamera : MonoBehaviour
{
    [Header("Orbital Settings")]
    [SerializeField] private Transform followTarget = null;       // Optional: Transform to follow (e.g., cat's spine)
    [SerializeField] private Vector3 targetPoint = Vector3.zero;  // Point to orbit around (used if followTarget is null)
    [SerializeField] private float distance = 15f;                // Distance from target
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Rotation Settings")]
    [SerializeField] private float theta = 45f;     // Horizontal angle (degrees)
    [SerializeField] private float phi = 60f;       // Vertical angle from top (degrees, 0=top, 90=horizon, 180=bottom)
    [SerializeField] private float minPhi = 5f;   // Prevent gimbal lock
    [SerializeField] private float maxPhi = 175f;   // Prevent gimbal lock

    [Header("Input Sensitivity")]
    [SerializeField] private float keyboardRotateSpeed = 50f;      // Degrees per second
    [SerializeField] private float mouseRotateSpeed = 0.3f;        // Degrees per pixel
    [SerializeField] private float scrollZoomSpeed = 2f;
    [SerializeField] private float keyboardZoomSpeed = 10f;        // Units per second
    [SerializeField] private float touchRotateSpeed = 0.3f;        // Degrees per pixel
    [SerializeField] private float pinchZoomSpeed = 0.02f;

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private bool enableSmoothing = false;         // Disabled for snappier touch response

    [Header("Debug")]
    [SerializeField] private bool debugTouch = false;              // Enable to see touch input logs

    // Smooth damping
    private float smoothTheta;
    private float smoothPhi;
    private float smoothDistance;
    private float thetaVelocity;
    private float phiVelocity;
    private float distanceVelocity;

    // Touch input
    private Vector2? lastTouchPosition = null;
    private float? lastPinchDistance = null;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        // Initialize smooth values
        smoothTheta = theta;
        smoothPhi = phi;
        smoothDistance = distance;

        UpdateCameraPosition();

        Debug.Log("Orbital Camera Controls:");
        Debug.Log("Desktop:");
        Debug.Log("  - WASD / Arrow Keys: Rotate around target");
        Debug.Log("  - Mouse Drag (Right Click): Rotate around target");
        Debug.Log("  - Mouse Scroll / +/- Keys: Zoom in/out");
        Debug.Log("Mobile:");
        Debug.Log("  - Single finger swipe: Rotate around target");
        Debug.Log("  - Two finger pinch: Zoom in/out");
    }

    void LateUpdate()
    {
        HandleDesktopInput();
        HandleTouchInput();

        // Clamp values
        phi = Mathf.Clamp(phi, minPhi, maxPhi);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        UpdateCameraPosition();
    }

    void HandleDesktopInput()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null && mouse == null) return;

        // Keyboard rotation (WASD or Arrow Keys)
        if (keyboard != null)
        {
            float horizontalInput = 0f;
            float verticalInput = 0f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) verticalInput -= 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) verticalInput += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput += 1f;

            theta += horizontalInput * keyboardRotateSpeed * Time.deltaTime;
            phi += verticalInput * keyboardRotateSpeed * Time.deltaTime;

            // Keyboard zoom (+/- or Q/E)
            float zoomInput = 0f;
            if (keyboard.equalsKey.isPressed || keyboard.eKey.isPressed) zoomInput = -1f;  // Zoom in
            if (keyboard.minusKey.isPressed || keyboard.qKey.isPressed) zoomInput = 1f;    // Zoom out

            distance += zoomInput * keyboardZoomSpeed * Time.deltaTime;
        }

        // Mouse rotation (Right Click + Drag)
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            theta -= mouseDelta.x * mouseRotateSpeed;
            phi += mouseDelta.y * mouseRotateSpeed;
        }

        // Mouse scroll zoom
        if (mouse != null)
        {
            Vector2 scroll = mouse.scroll.ReadValue();
            distance -= scroll.y * scrollZoomSpeed * 0.01f;  // scroll.y is typically in units of 120
        }
    }

    void HandleTouchInput()
    {
        int touchCount = Touch.activeTouches.Count;

        if (debugTouch && touchCount > 0)
        {
            Debug.Log($"Active touches: {touchCount}");
        }

        // Single touch - rotation
        if (touchCount == 1)
        {
            var touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                lastTouchPosition = touch.screenPosition;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (lastTouchPosition.HasValue)
                {
                    Vector2 delta = touch.screenPosition - lastTouchPosition.Value;

                    // Left/right swipe = horizontal rotation, up/down swipe = vertical rotation
                    theta -= delta.x * touchRotateSpeed;
                    phi += delta.y * touchRotateSpeed;  // Inverted: swipe up = look up

                    if (debugTouch)
                    {
                        Debug.Log($"Rotation - Delta: {delta}, Theta: {theta:F1}, Phi: {phi:F1}");
                    }
                }

                lastTouchPosition = touch.screenPosition;
            }

            // Reset pinch state
            lastPinchDistance = null;
        }
        // Two touches - pinch zoom
        else if (touchCount >= 2)
        {
            var touch0 = Touch.activeTouches[0];
            var touch1 = Touch.activeTouches[1];

            float currentPinchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

            if (lastPinchDistance.HasValue)
            {
                float deltaPinch = currentPinchDistance - lastPinchDistance.Value;
                distance -= deltaPinch * pinchZoomSpeed;

                if (debugTouch)
                {
                    Debug.Log($"Pinch - Delta: {deltaPinch:F1}, Distance: {distance:F1}");
                }
            }

            lastPinchDistance = currentPinchDistance;

            // Reset rotation state
            lastTouchPosition = null;
        }
        else
        {
            // No touches - reset all
            lastTouchPosition = null;
            lastPinchDistance = null;
        }
    }

    void UpdateCameraPosition()
    {
        // Update target point if following a Transform
        Vector3 currentTarget = targetPoint;
        if (followTarget != null)
        {
            currentTarget = followTarget.position;
        }

        // Apply smoothing if enabled
        if (enableSmoothing)
        {
            smoothTheta = Mathf.SmoothDampAngle(smoothTheta, theta, ref thetaVelocity, smoothTime);
            smoothPhi = Mathf.SmoothDampAngle(smoothPhi, phi, ref phiVelocity, smoothTime);
            smoothDistance = Mathf.SmoothDamp(smoothDistance, distance, ref distanceVelocity, smoothTime);
        }
        else
        {
            smoothTheta = theta;
            smoothPhi = phi;
            smoothDistance = distance;
        }

        // Convert spherical coordinates to Cartesian coordinates
        float thetaRad = smoothTheta * Mathf.Deg2Rad;
        float phiRad = smoothPhi * Mathf.Deg2Rad;

        float x = smoothDistance * Mathf.Sin(phiRad) * Mathf.Cos(thetaRad);
        float y = smoothDistance * Mathf.Cos(phiRad);
        float z = smoothDistance * Mathf.Sin(phiRad) * Mathf.Sin(thetaRad);

        Vector3 newPosition = currentTarget + new Vector3(x, y, z);
        transform.position = newPosition;

        // Always look at the target point
        transform.LookAt(currentTarget);
    }

    // Public methods to control camera programmatically

    // Set a Transform to follow (e.g., cat's spine). Set to null to stop following.
    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    // Set a static point to orbit around. This is ignored if followTarget is set.
    public void SetTargetPoint(Vector3 target)
    {
        targetPoint = target;
        followTarget = null;  // Clear follow target when setting static point
    }

    // Set the distance from the target.
    public void SetDistance(float dist)
    {
        distance = Mathf.Clamp(dist, minDistance, maxDistance);
    }

    // Set the camera angles.
    public void SetAngles(float horizontalAngle, float verticalAngle)
    {
        theta = horizontalAngle;
        phi = Mathf.Clamp(verticalAngle, minPhi, maxPhi);
    }

    // Get the current target position (either followTarget position or static targetPoint).
    public Vector3 GetCurrentTarget()
    {
        return followTarget != null ? followTarget.position : targetPoint;
    }
}
