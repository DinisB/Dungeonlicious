using UnityEngine;

public class EnemySwordSensor : MonoBehaviour
{
    private Enemy _enemy;

    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        CheckForTargetHit(other);
    }

    private void CheckForTargetHit(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
            _enemy.DamageTarget(damageable);
    }

}
