using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Dungeonlicious.Assets.Script
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyType _enemyType;
        private IEnemy _enemy;
        public static Action<GameObject> OnEnemyDeath;

        private void Start()
        {
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
    }

    enum EnemyType
    {
        Slime,
        Bakon
    }
}