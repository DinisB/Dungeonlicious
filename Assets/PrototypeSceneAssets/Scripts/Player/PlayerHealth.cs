using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] PlayerData _data;

    private GameManager _gameManager;
    //private Animator    _animator;
    [SerializeField] private int         _health;

    public event Action<float> OnHealthChanged;

    void Start()
    {
        _gameManager    = GameManager.instance;
        //_animator       = GetComponent<Animator>();
        _health         = _data.maxHealth;

        DispatchHealthChanged();
    }

    private void DispatchHealthChanged()
    {
        OnHealthChanged?.Invoke((float)_health / _data.maxHealth);
    }

    public Vector3 position
    {
        get
        {
            return transform.position;
        }
    }

    public bool CanBeDamaged()
    {
        return _health > 0;
    }

    public void Damage(int amount, GameObject damager)
    {
        //if (!_gameManager.cheatsEnabled)
        //{
            _health = Mathf.Max(0, _health - amount);

            //_animator.SetTrigger("Damage");

            DispatchHealthChanged();
        //}
    }

    public bool CanBeHealed()
    {
        return _health < _data.maxHealth;
    }

    public void Heal(int amount)
    {
        _health = Mathf.Min(_data.maxHealth, _health + amount);

        DispatchHealthChanged();
    }

}
