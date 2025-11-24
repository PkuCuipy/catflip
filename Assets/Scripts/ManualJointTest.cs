using UnityEngine;

public class ManualJointTest : MonoBehaviour
{
    [Header("Body Parts")]
    public Rigidbody frontBody;
    public Rigidbody backBody;
    
    [Header("Torque Settings")]
    public float maxTorque = 50f;
    
    void FixedUpdate()
    {
        // 在 FrontBody 的局部坐标系里定义力矩向量
        Vector3 localTorque = Vector3.zero;
        
        // 键盘输入
        if (UnityEngine.InputSystem.Keyboard.current.digit1Key.isPressed) localTorque.x = 1f;   // Pitch +
        if (UnityEngine.InputSystem.Keyboard.current.digit2Key.isPressed) localTorque.x = -1f;  // Pitch -
        if (UnityEngine.InputSystem.Keyboard.current.digit3Key.isPressed) localTorque.z = 1f;   // Roll +
        if (UnityEngine.InputSystem.Keyboard.current.digit4Key.isPressed) localTorque.z = -1f;  // Roll -
        if (UnityEngine.InputSystem.Keyboard.current.digit5Key.isPressed) localTorque.y = 1f;   // Yaw + (应该被限制)
        if (UnityEngine.InputSystem.Keyboard.current.digit6Key.isPressed) localTorque.y = -1f;  // Yaw - (应该被限制)
        
        // 转换到世界坐标系
        Vector3 worldTorque = frontBody.transform.TransformDirection(localTorque * maxTorque);
        
        // 对两个刚体施加相反的力矩
        frontBody.AddTorque(worldTorque, ForceMode.Force);
        backBody.AddTorque(-worldTorque, ForceMode.Force);
        
        if (localTorque != Vector3.zero)
        {
            Debug.Log($"Local Torque: {localTorque}, World Torque: {worldTorque}");
        }
    }
    
    // 可视化坐标轴
    void OnDrawGizmos()
    {
        if (frontBody == null) return;
        
        Vector3 center = frontBody.position;
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + frontBody.transform.right * 1.5f);
        Gizmos.DrawSphere(center + frontBody.transform.right * 1.5f, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + frontBody.transform.up * 1.5f);
        Gizmos.DrawSphere(center + frontBody.transform.up * 1.5f, 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, center + frontBody.transform.forward * 1.5f);
        Gizmos.DrawSphere(center + frontBody.transform.forward * 1.5f, 0.1f);
    }
}