using UnityEngine;

namespace Dungeonlicious.Assets.Script
{
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float lifeTime = 5f;

        private Vector3 direction;
        [SerializeField] private int damage;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            Destroy(gameObject, lifeTime);
        }

        public void Initialize(Vector3 direction, int damage)
        {
            rb.linearVelocity = direction.normalized * speed;
            this.damage = damage;

            transform.forward = direction.normalized;

        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Projectile hit: " + other.name);
            
            IDamageable damageable =
                other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.Damage(damage, gameObject);
            }

            Destroy(gameObject);
        }
    }
}
