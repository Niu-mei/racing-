using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Reference")]
    public Rigidbody rb;

    [Header("Wheels")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    [Header("Driving")]
    public float motorForce = 1500f;
    public float brakeForce = 2500f;
    public float maxSpeed = 42f;
    public float reverseSpeed = 12f;
    public float maxSteerAngle = 28f;
    public float steerSpeed = 8f;

    [Header("Drift")]
    public float grip = 0.9f;
    public float driftGrip = 0.55f;

    [Header("Misc")]
    public Transform centerOfMass;

    private float throttleInput;
    private float steeringInput;
    private float brakeInput;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (centerOfMass != null)
            rb.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        throttleInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        float targetSteer = steeringInput * maxSteerAngle;

        frontLeftWheel.steerAngle = Mathf.Lerp(frontLeftWheel.steerAngle, targetSteer, steerSpeed * Time.deltaTime);
        frontRightWheel.steerAngle = Mathf.Lerp(frontRightWheel.steerAngle, targetSteer, steerSpeed * Time.deltaTime);

        float motor = throttleInput * motorForce;
        frontLeftWheel.motorTorque = motor;
        frontRightWheel.motorTorque = motor;

        float brake = brakeInput * brakeForce;
        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        float lateralVel = localVel.x;

        float currentGrip = Input.GetKey(KeyCode.LeftShift) ? driftGrip : grip;

        localVel.x = Mathf.Lerp(localVel.x, 0f, Time.fixedDeltaTime * (1f - currentGrip) * 12f);

        rb.velocity = transform.TransformDirection(localVel);
    }

    public float CurrentSpeedKmh()
    {
        return rb.velocity.magnitude * 3.6f;
    }
}
