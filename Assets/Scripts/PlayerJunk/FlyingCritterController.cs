using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO probs dry this up since its mostly repeated CritterController
public class FlyingCritterController : MonoBehaviour, ICritterController
{
    [SerializeField] FlyingCritterMoverConfig critterConfig;
    [SerializeField] protected CritterAudioManager audioManager;

    public IInputGrabber localInputGrabber { get; set; }
    public ICritterMover Mover { get; protected set; }

    public bool IsServer { get; set; }
    public CritterInputPacket? InputPacketOveride { get; set; }

    public event Action<CritterStatePacket> OnCritterStatePacket;
    public event Action<CritterInputPacket> OnCritterInputPacket;

    protected virtual void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //inputGrabber = new CritterInputGrabber(mouseSensitivity);
        Mover = new FlyingCritterMover(gameObject, critterConfig, audioManager);
    }

    private void Update()
    {
        if (localInputGrabber != null)
        {
            // Always use the local input grabber to drive the mover with UpdateImmediate
            Mover.UpdateImmediate(localInputGrabber.UpdateImmediate());
        }
        else if (InputPacketOveride != null)
        {
            Mover.UpdateImmediate(InputPacketOveride.Value);
        }
    }

    public void UpdateViaCritterStatePacket(CritterStatePacket critterStatePacket)
    {
        Mover.TakeStateFromServer(critterStatePacket, localInputGrabber == null);
    }

    private void FixedUpdate()
    {
        CritterInputPacket? inputPacket = null;
        if (localInputGrabber != null)
        {
            inputPacket = localInputGrabber.UpdateTick();

            OnCritterInputPacket?.Invoke(inputPacket.Value);
        }

        if (IsServer && inputPacket != null)
        {
            var statePacket = Mover.UpdateTick(inputPacket.Value);
            OnCritterStatePacket?.Invoke(statePacket);
        }
        else
        {
            if (InputPacketOveride != null)
            {
                var statePacket = Mover.UpdateTick(InputPacketOveride.Value);
                if (IsServer)
                {
                    OnCritterStatePacket?.Invoke(statePacket);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = -collision.relativeVelocity;
        Mover.Collided = true;
        audioManager.PlayJumpSound();
    }

    private void OnCollisionExit(Collision collision)
    {
        Mover.Collided = false;
    }
}
