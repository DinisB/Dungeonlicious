namespace Dungeonlicious.Assets.Script
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;

    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private Image tomatoBar;
        [SerializeField] private PlayerHealth playerHealth;

        void OnEnable()
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;
        }

        void OnDisable()
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }

        void Start()
        {
            UpdateHealthBar((float)playerHealth.GetHealth() / playerHealth.GetMaxHealth());
        }

        private void UpdateHealthBar(float normalizedHealth)
        {
            healthBar.fillAmount = normalizedHealth;
        }

        private void UpdateTomatoBar(float time, float totalTime)
        {
            tomatoBar.fillAmount = 1 - (time / totalTime);
        }

        private IEnumerator UpdateTomatoBarCoroutine(float time)
        {
            float elapsedTime = 0f;
            while (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
                UpdateTomatoBar(elapsedTime, time);
                yield return null;
            }
        }

        public void StartTomatoBarCoroutine(float time)
        {
            StartCoroutine(UpdateTomatoBarCoroutine(time));
        }
    }
}