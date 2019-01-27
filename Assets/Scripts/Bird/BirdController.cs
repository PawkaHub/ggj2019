using UnityEngine;

// obsolete but keepin it around because....
public class BirdController : MonoBehaviour
{
    [SerializeField] readonly float MouseYawSensitivity = 1f;
    [SerializeField] readonly float MousePitchSensitivity = 1f;
    [SerializeField] readonly float KeyRollSensitivity = 1f;
    [SerializeField] readonly float RollYawTrim = 0.5f;
    [SerializeField] readonly float PitchClampDegrees = 45f;
    [SerializeField] readonly float RollClampDegrees = 45f;

    [SerializeField] readonly float FlapFrequency = 3f;
    [SerializeField] readonly float Mass = 0.1f;
    [SerializeField] readonly float DragMagic = 0.1f;
    [SerializeField] readonly float LiftMagic = 0.01f;
    [SerializeField] readonly float GlideMagic = 10f;
    [SerializeField] readonly float FlapThrustToLiftRatioPressingW = .7f;
    [SerializeField] readonly float FlapThrustToLiftRatioPressingNotPressingW = .3f;
    [SerializeField] readonly float FlapPower = 5f;

    [SerializeField] readonly float SizeOfShitRelativeToBird = .5f;
    [SerializeField] readonly float ShitsPerMinute = 240f;

    Rigidbody rb;
    float _yawDegress;
    float _pitchDegrees;
    float _rollDegrees;

    float timeOfLastBowelMovement = 0f;

    enum STATE { FLYING, STANDING}

    STATE currentState;
    STATE CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (currentState != value)
            {
                currentState = value;

                if (currentState == STATE.STANDING)
                {
                    ResetRB();
                }
            }
        }
    }

    void Awake()
    {
        CurrentState = STATE.STANDING;
        Cursor.lockState = CursorLockMode.Locked;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = Mass;
    }

    void FixedUpdate()
    {
        UpdateState();
        DoMove();
    }

    void UpdateState() // todo this is buggy and sets to standing when close by not touching
    {
        RaycastHit hit;
        Ray downRay = new Ray(transform.position, -Vector3.up);
        bool closeToSomethingToStandOn = Physics.Raycast(downRay, out hit) && hit.distance < 0.7f;
        CurrentState = closeToSomethingToStandOn ? STATE.STANDING : STATE.FLYING;
    }

    void ResetRB() // todo this maybe causing problems
    {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    void DoMove()
    {
        var orientation = UpdateOrientation();
        rb.MoveRotation(orientation);

        var angleOfAttack = Mathf.Acos(Vector3.Dot(rb.velocity.normalized, transform.forward.normalized)) * Mathf.Rad2Deg;

        float locForwardVel = transform.InverseTransformDirection(rb.velocity).z;
        float drag = DragMagic * (Mathf.Pow(locForwardVel, 2) / 2) * angleOfAttack;

        var sign = (Quaternion.LookRotation(Vector3.forward, Vector3.up) * transform.forward.normalized).y;
        var signedAngleOfAttack = angleOfAttack;
        if (sign < 0) signedAngleOfAttack *= -1;

        if (Input.GetKey(KeyCode.Space))
        {
            float flapForce = Mathf.Cos(Time.time * FlapFrequency) * FlapPower;
            flapForce = Mathf.Abs(flapForce);
            var flapThrustToLiftRatio = Input.GetKey(KeyCode.W) ? FlapThrustToLiftRatioPressingW : FlapThrustToLiftRatioPressingNotPressingW;
            rb.AddForce(transform.forward * flapForce * flapThrustToLiftRatio);
            rb.AddForce(transform.up * flapForce * (1 - flapThrustToLiftRatio));
        }

        var dragVector = rb.velocity * -drag;
        rb.AddForce(dragVector);

        float lift = (Mathf.Pow(locForwardVel, 2) * LiftMagic * signedAngleOfAttack) / 2;
        var liftVector = transform.up * lift;
        rb.AddForce(liftVector);
        rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        // janky, no science glide bs
        var fallVelocity = rb.velocity.y;
        var glideRatio = lift / drag;
        if (fallVelocity < 0 && glideRatio < 0)
        {
            var forwardGlide = fallVelocity * glideRatio * GlideMagic;
            rb.AddForce(transform.forward * forwardGlide, ForceMode.VelocityChange);
        }

        bool readyForBM = Time.time - timeOfLastBowelMovement > ShitsPerMinute / 60;
        if (Input.GetKey(KeyCode.Mouse0) && readyForBM)
        {
            timeOfLastBowelMovement = Time.time;
            var shit = Instantiate(Resources.Load("Prefabs/BirdShit") as GameObject);
            shit.transform.position = transform.position;
            shit.transform.localScale = transform.localScale * SizeOfShitRelativeToBird;
            var birdShit = shit.GetComponent<BirdShit>();
        }
    }

    Quaternion UpdateOrientation()
    {
        var mouseX = Input.GetAxis("Mouse X") * KeyRollSensitivity;
        _rollDegrees -= mouseX;
        _rollDegrees = Mathf.Clamp(_rollDegrees, -RollClampDegrees, RollClampDegrees);

        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw -= MouseYawSensitivity;
        if (Input.GetKey(KeyCode.D)) yaw += MouseYawSensitivity;
        _yawDegress += yaw * MouseYawSensitivity;
        _yawDegress += mouseX * RollYawTrim;

        _pitchDegrees -= Input.GetAxis("Mouse Y") * MousePitchSensitivity;
        _pitchDegrees = Mathf.Clamp(_pitchDegrees, -PitchClampDegrees, PitchClampDegrees);

        _rollDegrees = CurrentState == STATE.STANDING ? 0f : _rollDegrees; // todo this is dumb
        return Quaternion.Euler(_pitchDegrees, _yawDegress, _rollDegrees);
    }
}