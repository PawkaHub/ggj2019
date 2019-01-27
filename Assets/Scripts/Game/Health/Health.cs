using System;

namespace Game
{
    public class HealthEventArgs : EventArgs
    {
        public float Modification;
        public Damager Causee;
        public Damager Causer;

        public HealthEventArgs(Damager causee, Damager causer, float modification)
        {
            Modification = modification;
            Causee = causee;
            Causer = causer;
        }
    }

    public class Health : PlayerBehaviour
    {
        public float Default = 100;
        public float Current { get; set; }
        public bool IsDead { get; set; }

        public event EventHandler<HealthEventArgs> OnModified;
        public event EventHandler<HealthEventArgs> OnDeath;

        public override void Awake()
        {
            base.Awake();

            Current = Default;
            IsDead = false;
        }

        public HealthEventArgs Modify(float amount, Damager causer)
        {
            if (IsDead)
            {
                return null;
            }

            Current += amount;

            HealthEventArgs args = new HealthEventArgs(new Damager(Player), causer, amount);
            if (Current <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke(this, args);
                return args;
            }

            OnModified?.Invoke(this, new HealthEventArgs(new Damager(Player), causer, amount));
            return args;
        }

        public void Revive()
        {
            Current = Default;
            IsDead = false;
        }
    }
}