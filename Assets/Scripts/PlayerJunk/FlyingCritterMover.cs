using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlyingCritterMoverConfig
{
    public float MouseYawSensitivity = 1f;
    public float MousePitchSensitivity = 1f;
    public float KeyRollSensitivity = 1f;
    public float RollYawTrim = 0.5f;
    public float PitchClampDegrees = 45f;
    public float RollClampDegrees = 45f;

    public float FlapFrequency = 3f;
    public float Mass = 0.1f;
    public float DragMagic = 0.1f;
    public float LiftMagic = 0.01f;
    public float GlideMagic = 10f;
    public float FlapThrustToLiftRatioPressingW = .7f;
    public float FlapThrustToLiftRatioPressingNotPressingW = .3f;
    public float FlapPower = 5f;

    public float SizeOfShitRelativeToBird = .5f;
    public float ShitsPerMinute = 240f;

    public AttackKind attackKind;
}

public class FlyingCritterMover : ICritterMover
{
    readonly GameObject critter;
    readonly FlyingCritterMoverConfig config;

    float _yawDegress;
    float _pitchDegrees;
    float _rollDegrees;
    float timeOfLastBowelMovement = 0f;

    public FlyingCritterMover(GameObject critter, FlyingCritterMoverConfig config, IPlayerAudioManager audioManager)
    {
        this.critter = critter;
        this.config = config;

        CurrentState = STATE.FLYING;
        Cursor.lockState = CursorLockMode.Locked;
        rb = critter.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = config.Mass;
    }

    public GameObject GetHead()
    {
        throw new System.NotImplementedException();
    }

    public void TakeStateFromServer(CritterStatePacket state, bool setRotation = true)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateImmediate(CritterInputPacket packet)
    {
        // TODO not sure what to put in here
    }

    public CritterStatePacket UpdateTick(CritterInputPacket packet)
    {
        UpdateState();
        DoMove();

        return new CritterStatePacket
        {
            position = critter.transform.position, // TODO mmm this feels weird
            velocity = rb.velocity,
            headOrientation = critter.transform.rotation // TODO prob dont need this
        };
    }

    void DoMove()
    {
        var orientation = UpdateOrientation();
        rb.MoveRotation(orientation);

        var angleOfAttack = Mathf.Acos(Vector3.Dot(rb.velocity.normalized, critter.transform.forward.normalized)) * Mathf.Rad2Deg;

        float locForwardVel = critter.transform.InverseTransformDirection(rb.velocity).z;
        float drag = config.DragMagic * (Mathf.Pow(locForwardVel, 2) / 2) * angleOfAttack;

        var sign = (Quaternion.LookRotation(Vector3.forward, Vector3.up) * critter.transform.forward.normalized).y;
        var signedAngleOfAttack = angleOfAttack;
        if (sign < 0) signedAngleOfAttack *= -1;

        if (Input.GetKey(KeyCode.Space))
        {
            float flapForce = Mathf.Cos(Time.time * config.FlapFrequency) * config.FlapPower;
            flapForce = Mathf.Abs(flapForce);
            var flapThrustToLiftRatio = Input.GetKey(KeyCode.W) ? config.FlapThrustToLiftRatioPressingW : config.FlapThrustToLiftRatioPressingNotPressingW;
            rb.AddForce(critter.transform.forward * flapForce * flapThrustToLiftRatio);
            rb.AddForce(critter.transform.up * flapForce * (1 - flapThrustToLiftRatio));
        }

        var dragVector = rb.velocity * -drag;
        rb.AddForce(dragVector);

        float lift = (Mathf.Pow(locForwardVel, 2) * config.LiftMagic * signedAngleOfAttack) / 2;
        var liftVector = critter.transform.up * lift;
        rb.AddForce(liftVector);
        rb.AddForce(Physics.gravity, ForceMode.Acceleration);

        // janky, no science glide bs
        var fallVelocity = rb.velocity.y;
        var glideRatio = lift / drag;
        if (fallVelocity < 0 && glideRatio < 0)
        {
            var forwardGlide = fallVelocity * glideRatio * config.GlideMagic;
            rb.AddForce(critter.transform.forward * forwardGlide, ForceMode.VelocityChange);
        }

        bool readyForBM = Time.time - timeOfLastBowelMovement > config.ShitsPerMinute / 60;
        if (Input.GetKey(KeyCode.Mouse0) && readyForBM)
        {
            timeOfLastBowelMovement = Time.time;
            var shit = GameObject.Instantiate(Resources.Load("Prefabs/BirdShit") as GameObject);
            shit.transform.position = critter.transform.position;
            shit.transform.localScale = critter.transform.localScale * config.SizeOfShitRelativeToBird;
            var birdShit = shit.GetComponent<BirdShit>();
        }
    }

    void UpdateState() // todo this is buggy and sets to standing when close by not touching
    {
        RaycastHit hit;
        Ray downRay = new Ray(critter.transform.position, -Vector3.up);
        bool closeToSomethingToStandOn = Physics.Raycast(downRay, out hit) && hit.distance < 0.7f;
        CurrentState = closeToSomethingToStandOn ? STATE.STANDING : STATE.FLYING;
    }

    enum STATE { FLYING, STANDING }

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

    Rigidbody rb;

    void ResetRB() // todo this maybe causing problems
    {
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    Quaternion UpdateOrientation()
    {
        var mouseX = Input.GetAxis("Mouse X") * config.KeyRollSensitivity;
        _rollDegrees -= mouseX;
        _rollDegrees = Mathf.Clamp(_rollDegrees, -config.RollClampDegrees, config.RollClampDegrees);

        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw -= config.MouseYawSensitivity;
        if (Input.GetKey(KeyCode.D)) yaw += config.MouseYawSensitivity;
        _yawDegress += yaw * config.MouseYawSensitivity;
        _yawDegress += mouseX * config.RollYawTrim;

        _pitchDegrees -= Input.GetAxis("Mouse Y") * config.MousePitchSensitivity;
        _pitchDegrees = Mathf.Clamp(_pitchDegrees, -config.PitchClampDegrees, config.PitchClampDegrees);

        _rollDegrees = CurrentState == STATE.STANDING ? 0f : _rollDegrees; // todo this is dumb
        return Quaternion.Euler(_pitchDegrees, _yawDegress, _rollDegrees);
    }
}
