using System.Collections;
using UnityEngine;

public class SlimeHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;
    private SlimeAI slimeAI;
    public Vector3 position => transform.position;

    [SerializeField] private Renderer slimeRenderer;
    [SerializeField] private Color hitColor = Color.white;

    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine squashCoroutine;

    private bool isDead;

    public bool IsDead => isDead;

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
        if (isDead)
            return;

        currentHealth -= amount;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(Flash());

        if (squashCoroutine != null)
            StopCoroutine(squashCoroutine);

        squashCoroutine = StartCoroutine(HitSquash());

        slimeAI.Stagger(damager.transform.position);

        if (currentHealth <= 0)
        {
            isDead = true;
        }
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
    private IEnumerator HitSquash()
    {
        Vector3 originalScale = transform.localScale;

        transform.localScale = originalScale * 0.85f;

        yield return new WaitForSeconds(0.05f);

        transform.localScale = originalScale;
    }
}
