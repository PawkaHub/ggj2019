using Game;
using System;
using UnityEngine;

[Serializable]
public class CritterMoverConfig
{
    public float extraHeight;
    public float suspensionRadiusRatio;

    public float maxSpeed;
    public float accelLag;
    public float autoDecel;
    public float jumpVelY;
    public float gravityMult;

    public AttackKind attackKind;
}

public interface ICritterMover
{
    void UpdateImmediate(CritterInputPacket packet);
    void TakeStateFromServer(CritterStatePacket state, bool setRotation = true);
    CritterStatePacket UpdateTick(CritterInputPacket packet);
    GameObject GetHead();
    bool Collided { get; set; }
}

public class CritterMover : ICritterMover
{
    readonly GameObject Head;
    public readonly GameObject NeckBone;

    readonly GameObject critter;
    readonly CritterMoverConfig config;

    readonly GameObject childCamera;
    readonly Rigidbody rb;
    readonly float radius;
    readonly float suspensionRadius;
    readonly IAttackLauncher launcher;

    int grounded;
    float cameraBobT;

    public bool Collided { get; set; }

    public GameObject GetHead()
    {
        return Head;
    }

    private Transform FindChildByName(string partial, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(partial.ToLower()))
                return child;
        }

        foreach (Transform child in parent)
        {
            var trans = FindChildByName(partial, child);
            if (trans != null)
                return trans;
        }

        return null;
    }

    IPlayerAudioManager audioManager;

    public CritterMover(GameObject critter, CritterMoverConfig config, IPlayerAudioManager audioManager)
    {
        this.critter = critter;
        this.config = config;

        rb = critter.GetComponent<Rigidbody>();
        radius = critter.GetComponent<SphereCollider>().radius;
        Head = critter.transform.Find("Head").gameObject;
        NeckBone = FindChildByName("neck", critter.transform).gameObject;
        childCamera = critter.GetComponentInChildren<Camera>().gameObject;
        cameraBobT = 0;
        suspensionRadius = config.suspensionRadiusRatio * radius;
        this.audioManager = audioManager;
        launcher = AttackLauncherFactory.Create(config.attackKind, audioManager, critter.GetComponentInParent<Player>());
    }

    public void UpdateImmediate(CritterInputPacket packet)
    {
        cameraBobT += rb.velocity.WithY(0).magnitude * 0.05f;
        Head.transform.rotation = packet.headOrientation;
        childCamera.transform.localPosition = 0.02f * cameraBob(cameraBobT);
    }

    public CritterStatePacket UpdateTick(CritterInputPacket packet)
    {
        bool walking = false;

        rb.velocity += Physics.gravity * config.gravityMult * Time.fixedDeltaTime;

        var flatVel = rb.velocity.WithY(0);
        var fwd = Head.transform.forward.WithY(0).normalized;
        var right = Head.transform.right.WithY(0).normalized;

        var desiredVel = Vector3.zero;

        if (packet.forward) {
            desiredVel += fwd;
            walking = true;
        } else if (packet.backward) {
            desiredVel -= fwd;
            walking = true;
        }

        if (packet.rightward) {
            desiredVel += right;
            walking = true;
        } else if (packet.leftward) {
            desiredVel -= right;
            walking = true;
        }

        desiredVel = desiredVel.normalized * config.maxSpeed;

        if (walking) {
            flatVel = Vector3.Lerp(flatVel, desiredVel, 1f / config.accelLag);
        } else {
            flatVel *= config.autoDecel;
        }

        if (flatVel.sqrMagnitude > config.maxSpeed * config.maxSpeed) {
            flatVel = flatVel.normalized * config.maxSpeed;
        }

        rb.velocity = flatVel + Vector3.up * rb.velocity.y;

        if (packet.jump && grounded > 0) {
            grounded = 0;
            rb.velocity = rb.velocity.WithY(config.jumpVelY);
            audioManager.PlayJumpSound();
        }

        var newPosition = rb.position + rb.velocity * Time.fixedDeltaTime;

        if (grounded > 0) grounded--;

        var hits = Physics.SphereCastAll(newPosition, suspensionRadius, Vector3.down, config.extraHeight, LayerMask.GetMask("Default"));
        var hitInfo = findImportantHit(hits);
        if (hitInfo.HasValue)
        {
            var fixedCastPos = newPosition + Vector3.down * hitInfo.Value.distance;
            newPosition = fixedCastPos + Vector3.up * config.extraHeight;
            rb.velocity = rb.velocity.WithY(0);
            rb.velocity += (newPosition - rb.position) * 10f;

            grounded = 2;
        }

        launcher.Update(packet.shoot, critter.transform.position, packet.headOrientation * Vector3.forward);

        return new CritterStatePacket {
            position = newPosition,
            velocity = rb.velocity,
            headOrientation = Head.transform.rotation
        };
    }

    public void TakeStateFromServer(CritterStatePacket state, bool setHeadOrientation = true)
    {
        rb.MovePosition(state.position);
        rb.velocity = state.velocity;

        if (setHeadOrientation)
        {
            Head.transform.rotation = state.headOrientation;
        }
    }

    static Vector2 cameraBob(float t)
    {
        var x = Mathf.Sin(t);
        var y = -x * x;
        return new Vector2(x, y);
    }

    RaycastHit? findImportantHit(RaycastHit[] hits)
    {
        foreach (var hit in hits) {
            if (hit.point == Vector3.zero) continue;

            if (hit.point.y < critter.transform.position.y - radius) {
                return hit;
            }
        }
        return null;
    }

    static public void DrawGizmos(Vector3 critterPosition, float critterRadius, CritterMoverConfig config)
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(critterPosition + Vector3.down * config.extraHeight, critterRadius * config.suspensionRadiusRatio);
    }
}
