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
    [SerializeField] private float alignmentExpScale = 2f;  // Exp decay scale for alignment reward (对齐奖励的指数衰减尺度)
    [SerializeField] private float efficiencyPenaltyWeight = 0.05f;
    [SerializeField] private float relativeAngularVelocityPenaltyWeight = 0.005f;  // Penalty weight for relative angular velocity (相对角速度惩罚权重)
    [SerializeField] private int maxSteps = 250;  // Maximum steps per episode (每个回合的最大步数)
    
    private float previousScore;
    private float DT;
    
    // 用于 Logging 的累计统计数据
    // Cumulative statistics for logging
    private float cumulativeAlignmentRewardForStatOnly;
    private float cumulativeEfficiencyPenaltyForStatOnly;
    private float cumulativeAngularVelocityPenaltyForStatOnly;
    private int stepCount;
    
    public override void OnEpisodeBegin()
    {
        DT = Time.fixedDeltaTime;
        
        // 在每个回合开始时随机旋转猫咪
        // Randomly rotate the cat at the start of each episode
        transform.rotation = Random.rotation;
        
        // 重置位置
        // Reset physics
        frontRb.linearVelocity = Vector3.zero;
        frontRb.angularVelocity = Vector3.zero;
        backRb.linearVelocity = Vector3.zero;
        backRb.angularVelocity = Vector3.zero;
        
        // 重置关节
        // Reset joint
        joint.targetAngularVelocity = Vector3.zero;
        
        // 计算初始分数
        // Initialize "previous score" as initial score
        previousScore = CalculateScore();
        
        // 重置累计统计数据
        // Reset cumulative statistics
        cumulativeAlignmentRewardForStatOnly = 0f;
        cumulativeEfficiencyPenaltyForStatOnly = 0f;
        cumulativeAngularVelocityPenaltyForStatOnly = 0f;
        stepCount = 0;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // obs[0:2]: FrontBody 重力方向在局部坐标系
        // obs[0:2]: Gravity direction in FrontBody local space
        Vector3 frontLocalDown = frontBody.InverseTransformDirection(Vector3.down);
        sensor.AddObservation(frontLocalDown);
        
        // obs[3:5]: BackBody 重力方向在局部坐标系
        // obs[3:5]: Gravity direction in BackBody local space
        Vector3 backLocalDown = backBody.InverseTransformDirection(Vector3.down);
        sensor.AddObservation(backLocalDown);
        
        // obs[6:8]: FrontBody 角速度在局部坐标系
        // obs[6:8]: FrontBody angular velocity in local space
        Vector3 frontLocalAngularVel = frontBody.InverseTransformDirection(frontRb.angularVelocity);
        sensor.AddObservation(frontLocalAngularVel);
        
        // obs[9:11]: BackBody 角速度在局部坐标系
        // obs[9:11]: BackBody angular velocity in local space
        Vector3 backLocalAngularVel = backBody.InverseTransformDirection(backRb.angularVelocity);
        sensor.AddObservation(backLocalAngularVel);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 获取动作并缩放到 [-maxAngularVelocity, maxAngularVelocity]
        // Get target angular velocity from actions and scale to [-maxAngularVelocity, maxAngularVelocity]
        Vector3 targetVelocity = new Vector3(
            actions.ContinuousActions[0],
            actions.ContinuousActions[1],
            actions.ContinuousActions[2]
        ) * maxAngularVelocity;
        
        // 应用到关节
        // Apply to joint
        joint.targetAngularVelocity = targetVelocity;
        
        // 计算当前分数
        // Calculate current score
        float currentScore = CalculateScore();
        
        // 计算奖励
        // Incremental reward
        float alignmentReward = currentScore - previousScore;
        
        // 能效惩罚 (使用当前分数来调节惩罚权重)
        // Efficiency penalty (scaled by current score)
        float penaltyCurve = currentScore;
        var a = actions.ContinuousActions;
        Vector3 actVec = new Vector3(a[0], a[1], a[2]);
        float actionMagnitude = actVec.magnitude;
        float efficiencyPenalty = -efficiencyPenaltyWeight * penaltyCurve * actionMagnitude * DT;
        
        // 相对角速度惩罚 (两个刚体角速度的差值，分数越高惩罚越高)
        // Relative angular velocity penalty (difference between the two bodies' angular velocities, scaled by score)
        Vector3 relativeAngularVelocity = frontRb.angularVelocity - backRb.angularVelocity;
        float relativeAngularVelocityMagnitude = relativeAngularVelocity.magnitude;
        float relativeAngularVelocityPenalty = -relativeAngularVelocityPenaltyWeight * penaltyCurve * relativeAngularVelocityMagnitude * DT;
        
        AddReward(alignmentReward + efficiencyPenalty + relativeAngularVelocityPenalty);
        
        // 累加统计
        // Accumulate statistics
        cumulativeAlignmentRewardForStatOnly += alignmentReward;
        cumulativeEfficiencyPenaltyForStatOnly += efficiencyPenalty;
        cumulativeAngularVelocityPenaltyForStatOnly += relativeAngularVelocityPenalty;
        stepCount++;
        
        previousScore = currentScore;
        
        // 如果达到最大步数，打印统计并结束 Episode
        // If max steps reached, log statistics and end episode
        if (stepCount >= maxSteps)
        {
            Debug.Log($"Episode End | Steps: {stepCount} | " +
                      $"  Alignment: {cumulativeAlignmentRewardForStatOnly:F3} | " +
                      $"  EfficiencyPenalty: {cumulativeEfficiencyPenaltyForStatOnly:F3} | " +
                      $"  AngVelPenalty: {cumulativeAngularVelocityPenaltyForStatOnly:F3}");
            EndEpisode();
        }
    }
    
    private float CalculateScore()
    {
        // 腹部朝向与世界上方向的点积 (front.right 是朝上的, back.right 是朝下的)
        // Dot product of belly direction with world up direction (front.right is up, back.right is down)
        float frontAlignment = Vector3.Dot(frontBody.right, Vector3.up);
        float backAlignment = Vector3.Dot(-backBody.right, Vector3.up);
        
        // 转换到 [0, 1] 范围
        // Convert to [0, 1] range
        float frontNormalized = (frontAlignment + 1f) / 2f;
        float backNormalized = (backAlignment + 1f) / 2f;
        
        // 分别计算 exp, 然后取平均
        // Calculate exp for both, then take the minimum
        float frontScore = Mathf.Exp(alignmentExpScale * (frontNormalized - 1f));
        float backScore = Mathf.Exp(alignmentExpScale * (backNormalized - 1f));
        
        // 取二者的最差值, 避免 agent 使用一种 "身子折叠" 的局部极值 trick
        // Take the minimum of the two to avoid "body folding" local optimum trick
        float theMin = Mathf.Min(frontScore, backScore);
        return theMin;
    }
}