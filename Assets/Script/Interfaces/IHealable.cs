using UnityEngine;

public interface IHealable
{
    public bool CanBeHealed();
    public void Heal(int amount);
}
