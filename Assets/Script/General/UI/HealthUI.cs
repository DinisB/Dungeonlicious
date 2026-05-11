using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;
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
}