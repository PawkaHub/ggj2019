using System;
using UnityEngine;

public interface ICritterController
{
    ICritterMover Mover { get; }
    bool IsServer { get; set; }
    event Action<CritterStatePacket> OnCritterStatePacket;
    event Action<CritterInputPacket> OnCritterInputPacket;
    IInputGrabber localInputGrabber { get; set; }
    CritterInputPacket? InputPacketOveride { get; set; }
    void UpdateViaCritterStatePacket(CritterStatePacket critterStatePacket);
    Transform transform { get; }
}

public class CritterController : MonoBehaviour, ICritterController
{
    [SerializeField] CritterMoverConfig critterConfig;
    [SerializeField] protected CritterAudioManager audioManager;

    public IInputGrabber localInputGrabber { get; set; }
    public ICritterMover Mover { get; private set; }

    public bool IsServer { get; set; }
    public CritterInputPacket? InputPacketOveride { get; set; }

    public event Action<CritterStatePacket> OnCritterStatePacket;
    public event Action<CritterInputPacket> OnCritterInputPacket;

    protected virtual void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //inputGrabber = new CritterInputGrabber(mouseSensitivity);
        Mover = new CritterMover(gameObject, critterConfig, audioManager);
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

    private void OnDrawGizmosSelected()
    {
        CritterMover.DrawGizmos(transform.position, GetComponent<SphereCollider>().radius, critterConfig);
    }
}
