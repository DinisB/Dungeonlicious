using UnityEngine;

public interface IAttackUpgrade
{
    public bool CanUpgradeAttack();
    public void IncreaseAttack(int amount);
}
