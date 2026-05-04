using System;
using UnityEngine;

namespace Dungeonlicious.Assets.Script
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyType _enemyType;
        [SerializeField] private int _speed;
        private IEnemy _enemy;
        public static Action<GameObject> OnEnemyDeath;
        private Rigidbody _rigidbody;

        private Renderer _renderer;
        [SerializeField] private float _flashDuration = 0.1f;

        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private Transform _player;

        //Slime AI
        [SerializeField] private GameObject _damageAreaPrefab;
        [SerializeField] private Transform _attackPoint;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _damage = 5;

        private float _lastAttackTime;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerHealth>().transform;
            _renderer = GetComponent<Renderer>();

            _originalColor = _renderer.material.color;

            _rigidbody = GetComponent<Rigidbody>();

            switch (_enemyType)
            {
                case EnemyType.Slime:
                    _enemy = new Slime();
                    break;
                case EnemyType.Bakon:
                    _enemy = new Bakon();
                    break;
            }
        }

        private void Update()
        {
            if (_player == null) return;

            float distance = Vector3.Distance(transform.position, _player.position);

            if (distance > _attackRange)
            {
                MoveTo(_player.position);
            }
            else
            {
                TryAttack();
            }
        }

        public void TakeDamage(int damage)
        {
            _enemy.TakeDamage(damage);
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashWhite());

            if (_enemy.Health <= 0)
            {
                OnEnemyDeath?.Invoke(gameObject);
                gameObject.SetActive(false);
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            /*
            Vector3 direction = (targetPosition - transform.position).normalized;
            _rigidbody.MovePosition(transform.position + direction * _speed * Time.deltaTime);
            */
                Vector3 direction = (targetPosition - transform.position).normalized;

                _rigidbody.MovePosition(transform.position + direction * _speed * Time.deltaTime);

                if (direction != Vector3.zero)
                {
                    direction.y = 0f; // prevent tilting up/down
                    transform.rotation = Quaternion.LookRotation(direction);
                }
        }
        private System.Collections.IEnumerator FlashWhite()
        {
            _renderer.material.color = Color.white;

            yield return new WaitForSeconds(_flashDuration);

            _renderer.material.color = _originalColor;
        }

        private void TryAttack()
        {
            if (Time.time < _lastAttackTime + _attackCooldown) return;

            _lastAttackTime = Time.time;

            GameObject damageArea = Instantiate(_damageAreaPrefab, _attackPoint.position, _attackPoint.rotation);

            DamageAreaSlime damageScript = damageArea.GetComponent<DamageAreaSlime>();
            damageScript.Initialize(_damage, gameObject);
        }
    }


    enum EnemyType
    {
        Slime,
        Bakon
    }
}