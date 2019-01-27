using System.Collections.Generic;
using UnityEngine.Networking;

namespace Networking
{
    public class CritterInputPacketMessage : MessageBase
    {
        public int ID;
        public CritterInputPacket critterInputPacket;
    }

}