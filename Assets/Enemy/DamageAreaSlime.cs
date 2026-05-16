using UnityEngine;

public class DamageAreaSlime : MonoBehaviour
{
    private int _damage;
    private GameObject _owner;
    private bool _hasHit = false;

    public void Initialize(int damage, GameObject owner)
    {
        _damage = damage;
        _owner = owner;

        Destroy(gameObject, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        IDamageable target = other.GetComponent<IDamageable>();

        if(target != null && other.gameObject != _owner)
        {
            if (target.CanBeDamaged())
            {
                target.Damage(_damage, _owner);
                _hasHit = true;
            }
        }
    }

}
