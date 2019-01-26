﻿using UnityEngine;

namespace Game
{
    public class PlayerScore : PlayerBehaviour
    {
        [SerializeField]
        private int deaths;
        public int Deaths { get { return deaths; } }
        [SerializeField]
        private int kills;
        public int Kills { get { return kills; } }
        [SerializeField]
        private float damageTaken;
        public float DamageTaken { get { return damageTaken; } }
        [SerializeField]
        private float damageDealt;
        public float DamageDealt { get { return damageDealt; } }

        public override void Awake()
        {
            base.Awake();

            var health = Player.GetComponent<Health>();
            health.OnDeath += HandleDeath;
            health.OnModified += HandleDamage;
        }

        private void HandleDamage(object sender, HealthEventArgs e)
        {
            OnDamageTaken(e.Modification);
            e.Causer.GameObject.GetComponent<PlayerScore>()?.OnDamageDealt(e.Modification);
        }

        private void HandleDeath(object sender, HealthEventArgs e)
        {
            OnDeath();
            e.Causer.GameObject.GetComponent<PlayerScore>()?.OnKill();
        }

        private void OnDeath()
        {
            deaths++;
        }

        private void OnKill()
        {
            kills++;
        }

        private void OnDamageTaken(float amount)
        {
            damageDealt -= amount; //negative, since amount is a health modification
        }

        private void OnDamageDealt(float amount)
        {
            damageTaken -= amount; //negative, since amount is a health modification
        }
    }
}