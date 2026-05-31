using UnityEngine;

namespace Dungeonlicious.Assets.Script
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifeTime = 5f;

        private Vector3 direction;
        [SerializeField] private int damage;

        public void Initialize(Vector3 direction, int damage)
        {
            this.direction = direction;
            this.damage = damage;

            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            IDamageable damageable =
                other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.Damage(damage, gameObject);
                Destroy(gameObject);
            }
        }
    }
}
