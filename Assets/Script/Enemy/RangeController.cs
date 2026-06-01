using System;
using UnityEngine;

namespace Dungeonlicious.Assets.Script
{
    public class RangeController : MonoBehaviour, ICombatEnemy, IDamageable
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private int speed = 3;

        [SerializeField] private CombatChecker combatChecker;

        public static Action<GameObject> OnEnemyDeath;

        private Renderer _renderer;
        [SerializeField] private float _flashDuration = 0.1f;

        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private Transform player;
        private Rigidbody rb;
        private float lastAttackTime;

        [SerializeField] private int maxHealth = 50;

        private int currentHealth;

        public Vector3 position => transform.position;

        private void Start()
        {
            currentHealth = maxHealth;

            _renderer = GetComponent<Renderer>();
            _originalColor = _renderer.material.color;

            player = FindFirstObjectByType<PlayerHealth>().transform;
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > attackRange)
            {
                MoveTo(player.position);
            }
            else
            {
                Shoot();
            }
        }

        private void MoveTo(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;

            rb.MovePosition(
                transform.position +
                direction * speed * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                direction.y = 0;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void Shoot()
        {
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            lastAttackTime = Time.time;

            Vector3 direction =
                (player.position - firePoint.position).normalized;

            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            GameObject projectileObject = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation);

            EnemyProjectile projectile =
                projectileObject.GetComponent<EnemyProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(direction, 10); // damage value
            }
        }
        public void Initialize(CombatChecker checker)
        {
            combatChecker = checker;

            combatChecker.RegisterEnemy(gameObject);
        }
        private void OnDisable()
        {
            if (combatChecker != null)
            {
                combatChecker.UnregisterEnemy(gameObject);
            }
        }

        public bool CanBeDamaged()
        {
            return true;
        }

        public void Damage(int amount, GameObject damager)
        {
            currentHealth -= amount;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashRed());

            if (currentHealth <= 0)
            {
                OnEnemyDeath?.Invoke(gameObject);
                gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator FlashRed()
        {
            _renderer.material.color = Color.red;

            yield return new WaitForSeconds(_flashDuration);

            _renderer.material.color = _originalColor;
        }
    }
}
