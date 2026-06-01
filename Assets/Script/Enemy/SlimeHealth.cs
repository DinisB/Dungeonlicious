using System.Collections;
using UnityEngine;

public class SlimeHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;
    private SlimeAI slimeAI;
    public Vector3 position => transform.position;

    [SerializeField] private Renderer slimeRenderer;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        originalColor = slimeRenderer.material.color;
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

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(Flash());

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

    private IEnumerator Flash()
    {
        for (int i = 0; i < 3; i++)
        {
            slimeRenderer.material.color = hitColor;
            yield return new WaitForSeconds(0.05f);

            slimeRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
