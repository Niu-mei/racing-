using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class AIController : MonoBehaviour
{
    public enum DriveStyle
    {
        Conservative,
        Balanced,
        Aggressive
    }

    [Header("AI Setup")]
    public string carName = "AI";
    public DriveStyle driveStyle = DriveStyle.Balanced;
    public RaceManager raceManager;

    [Header("Player")]
    public Transform playerCar;
    public float catchDistance = 20f;
    public float safeDistanceToPlayer = 8f;

    [Header("Waypoint Path")]
    public Transform[] waypoints;
    public float waypointReachDistance = 6f;

    [Header("Core Movement")]
    public float maxSpeed = 30f;
    public float accelerateForce = 20f;
    public float brakingForce = 16f;
    public float turnSpeed = 2.8f;

    [Header("Cornering")]
    public float cornerSpeedFactor = 0.58f;
    public float obstacleCheckDistance = 12f;
    public float obstacleRadius = 1.2f;

    [Header("Overtake")]
    public float overtakingBias = 1.4f;
    public float tangentOffset = 5f;

    [Header("Traffic")]
    public float queueDistance = 8f;
    public float queueForce = 1.2f;

    [Header("Impact FX")]
    public AudioSource impactAudio;
    public ParticleSystem impactParticles;
    public LayerMask obstacleMask;

    [Header("UI")]
    public Text lapText;
    public Text rankText;

    public int currentRank = 1;
    public float lapProgress = 1f;

    private Rigidbody rb;
    private int waypointIndex = 0;
    private float collisionCooldown = 0f;
    private float progress = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (raceManager == null)
            raceManager = FindObjectOfType<RaceManager>();

        if (playerCar == null)
            playerCar = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (raceManager != null)
            raceManager.RegisterAI(this);
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            waypointIndex = 0;
    }

    private void Update()
    {
        UpdateLapProgress();
        UpdateUI();
        if (collisionCooldown > 0f)
            collisionCooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (rb == null || waypoints == null || waypoints.Length == 0)
            return;

        Vector3 targetPoint = CalculateTargetPoint();

        Vector3 avoidanceVector = GetAvoidanceVector();
        Vector3 trafficVector = GetTrafficAvoidance();

        Vector3 targetDirection = (targetPoint - transform.position).normalized;
        targetDirection.y = 0f;

        if (avoidanceVector != Vector3.zero)
            targetDirection = (targetDirection + avoidanceVector.normalized * 2f).normalized;

        if (trafficVector != Vector3.zero)
            targetDirection = (targetDirection + trafficVector.normalized * 1.5f).normalized;

        Quaternion targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);

        float targetSpeed = GetMaxSpeedByStyle();
        float speedFactor = 1f;

        if (IsNearCorner())
            speedFactor = cornerSpeedFactor;

        if (playerCar != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerCar.position);
            bool playerAhead = Vector3.Dot(transform.forward, playerCar.position - transform.position) > 0.1f;

            if (playerAhead && distToPlayer < catchDistance)
            {
                targetSpeed *= 1.08f;
            }

            if (!playerAhead && distToPlayer < safeDistanceToPlayer)
            {
                targetSpeed *= 0.6f;
            }
        }

        if (rb.velocity.magnitude < targetSpeed * speedFactor)
        {
            rb.velocity += transform.forward * accelerateForce * Time.fixedDeltaTime;
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, transform.forward * targetSpeed * speedFactor, 0.12f);
        }

        // 自动修正撞墙
        if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 1.2f, transform.forward, out RaycastHit wallHit, 4f, obstacleMask))
        {
            Vector3 correction = Vector3.Cross(Vector3.up, wallHit.normal).normalized;
            if (correction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(correction, Vector3.up), 0.2f);
                rb.velocity += correction * 4f;
            }
        }

        // 和玩家碰撞时后退和晃动
        if (playerCar != null)
        {
            float dist = Vector3.Distance(transform.position, playerCar.position);
            if (dist < 3.5f && collisionCooldown <= 0f)
            {
                Vector3 pushDir = (transform.position - playerCar.position).normalized;
                rb.velocity += pushDir * 6f;

                Vector3 shake = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * 2f;
                rb.velocity += shake;

                collisionCooldown = 0.7f;

                TriggerImpactFX();
            }
        }

        // 限制最大速度
        if (rb.velocity.magnitude > targetSpeed + 8f)
            rb.velocity = rb.velocity.normalized * (targetSpeed + 8f);

        // 避免上下漂浮
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    public float GetApproxProgress()
    {
        return lapProgress + progress;
    }

    private Vector3 CalculateTargetPoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return transform.position;

        if (waypointIndex >= waypoints.Length)
            waypointIndex = 0;

        if (Vector3.Distance(transform.position, waypoints[waypointIndex].position) < waypointReachDistance)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        Vector3 target = waypoints[waypointIndex].position;

        if (playerCar != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerCar.position);
            bool playerAhead = Vector3.Dot(transform.forward, playerCar.position - transform.position) > 0.1f;

            if (playerAhead && distToPlayer < catchDistance)
            {
                Vector3 dirToPlayer = (playerCar.position - transform.position).normalized;
                Vector3 tangent = Vector3.Cross(Vector3.up, dirToPlayer).normalized;
                return playerCar.position + tangent * tangentOffset + playerCar.forward * 4f;
            }
        }

        return target;
    }

    private Vector3 GetAvoidanceVector()
    {
        Vector3 avoidance = Vector3.zero;

        Vector3[] dirs = new Vector3[]
        {
            transform.forward,
            transform.forward + transform.right * 0.6f,
            transform.forward - transform.right * 0.6f,
            transform.right * 1.2f,
            -transform.right * 1.2f
        };

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = dirs[i].normalized;
            Vector3 origin = transform.position + Vector3.up * 0.5f + dir * 1.5f;

            if (Physics.SphereCast(origin, obstacleRadius, dir, out RaycastHit hit, obstacleCheckDistance, obstacleMask))
            {
                Vector3 away = (transform.position - hit.point).normalized;
                away.y = 0f;
                avoidance += away * 1.6f;
            }
        }

        return avoidance;
    }

    private Vector3 GetTrafficAvoidance()
    {
        Vector3 result = Vector3.zero;

        if (raceManager == null)
            return result;

        foreach (var ai in raceManager.aiCars)
        {
            if (ai == null || ai == this)
                continue;

            float dist = Vector3.Distance(transform.position, ai.transform.position);
            if (dist > 0f && dist < queueDistance)
            {
                Vector3 dir = (transform.position - ai.transform.position).normalized;
                dir.y = 0f;
                result += dir * queueForce;
            }
        }

        return result;
    }

    private bool IsNearCorner()
    {
        Vector3[] dirs = new Vector3[]
        {
            transform.forward,
            transform.forward + transform.right * 0.6f,
            transform.forward - transform.right * 0.6f
        };

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 origin = transform.position + Vector3.up * 0.4f + dirs[i] * 2.5f;
            if (Physics.SphereCast(origin, 0.9f, dirs[i], out RaycastHit hit, obstacleCheckDistance, obstacleMask))
                return true;
        }

        return false;
    }

    private void UpdateLapProgress()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (waypointIndex >= waypoints.Length)
            waypointIndex = 0;

        float distToCurrent = Vector3.Distance(transform.position, waypoints[waypointIndex].position);
        float segLen = Vector3.Distance(
            waypoints[waypointIndex].position,
            waypoints[(waypointIndex + 1) % waypoints.Length].position
        );

        if (segLen > 0.001f)
            progress = 1f - Mathf.Clamp01(distToCurrent / segLen);

        // 简易 lap
        lapProgress = (waypointIndex + progress) / waypoints.Length;
    }

    private float GetMaxSpeedByStyle()
    {
        switch (driveStyle)
        {
            case DriveStyle.Conservative:
                return maxSpeed * 0.8f;
            case DriveStyle.Balanced:
                return maxSpeed;
            case DriveStyle.Aggressive:
                return maxSpeed * 1.15f;
            default:
                return maxSpeed;
        }
    }

    private void UpdateUI()
    {
        if (lapText != null)
            lapText.text = "Lap: " + Mathf.RoundToInt(Mathf.Max(1f, lapProgress * 10f));

        if (rankText != null)
            rankText.text = "Rank: " + currentRank;
    }

    private void TriggerImpactFX()
    {
        if (impactAudio != null && !impactAudio.isPlaying)
            impactAudio.Play();

        if (impactParticles != null)
            impactParticles.Play();
    }
}
