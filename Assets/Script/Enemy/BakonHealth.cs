using System;
using UnityEngine;

public class BakonHealth : MonoBehaviour, IDamageable
{
    public Vector3 position => transform.position;
    [SerializeField] private int maxHealth = 30;

    [SerializeField] private int currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;

    public event Action OnDamaged;

    public bool CanBeDamaged()
    {
        return true;
    }

    public void Damage(int amount, GameObject damager)
    {
        if(isDead)
        {
            return;
        }

        currentHealth -= amount;

        OnDamaged?.Invoke();

        if(currentHealth <= 0)
        {
            isDead = true;
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
