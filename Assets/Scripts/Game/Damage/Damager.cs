using UnityEngine;

namespace Game
{
    public class Damager
    {
        public GameObject GameObject { get; private set; }
        public Player Player;

        public Damager(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public Damager(Player player) : this(player.gameObject)
        {
            this.Player = player;
        }

        public string GetName()
        {
            if (Player != null) return Player.NetworkPlayer.Name;

            return GameObject.name;
        }
    }
}