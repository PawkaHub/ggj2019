using System;
using System.Collections;
using System.Collections.Generic;
using Game;
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
    readonly IAttackLauncher launcher;

    float _yawDegress;
    float _pitchDegrees;
    float _rollDegrees;
    bool hasHadAnyInput;

    IPlayerAudioManager audioManager;

    public FlyingCritterMover(GameObject critter, FlyingCritterMoverConfig config, IPlayerAudioManager audioManager)
    {
        this.critter = critter;
        this.config = config;
        this.audioManager = audioManager;

        hasHadAnyInput = false;
        currentState = STATE.FLYING;
        Cursor.lockState = CursorLockMode.Locked;
        rb = critter.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = config.Mass;
        launcher = AttackLauncherFactory.Create(config.attackKind, audioManager, critter.GetComponentInParent<Player>());
    }

    public GameObject GetHead()
    {
        return null;
    }

    public void TakeStateFromServer(CritterStatePacket state, bool setRotation = true)
    {
        rb.MovePosition(state.position);
        rb.velocity = state.velocity;
    }

    public void UpdateImmediate(CritterInputPacket packet)
    {
        hasHadAnyInput |= packet.forward || packet.jump;
        // TODO not sure what to put in here
    }

    public CritterStatePacket UpdateTick(CritterInputPacket packet)
    {
        hasHadAnyInput |= packet.forward || packet.jump;
        UpdateState();
        DoMove(packet);
        TryPoop(packet);

        return new CritterStatePacket
        {
            position = critter.transform.position, // TODO mmm this feels weird
            velocity = rb.velocity,
            headOrientation = critter.transform.rotation // TODO prob dont need this
        };
    }

    void TryPoop(CritterInputPacket packet)
    {
        launcher.Update(packet.shoot, critter.transform.position, -critter.transform.up);
    }

    

    void DoMove(CritterInputPacket packet)
    {
        var orientation = UpdateOrientation(packet);
        rb.MoveRotation(orientation);

        var angleOfAttack = Mathf.Acos(Vector3.Dot(rb.velocity.normalized, critter.transform.forward.normalized)) * Mathf.Rad2Deg;

        float locForwardVel = critter.transform.InverseTransformDirection(rb.velocity).z;
        float drag = config.DragMagic * (Mathf.Pow(locForwardVel, 2) / 2) * angleOfAttack;

        var sign = (Quaternion.LookRotation(Vector3.forward, Vector3.up) * critter.transform.forward.normalized).y;
        var signedAngleOfAttack = angleOfAttack;
        if (sign < 0) signedAngleOfAttack *= -1;

        if (packet.jump)
        {
            float flapForce = Mathf.Cos(Time.time * config.FlapFrequency) * config.FlapPower;
            flapForce = Mathf.Abs(flapForce);
            var flapThrustToLiftRatio = config.FlapThrustToLiftRatioPressingNotPressingW;
            if (currentState == STATE.FLYING)
            {
                flapThrustToLiftRatio = packet.forward ? config.FlapThrustToLiftRatioPressingW : config.FlapThrustToLiftRatioPressingNotPressingW;
            }
            else
            {
                audioManager.PlayJumpSound();
            }
            rb.AddForce(critter.transform.forward * flapForce * flapThrustToLiftRatio);
            rb.AddForce(critter.transform.up * flapForce * (1 - flapThrustToLiftRatio));
        }

        var dragVector = rb.velocity * -drag;
        if (Vector3.Magnitude(dragVector) < Mathf.Infinity)
        {
            rb.AddForce(dragVector);
        }

        float lift = (Mathf.Pow(locForwardVel, 2) * config.LiftMagic * signedAngleOfAttack) / 2;
        var liftVector = critter.transform.up * lift;
        rb.AddForce(liftVector);

        if (hasHadAnyInput)
        {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }

        // janky, no science glide bs
        var fallVelocity = rb.velocity.y;
        var glideRatio = lift / drag;
        if (!Collided && fallVelocity < 0 && glideRatio < 0)
        {
            var forwardGlide = fallVelocity * glideRatio * config.GlideMagic;
            rb.AddForce(critter.transform.forward * forwardGlide, ForceMode.VelocityChange);
        }
    }

    public void UpdateState() // todo this is buggy and sets to standing when close by not touching
    {
        RaycastHit hit;
        Ray downRay = new Ray(critter.transform.position, -Vector3.up);
        bool closeToSomethingToStandOn = Physics.Raycast(downRay, out hit) && hit.distance < 0.7f;
        currentState = closeToSomethingToStandOn ? STATE.STANDING : STATE.FLYING;
    }

    enum STATE { FLYING, STANDING }

    STATE currentState { get; set; }

    public bool Collided { get; set; }

    Rigidbody rb;

    Quaternion UpdateOrientation(CritterInputPacket packet)
    {
        var mouseX = packet.MouseX * config.KeyRollSensitivity;
        _rollDegrees -= mouseX;
        _rollDegrees = Mathf.Clamp(_rollDegrees, -config.RollClampDegrees, config.RollClampDegrees);

        float yaw = 0f;
        if (packet.leftward) yaw -= config.MouseYawSensitivity;
        if (packet.rightward) yaw += config.MouseYawSensitivity;
        _yawDegress += yaw * config.MouseYawSensitivity;
        _yawDegress += mouseX * config.RollYawTrim;

        _pitchDegrees -= packet.MouseY * config.MousePitchSensitivity;
        _pitchDegrees = Mathf.Clamp(_pitchDegrees, -config.PitchClampDegrees, config.PitchClampDegrees);

        return Quaternion.Euler(_pitchDegrees, _yawDegress, _rollDegrees);
    }

}
