using System.Collections.Generic;
using UnityEngine.Networking;

namespace Networking
{
    public class CritterStatePacketMessage : MessageBase
    {
        public CritterStatePacket critterStatePacket;
        internal int ID;
        public float Time;
    }

}