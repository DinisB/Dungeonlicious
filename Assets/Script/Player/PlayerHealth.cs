namespace Dungeonlicious.Assets.Script
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

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

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip playerDamaged;

        private bool godMode = false;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            _gameManager = GameManager.instance;
            //_animator       = GetComponent<Animator>();
            _attack = _data.attack;
            _knifesAttack = _data.knifesAttack;
            
            UpgradeManager upgradeManager = UpgradeManager.Instance ?? FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                foreach (IUpgrade upgrade in upgradeManager.GetUpgrades())
                {
                    if (upgrade.upgradeType == UpgradeType.Health)
                    {
                        _extraHealth += (int)upgrade.upgradeValue;
                    }
                    else if (upgrade.upgradeType == UpgradeType.Strength)
                    {
                        _attack += (int)upgrade.upgradeValue;
                    }
                    else if (upgrade.upgradeType == UpgradeType.Speed)
                    {
                        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
                        if (playerMovement != null)
                        {
                            playerMovement.SpeedChange += (int)upgrade.upgradeValue;
                        }
                    }
                    else if (upgrade.upgradeType == UpgradeType.Knife)
                    {
                        _knifesAttack += (int)upgrade.upgradeValue;
                        PlayerKnifeThrow playerKnifeThrow = FindFirstObjectByType<PlayerKnifeThrow>();
                        if (playerKnifeThrow != null)
                        {
                            playerKnifeThrow.AddKnives((int)upgrade.upgradeValue);
                        }
                    }
                }
            }

            _health = _data.maxHealth + _extraHealth;

            DispatchHealthChanged();
        }

        private void Update()
        {
            if(InputSystem.actions.FindAction("God1").IsPressed() && InputSystem.actions.FindAction("God2").WasPressedThisFrame())
            {
                TurnOnGodMode();
            }
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
            if(godMode)
            {
                return;
            }
            audioSource.PlayOneShot(playerDamaged);
            _health = Mathf.Max(0, _health - amount);

            DispatchHealthChanged();
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

        private void TurnOnGodMode()
        {
            if(godMode == false)
            {
                godMode = true;
            }
            else
            {
                godMode = false;
            }
        }

    }
}