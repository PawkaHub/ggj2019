using UnityEngine;
using NetworkPlayer = Networking.NetworkPlayer;

namespace Game
{
    public class Player : MonoBehaviour
    {
        public NetworkPlayer NetworkPlayer { get; set; }
        public Health Health { get; private set; }
        public PlayerScore Score { get; private set; }
        public ICritterController CritterController { get; private set; }
        public Effects Effects { get; private set; }

        public void Awake()
        {
            Health = GetComponentInChildren<Health>(true);
            Score = GetComponentInChildren<PlayerScore>(true);
            CritterController = GetComponentInChildren<ICritterController>(true);
            Effects = GetComponentInChildren<Effects>(true);
            Effects.Player = this;
        }

        internal void Initialize(NetworkPlayer netPlayer, IInputGrabber localInputGrabber, bool isServer)
        {
            NetworkPlayer = netPlayer;
            name = string.Format("[Player] {0}", netPlayer.Name);
            netPlayer.Player = this;

            Debug.Log("Player Initialized: " + name + " isSelf" + netPlayer.IsSelf);

            GetComponent<CharacterInstantiator>().Create();
            CritterController = GetComponentInChildren<ICritterController>(true);
            Effects = GetComponentInChildren<Effects>(true);

            CritterController.IsServer = isServer;
            CritterController.OnCritterStatePacket += netPlayer.ForwardCritterStatePacket;
            CritterController.localInputGrabber = (netPlayer.IsSelf) ? localInputGrabber : null;

            if (netPlayer.IsSelf)
            {
                if (isServer)
                {
                    CritterController.OnCritterInputPacket += (p) =>
                    {
                        netPlayer.ServerConnection.SendUpdateCritterInput(netPlayer, p);
                    };
                }
                else
                {
                    CritterController.OnCritterInputPacket += NetworkPlayer.PostCritterInputPacket;
                }
            }
        }

        internal void SetInputPacket(CritterInputPacket critterInputPacket)
        {
            CritterController.InputPacketOveride = critterInputPacket;
        }
    }
}