using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    private Enemy _enemy;

    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        CheckForTargetInRange(other);
    }

    private void CheckForTargetInRange(Collider other)
    {
        if (_enemy.isAlive)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null && damageable.CanBeDamaged() && Physics.Linecast(_enemy.transform.position, other.transform.position, out RaycastHit hitInfo, _enemy.sensorLayerMask, QueryTriggerInteraction.Ignore) && hitInfo.collider == other)
                _enemy.SetTarget(damageable);
        }
    }

    void OnTriggerStay(Collider other)
    {
        CheckForTargetInRange(other);
    }

    void OnTriggerExit(Collider other)
    {
        CheckForTargetOutOfRange(other);
    }

    private void CheckForTargetOutOfRange(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
            _enemy.ClearTarget(damageable);
    }

}
