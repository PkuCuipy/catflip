using UnityEngine;

public class CatPhysicsSetup : MonoBehaviour
{
  [Header("Body References")]
  [SerializeField] private Rigidbody frontBody;
  [SerializeField] private Rigidbody backBody;
  [SerializeField] private ConfigurableJoint joint;

  [Header("Physics Parameters")]
  [SerializeField] private float maxAngularVelocity = 10f;
  [SerializeField] private float jointSpring = 2000f;
  [SerializeField] private float jointDamper = 1000f;
  [SerializeField] private float jointMaxForce = 500f;
  [SerializeField] private float mass = 2f;
  [SerializeField] private float drag = 3f;
  [SerializeField] private float angularDrag = 3f;


  private void Awake()
  {
    SetupPhysics();
  }

  private void SetupPhysics()
  {
    // 配置刚体物理属性
    // Configure rigidbody physics properties
    ConfigureRigidbody(frontBody);
    ConfigureRigidbody(backBody);

    // 配置关节
    // Configure joint
    ConfigureJoint();

    Debug.Log($"[CatPhysicsSetup] Physics configured for {gameObject.name}");
  }

  private void ConfigureRigidbody(Rigidbody rb)
  {
    if (rb == null)
    {
      Debug.LogError($"[CatPhysicsSetup] Missing Rigidbody reference on {gameObject.name}");
      return;
    }

    rb.mass = mass;
    rb.linearDamping = drag;
    rb.angularDamping = angularDrag;
    rb.useGravity = false;  // 关闭重力（自由落体等效） Disable gravity (free-fall equivalent)
    rb.maxAngularVelocity = maxAngularVelocity;
    rb.interpolation = RigidbodyInterpolation.None;  // 训练时不需要插值 (no interpolation needed during training)
    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;  // 性能优先，训练时使用离散碰撞检测 (Performance priority, use discrete collision detection during training)
  }

  private void ConfigureJoint()
  {
    if (joint == null)
    {
      Debug.LogError($"[CatPhysicsSetup] Missing ConfigurableJoint reference on {gameObject.name}");
      return;
    }

    // 配置弹簧
    // Configure spring
    var drive = joint.angularXDrive;
    drive.positionSpring = jointSpring;
    drive.positionDamper = jointDamper;
    drive.maximumForce = jointMaxForce;
    joint.angularXDrive = drive;
    joint.angularYZDrive = drive;

    // 确保关节模式正确
    // Ensure correct joint mode
    joint.rotationDriveMode = RotationDriveMode.XYAndZ;
  }
}