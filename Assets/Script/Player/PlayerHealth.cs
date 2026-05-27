namespace Dungeonlicious.Assets.Script
{
    using System;
    using UnityEngine;

    public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IAttackUpgrade
    {
        [SerializeField] PlayerData _data;

        private GameManager _gameManager;
        //private Animator    _animator;
        [SerializeField] private int _health;
        private int _extraHealth;
        private int _attack;
        private int _knifesAttack;

        public event Action<float> OnHealthChanged;

        void Start()
        {
            _gameManager = GameManager.instance;
            //_animator       = GetComponent<Animator>();
            _health = _data.maxHealth + _extraHealth;
            _attack = _data.attack;
            _knifesAttack = _data.knifesAttack;
            UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
            foreach (IUpgrade upgrade in upgradeManager.GetComponents<IUpgrade>())
            {
                if (upgrade.upgradeType == UpgradeType.Health)
                {
                    _extraHealth += (int)upgrade.upgradeValue;
                }
                if (upgrade.upgradeType == UpgradeType.Strength)
                {
                    _attack += (int)upgrade.upgradeValue;
                }
                if (upgrade.upgradeType == UpgradeType.Speed)
                {
                    PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
                    playerMovement.SpeedChange += (int)upgrade.upgradeValue;
                }
            }

            DispatchHealthChanged();
        }

        private void DispatchHealthChanged()
        {
            OnHealthChanged?.Invoke((float)_health / (_data.maxHealth + _extraHealth));
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

        public void IncreaseAttack(int amount)
        {
            _attack += amount;
        }

        public bool CanBeHealed()
        {
            return _health < _data.maxHealth + _extraHealth;
        }

        public void Heal(int amount)
        {
            _health = Mathf.Min(_data.maxHealth + _extraHealth, _health + amount);

            DispatchHealthChanged();
        }

        public int GetHealth()
        {
            return _health;
        }

        public int GetMaxHealth()
        {
            return _data.maxHealth + _extraHealth;
        }

        public int GetAttack()
        {
            return _attack;
        }

        public int GetKnifesAttack()
        {
            return _knifesAttack;
        }

    }
}