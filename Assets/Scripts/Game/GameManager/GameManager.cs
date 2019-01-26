﻿using UnityEngine;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        private Player[] players;

        public void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            FindPlayers();
            for (int i = 0; i < players.Length; i++)
            {
                InitializePlayer(players[i]);
            }
        }

        private void FindPlayers()
        {
            players = FindObjectsOfType<Player>();
        }

        private void InitializePlayer(Player player)
        {
            player.Health.OnModified += OnHealthModified;
            player.Health.OnDeath += OnDeath;
        }

        private void OnDeath(object sender, HealthEventArgs e)
        {
            Debug.LogErrorFormat("Player {0} ate shit and died.");
        }

        private void OnHealthModified(object sender, HealthEventArgs e)
        {
            Debug.LogErrorFormat("Player {0} ate shit and took some fucking damage.");
        }
    }
}