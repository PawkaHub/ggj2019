using Game;
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

            var color = Fader.color;
            color.a = 0;
            Fader.color = color;
            color = TitleFader.color;
            color.a = 0;
            TitleFader.color = color;
            HealthText.enabled = false;
            HealthBar.gameObject.SetActive(false);
            KillsText.enabled = false;
            DeathsText.enabled = false;
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

                    HealthText.text = string.Format("Health {0}/{1}", health.Current, health.Default);
                    HealthBar.value = health.Current / health.Default;

                    KillsText.text = string.Format("Kills: {0}", score.Kills);
                    DeathsText.text = string.Format("Deaths: {0}", score.Deaths);

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
            SetFaderAlpha(0);
            yield return null;
            while (GetFaderAlpha() < 1)
            {
                SetFaderAlpha(GetFaderAlpha() + speed);
                yield return null;
            }
        }

        private IEnumerator FadeOut(string title, float speed)
        {
            SetFaderAlpha(1);
            yield return null;
            while (GetFaderAlpha() > 0)
            {
                SetFaderAlpha(GetFaderAlpha() - speed);
                yield return null;
            }
        }

        private float GetFaderAlpha()
        {
            return Fader.color.a;
        }

        private void SetFaderAlpha(float alpha)
        {
            var color = Fader.color;
            color.a = alpha;
            Fader.color = color;
            color = TitleFader.color;
            color.a = alpha;
            TitleFader.color = color;
        }
    }
}
