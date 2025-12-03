using UnityEngine;
using UnityEngine.InputSystem;

public class ManualJointTestControl : MonoBehaviour
{
    [Header("References")]
    public ConfigurableJoint joint;
    
    [Header("Control Settings")]
    public float maxAngularVelocity = 10f;
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (joint == null)
        {
            Debug.LogError("Please assign a ConfigurableJoint to ManualJointTestControl!");
            return;
        }
    }
    
    
    void FixedUpdate()
    {
        if (joint == null) return;
        
        Vector3 targetVelocity = Vector3.zero;
        
        // Keyboard input
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // X axis - Twist
        if (keyboard.digit1Key.isPressed) targetVelocity.x = 1f;
        if (keyboard.digit2Key.isPressed) targetVelocity.x = -1f;
        
        // Y axis - Swing1
        if (keyboard.digit3Key.isPressed) targetVelocity.y = 1f;
        if (keyboard.digit4Key.isPressed) targetVelocity.y = -1f;
        
        // Z axis - Swing2
        if (keyboard.digit5Key.isPressed) targetVelocity.z = 1f;
        if (keyboard.digit6Key.isPressed) targetVelocity.z = -1f;
        
        // Normalize and apply
        joint.targetAngularVelocity = targetVelocity * maxAngularVelocity;
    }
    
    public void SetJointVelocity(Vector3 velocity)
    {
        if (joint != null)
        {
            joint.targetAngularVelocity = velocity * maxAngularVelocity;
        }
    }
}