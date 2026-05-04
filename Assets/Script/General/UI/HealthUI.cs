using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    private Image healthBar;
    private PlayerHealth playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = GetComponent<Image>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = playerHealth.GetHealth() / (float)playerHealth.GetMaxHealth();
    }
}
