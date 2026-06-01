namespace Dungeonlicious.Assets.Script
{

    using UnityEngine;

    public class Knife : MonoBehaviour
    {
        [SerializeField] private float knifeSpeed = 10f;
        private Vector3 moveDirection;
        private bool isFlying = false;
        private Rigidbody rb;
        private Collider col;

        [SerializeField] private int damage = 20;
        private GameObject owner;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // ADD THIS
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void SetDirection(Vector3 direction)
        {
            moveDirection = direction.normalized;

            transform.rotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(0, 90, 0);

            rb.isKinematic = false;
            rb.linearVelocity = moveDirection * knifeSpeed;

            isFlying = true;
        }

        public void SetOwner(GameObject owner)
        {
            this.owner = owner;
        }

        public GameObject GetOwner()
        {
            return owner;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isFlying) return;

            isFlying = false;

            IDamageable enemy = collision.gameObject.GetComponent<IDamageable>();

            if (enemy != null)
            {
                enemy.Damage(damage, gameObject);
            }

            rb.isKinematic = true;
            col.isTrigger = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (isFlying) return;

            PlayerKnifeThrow playerKnives = other.GetComponent<PlayerKnifeThrow>();

            if (playerKnives != null)
            {
                playerKnives.AddKnife();
                Destroy(gameObject);
            }
        }
    }
}