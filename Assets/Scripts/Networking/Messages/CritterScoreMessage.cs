using UnityEngine.Networking;

namespace Networking
{
    public class CritterScoreMessage : MessageBase
    {
        public int ID;
        public int DeathCount;
        public int KillCount;
        public float DamageTaken;
        public float DamageDealt;
    }

}