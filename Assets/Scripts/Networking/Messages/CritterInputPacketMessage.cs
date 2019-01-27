using System.Collections.Generic;
using UnityEngine.Networking;

namespace Networking
{
    public class CritterInputPacketMessage : MessageBase
    {
        public CritterInputPacket critterInputPacket;
        public float time;
    }

}