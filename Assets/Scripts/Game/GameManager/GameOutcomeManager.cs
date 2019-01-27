using System;
using System.Linq;
using Networking;
using UnityEngine;

namespace Game
{
    public class GameOutcomeManager : MonoBehaviour
    {
        public int KillsToWin = 10;

        public event EventHandler<GameOutcomeEventArgs> OnGameEnded;

        private GameManager GameManager { get; set; }

        public void Start()
        {
            GameManager = GetComponent<GameManager>();
            GameManager.OnInitialized += Init;
        }

        private void Init(object sender, EventArgs e)
        {
            GameManager.ConnectionManager.OnPlayerConnect += OnConnect;
            GameManager.ConnectionManager.OnPlayerDisconnect += OnDisconnect;
        }

        private void OnDisconnect(Networking.NetworkPlayer obj)
        {
            DeInitializePlayer(obj.Player);
        }

        private void OnConnect(Networking.NetworkPlayer obj)
        {
            InitializePlayer(obj.Player);
        }

        public void Update()
        {
            if (!GameManager.Initialized)
                return;
            
            CheckGameOutcome();
        }

        private void InitializePlayer(Player player)
        {
            player.Health.OnModified += OnHealthModified;
            player.Health.OnDeath += OnDeath;
        }

        private void DeInitializePlayer(Player player)
        {
            if (player == null)
                return;

            player.Health.OnModified -= OnHealthModified;
            player.Health.OnDeath -= OnDeath;
        }

        private void OnDeath(object sender, HealthEventArgs e)
        {
            Debug.LogErrorFormat("Player {0} died thanks to {1}.", e.Causee.GetName(), e.Causer.GetName());
        }

        private void OnHealthModified(object sender, HealthEventArgs e)
        {
            Debug.LogErrorFormat("Player {0} took some damage from {1}.", e.Causee.GetName(), e.Causer.GetName());
        }

        private void CheckGameOutcome()
        {
            foreach (var player in GameManager.Players)
            {
                var score = player.Score;
                if (score.Kills >= KillsToWin)
                {
                    OnGameEnded?.Invoke(this, new GameOutcomeEventArgs(new GameOutcome(player, GameManager.Players.Where(p => p != player).ToList())));
                }
            }
        }
    }
}