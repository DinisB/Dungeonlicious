using UnityEngine;

public class SlimeHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;
    private SlimeAI slimeAI;
    public Vector3 position => transform.position;

    private void Awake()
    {
        currentHealth = maxHealth;
        slimeAI = GetComponent<SlimeAI>();
    }

    public bool CanBeDamaged()
    {
        return currentHealth > 0;
    }

    public void Damage(int amount, GameObject damager)
    {
        currentHealth -= amount;

        slimeAI.Stagger(damager.transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}
