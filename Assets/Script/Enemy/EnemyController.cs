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

        private void Start()
        {
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

        public void TakeDamage(int damage)
        {
            _enemy.TakeDamage(damage);
            if (_enemy.Health <= 0)
            {
                OnEnemyDeath?.Invoke(gameObject);
                gameObject.SetActive(false);
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            _rigidbody.MovePosition(transform.position + direction * _speed * Time.deltaTime);
        }
    }

    enum EnemyType
    {
        Slime,
        Bakon
    }
}