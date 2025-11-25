using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CatAgent : Agent
{
    [Header("Body References")]
    [SerializeField] private Transform frontBody;
    [SerializeField] private Transform backBody;
    [SerializeField] private Rigidbody frontRb;
    [SerializeField] private Rigidbody backRb;
    [SerializeField] private ConfigurableJoint joint;
    
    [Header("Training Parameters")]
    [SerializeField] private float maxAngularVelocity = 10f;
    [SerializeField] private float alignmentExpScale = 3f;  // 对齐度的 exp 缩放
    [SerializeField] private float efficiencyExpScale = 3f;  // 能效惩罚的 exp 缩放
    [SerializeField] private float efficiencyPenaltyWeight = 0.05f;
    
    private float previousAlignment;
    
    public override void OnEpisodeBegin()
    {
        // 随机旋转整个猫
        transform.rotation = Random.rotation;
        
        // 重置物理状态
        frontRb.linearVelocity = Vector3.zero;
        frontRb.angularVelocity = Vector3.zero;
        backRb.linearVelocity = Vector3.zero;
        backRb.angularVelocity = Vector3.zero;
        
        // 重置关节
        joint.targetAngularVelocity = Vector3.zero;
        
        // 记录初始对齐度
        previousAlignment = CalculateAlignment();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // obs[0:2]: FrontBody 重力方向在局部坐标系
        Vector3 frontLocalDown = frontBody.InverseTransformDirection(Vector3.down);
        sensor.AddObservation(frontLocalDown);
        
        // obs[3:5]: BackBody 重力方向在局部坐标系
        Vector3 backLocalDown = backBody.InverseTransformDirection(Vector3.down);
        sensor.AddObservation(backLocalDown);
        
        // obs[6:8]: FrontBody 角速度在局部坐标系
        Vector3 frontLocalAngularVel = frontBody.InverseTransformDirection(frontRb.angularVelocity);
        sensor.AddObservation(frontLocalAngularVel);
        
        // obs[9:11]: BackBody 角速度在局部坐标系
        Vector3 backLocalAngularVel = backBody.InverseTransformDirection(backRb.angularVelocity);
        sensor.AddObservation(backLocalAngularVel);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 获取动作并缩放到 [-maxAngularVelocity, maxAngularVelocity]
        Vector3 targetVelocity = new Vector3(
            actions.ContinuousActions[0],
            actions.ContinuousActions[1],
            actions.ContinuousActions[2]
        ) * maxAngularVelocity;
        
        // 应用到关节
        joint.targetAngularVelocity = targetVelocity;
        
        // 计算奖励
        float currentAlignment = CalculateAlignment();
        float deltaAlignment = currentAlignment - previousAlignment;
        
        // 对齐度增量奖励（套 exp 平滑）
        float alignmentReward = Mathf.Exp(alignmentExpScale * currentAlignment) * deltaAlignment;
        
        // 能效惩罚
        float normalizedAlignment = (currentAlignment + 1f) / 2f;  // [-1,1] → [0,1]
        float penaltyCurve = Mathf.Exp(efficiencyExpScale * (normalizedAlignment - 1f));
        var a = actions.ContinuousActions;
        Vector3 actVec = new Vector3(a[0], a[1], a[2]);
        float actionMagnitude = actVec.magnitude;
        float efficiencyPenalty = -efficiencyPenaltyWeight * penaltyCurve * actionMagnitude;
        
        AddReward(alignmentReward + efficiencyPenalty);
        
        previousAlignment = currentAlignment;
    }
    
    private float CalculateAlignment()
    {
        // 腹部朝向与世界上方向的点积 (front.right 是朝上的, back.right 是朝下的)
        float frontAlignment = Vector3.Dot(frontBody.right, Vector3.up);
        float backAlignment = Vector3.Dot(-backBody.right, Vector3.up);
        Debug.Log($"Front Alignment: {frontAlignment}, Back Alignment: {backAlignment}");
        return (frontAlignment + backAlignment) / 2f;  // 范围 [-1, 1]
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 用于手动测试（可选）
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = 0f;
        continuousActions[1] = 0f;
        continuousActions[2] = 0f;
    }
}