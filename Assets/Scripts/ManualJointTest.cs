using UnityEngine;
using UnityEngine.InputSystem;

public class CatJointController : MonoBehaviour
{
    [Header("References")]
    public ConfigurableJoint joint;  // 在Inspector中拖入你的Joint
    
    [Header("Control Settings")]
    public float maxAngularVelocity = 5f;  // 最大角速度(rad/s)
    public float driveDamper = 1000f;      // 驱动阻尼
    public float driveForce = 500f;        // 最大驱动力
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (joint == null)
        {
            Debug.LogError("请在Inspector中指定ConfigurableJoint!");
            return;
        }
        
        SetupVelocityDrive();
    }
    
    void SetupVelocityDrive()
    {
        // 配置X轴驱动(Twist - 扭转)
        var xDrive = new JointDrive
        {
            positionSpring = 0,
            positionDamper = driveDamper,
            maximumForce = driveForce
        };
        joint.angularXDrive = xDrive;
        
        // 配置YZ轴驱动(Swing - 摆动/弯曲)
        var yzDrive = new JointDrive
        {
            positionSpring = 0,
            positionDamper = driveDamper,
            maximumForce = driveForce
        };
        joint.angularYZDrive = yzDrive;
        
        // 设置为XYZ模式
        joint.rotationDriveMode = RotationDriveMode.XYAndZ;
        
        Debug.Log("Velocity Drive配置完成");
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
        
        // 调试信息
        if (showDebugInfo && targetVelocity != Vector3.zero)
        {
            Debug.Log($"Target Velocity: {targetVelocity}, Scaled: {joint.targetAngularVelocity}");
        }
    }
    
    // 用于RL训练的接口
    public void SetJointVelocity(Vector3 velocity)
    {
        if (joint != null)
        {
            joint.targetAngularVelocity = velocity * maxAngularVelocity;
        }
    }
    
    // Gizmos可视化关节坐标系
    void OnDrawGizmos()
    {
        if (joint == null) return;
        
        Vector3 center = joint.transform.position;
        
        // X轴(红色) - Twist轴
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + joint.transform.right * 1.5f);
        Gizmos.DrawSphere(center + joint.transform.right * 1.5f, 0.1f);
        
        // Y轴(绿色) - Swing1轴
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + joint.transform.up * 1.5f);
        Gizmos.DrawSphere(center + joint.transform.up * 1.5f, 0.1f);
        
        // Z轴(蓝色) - Swing2轴
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, center + joint.transform.forward * 1.5f);
        Gizmos.DrawSphere(center + joint.transform.forward * 1.5f, 0.1f);
    }
}