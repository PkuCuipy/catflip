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
    [SerializeField] private float angularVelocityPenaltyWeight = 0.01f;  // 角速度惩罚权重
    
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
        previousScore = CalculateScore();
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
        float currentScore = CalculateScore();
        
        // Incremental reward
        float alignmentReward = currentScore - previousScore;
        
        // 能效惩罚 (使用当前分数来调节惩罚权重)
        float penaltyCurvingCoef = currentScore;
        var a = actions.ContinuousActions;
        Vector3 actVec = new Vector3(a[0], a[1], a[2]);
        float actionMagnitude = actVec.magnitude;
        float efficiencyPenalty = -efficiencyPenaltyWeight * penaltyCurvingCoef * actionMagnitude * DT;
        
        // 角速度惩罚 (分数越高惩罚越高，鼓励稳定)
        float totalAngularVelocity = frontRb.angularVelocity.magnitude + backRb.angularVelocity.magnitude;
        float angularVelocityPenalty = -angularVelocityPenaltyWeight * penaltyCurvingCoef * totalAngularVelocity * DT;
        
        AddReward(alignmentReward + efficiencyPenalty + angularVelocityPenalty);
        
        previousScore = currentScore;
        // Debug.Log($"currentScore: {currentScore}");
    }
    
    private float CalculateScore()
    {
        // 腹部朝向与世界上方向的点积 (front.right 是朝上的, back.right 是朝下的)
        float frontAlignment = Vector3.Dot(frontBody.right, Vector3.up);
        float backAlignment = Vector3.Dot(-backBody.right, Vector3.up);
        
        // 转换到 [0, 1] 范围
        float frontNormalized = (frontAlignment + 1f) / 2f;
        float backNormalized = (backAlignment + 1f) / 2f;
        
        // 分别计算 exp, 然后取平均
        float frontScore = Mathf.Exp(alignmentExpScale * (frontNormalized - 1f));
        float backScore = Mathf.Exp(alignmentExpScale * (backNormalized - 1f));
        
        return (frontScore + backScore) / 2f;
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