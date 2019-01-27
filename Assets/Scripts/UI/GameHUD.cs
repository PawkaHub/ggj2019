using Game;
using Networking;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField]
        private Image Fader;
        [SerializeField]
        private Text TitleFader;

        [SerializeField]
        private Text HealthText;
        [SerializeField]
        private Slider HealthBar;
        [SerializeField]
        private Text KillsText;
        [SerializeField]
        private Text DeathsText;

        private GameManager gameManager;
        private bool hasPlayer = false;

        public void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            FindObjectOfType<GameOutcomeManager>().OnGameEnded += OnGameOver;
            gameManager.OnInitialized += OnInit;

            SetFaderAlpha(0, "");
            HealthText.enabled = false;
            HealthBar.gameObject.SetActive(false);
            KillsText.enabled = false;
            DeathsText.enabled = false;
        }

        private void OnInit(object sender, EventArgs e)
        {
            var self = gameManager.Players.FirstOrDefault(p => p.NetworkPlayer.IsSelf);
            if (self != null)
            {
                InitSelf(self);
                return;
            }

            gameManager.ConnectionManager.OnPlayerConnect += OnConnect;
        }

        private void OnConnect(Networking.NetworkPlayer obj)
        {
            if (!obj.IsSelf)
                return;

            InitSelf(obj.Player);
        }

        private void InitSelf(Player self)
        {
            self.Health.OnDeath += OnDeath;
        }

        private static readonly string[] DEATH_QUIPS = new string[]
        {
            "RIP",
            "YOU WILL BE REMEMBERED",
            "YOU WERE LOVED",
        };

        private void OnDeath(object sender, HealthEventArgs e)
        {
            StartCoroutine(FadeInThenOut(DEATH_QUIPS[UnityEngine.Random.Range(0, DEATH_QUIPS.Length)], 0.1f, 0.005f));

            // this is a dumb spot for this but I dont care
            gameManager.Spawner.ReSpawn(e.Causee.Player);
            e.Causee.Player.Health.Revive();
        }

        private void OnGameOver(object sender, GameOutcomeEventArgs e)
        {
            if (e.Outcome.Winner.NetworkPlayer.IsSelf)
            {
                StartCoroutine(FadeInThenOut("YOU'RE THE BEST PET", 0.1f, 0.001f));
            }
            else
            {
                StartCoroutine(FadeInThenOut("YOU LOST. BUT WE STILL LOVE YOU", 0.1f, 0.001f));
            }
        }

        public void Update()
        {
            if (gameManager.Players != null)
            {
                var player = gameManager.Players.FirstOrDefault(p => p.NetworkPlayer.IsSelf);
                if (player != null)
                {
                    var health = player.Health;
                    var score = player.Score;

                    HealthText.text = string.Format("Health   {0}  /  {1}", health.Current, health.Default);
                    HealthBar.value = health.Current / health.Default;

                    KillsText.text = string.Format("Kills  :    {0}", score.Kills);
                    DeathsText.text = string.Format("Deaths  :    {0}", score.Deaths);

                    if (!hasPlayer)
                    {
                        StartCoroutine(FadeInThenOut("LAZER PETZ", 0.1f, 0.005f));
                        HealthText.enabled = true;
                        HealthBar.gameObject.SetActive(true);
                        KillsText.enabled = true;
                        DeathsText.enabled = true;
                    }

                    hasPlayer = true;
                    return;
                }
            }
        }

        private IEnumerator FadeInThenOut(string title, float fadeInSpeed, float fadeOutSpeed)
        {
            yield return FadeIn(title, fadeInSpeed);
            yield return FadeOut(title, fadeOutSpeed);
        }

        private IEnumerator FadeIn(string title, float speed)
        {
            SetFaderAlpha(0, title);
            yield return null;
            while (GetFaderAlpha() < 1)
            {
                SetFaderAlpha(GetFaderAlpha() + speed, title);
                yield return null;
            }
        }

        private IEnumerator FadeOut(string title, float speed)
        {
            SetFaderAlpha(1, title);
            yield return null;
            while (GetFaderAlpha() > 0)
            {
                SetFaderAlpha(GetFaderAlpha() - speed, title);
                yield return null;
            }
        }

        private float GetFaderAlpha()
        {
            return Fader.color.a;
        }

        private void SetFaderAlpha(float alpha, string text)
        {
            var color = Fader.color;
            color.a = alpha;
            Fader.color = color;
            color = TitleFader.color;
            color.a = alpha;
            TitleFader.color = color;
            TitleFader.text = text;
        }
    }
}
