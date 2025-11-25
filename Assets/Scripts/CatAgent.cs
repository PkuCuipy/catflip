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
    
    private float previousScore;

    private float DT;
    
    public override void OnEpisodeBegin()
    {
        DT = Time.fixedDeltaTime;
        
        // 随机旋转整个猫
        transform.rotation = Random.rotation;
        
        // 重置物理状态
        frontRb.linearVelocity = Vector3.zero;
        frontRb.angularVelocity = Vector3.zero;
        backRb.linearVelocity = Vector3.zero;
        backRb.angularVelocity = Vector3.zero;
        
        // 重置关节
        joint.targetAngularVelocity = Vector3.zero;
        
        // 记录初始分数
        float currentAlignment = CalculateAlignment();
        previousScore = Mathf.Exp(alignmentExpScale * (currentAlignment - 1f));
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

        // Debug.Log($"Actions received: {actions.ContinuousActions[0]}, {actions.ContinuousActions[1]}, {actions.ContinuousActions[2]}");
        
        // 应用到关节
        joint.targetAngularVelocity = targetVelocity;
        
        // 计算当前分数
        float currentAlignment = CalculateAlignment();
        float currentScore = Mathf.Exp(alignmentExpScale * (currentAlignment - 1f));
        
        // Incremental reward
        float alignmentReward = currentScore - previousScore;
        
        // 能效惩罚
        float penaltyCurve = Mathf.Exp(efficiencyExpScale * (currentAlignment - 1f));
        var a = actions.ContinuousActions;
        Vector3 actVec = new Vector3(a[0], a[1], a[2]);
        float actionMagnitude = actVec.magnitude;
        float efficiencyPenalty = -efficiencyPenaltyWeight * penaltyCurve * actionMagnitude * DT;
        
        AddReward(alignmentReward + efficiencyPenalty);
        
        previousScore = currentScore;

        // Debug.Log($"currentScore: {currentScore}");
    }
    
    private float CalculateAlignment()
    {
        // 腹部朝向与世界上方向的点积 (front.right 是朝上的, back.right 是朝下的)
        float frontAlignment = Vector3.Dot(frontBody.right, Vector3.up);
        float backAlignment = Vector3.Dot(-backBody.right, Vector3.up);
        float alignment = (frontAlignment + backAlignment) / 2f;  // 范围 [-1, 1]
        return (alignment + 1f) / 2f;  // 转换到 [0, 1]
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