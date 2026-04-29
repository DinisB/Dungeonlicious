using UnityEngine;

public interface IDamageable
{
    public Vector3 position { get; }

    public bool CanBeDamaged();

    public void Damage(int amount, GameObject damager);
}
