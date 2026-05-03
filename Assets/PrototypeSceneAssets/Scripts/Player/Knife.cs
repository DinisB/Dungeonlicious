using UnityEngine;

public class Knife : MonoBehaviour
{
    [SerializeField] private float knifeSpeed = 10f;
    private Vector3 moveDirection;
    private bool isFlying = false;
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
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

        transform.rotation = Quaternion.LookRotation(moveDirection);

        rb.isKinematic = false;
        rb.linearVelocity = moveDirection * knifeSpeed;

        isFlying = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!isFlying) return;

        isFlying = false;

        rb.isKinematic = true;
        col.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(isFlying) return;

        PlayerKnifeThrow playerKnives = other.GetComponent<PlayerKnifeThrow>();

        if (playerKnives != null)
        {
            playerKnives.AddKnife();
            Destroy(gameObject);
        }
    }
}
