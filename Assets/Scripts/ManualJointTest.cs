using UnityEngine;
using UnityEngine.InputSystem;

public class ManualJointTestControl : MonoBehaviour
{
    [Header("References")]
    public ConfigurableJoint joint;  // 在Inspector中拖入你的Joint
    
    [Header("Control Settings")]
    public float maxAngularVelocity = 10f;  // 最大角速度(rad/s)
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (joint == null)
        {
            Debug.LogError("请在Inspector中指定ConfigurableJoint!");
            return;
        }
    }
    
    
    void FixedUpdate()
    {
        if (joint == null) return;
        
        Vector3 targetVelocity = Vector3.zero;
        
        // 键盘输入
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // X轴 - Twist(扭转,沿着capsule长轴)
        if (keyboard.digit1Key.isPressed) targetVelocity.x = 1f;
        if (keyboard.digit2Key.isPressed) targetVelocity.x = -1f;
        
        // Y轴 - Swing1
        if (keyboard.digit3Key.isPressed) targetVelocity.y = 1f;
        if (keyboard.digit4Key.isPressed) targetVelocity.y = -1f;
        
        // Z轴 - Swing2
        if (keyboard.digit5Key.isPressed) targetVelocity.z = 1f;
        if (keyboard.digit6Key.isPressed) targetVelocity.z = -1f;
        
        // 应用目标角速度
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