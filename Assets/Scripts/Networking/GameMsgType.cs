using UnityEngine.Networking;

namespace Networking
{
    public class GameMsgType
    {
        public const short UpdateActivePlayers = MsgType.Highest + 1;
        public const short PlayerDisconnect = MsgType.Highest + 2;
        public const short Effects = MsgType.Highest + 3;
        public const short UpdateCritterState = MsgType.Highest + 4;
        public const short UpdateCritterInput = MsgType.Highest + 5;
        public const short UpdateCritterScores = MsgType.Highest + 6;
    }

}