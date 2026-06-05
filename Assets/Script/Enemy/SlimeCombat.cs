using UnityEngine;

public class SlimeCombat : MonoBehaviour, ICombatEnemy
{
    private CombatChecker combatChecker;
    public void Initialize(CombatChecker checker)
    {
        combatChecker = checker;
        combatChecker.RegisterEnemy(gameObject);
    }
    private void OnDestroy()
    {
        combatChecker?.UnregisterEnemy(gameObject);
    }
}
