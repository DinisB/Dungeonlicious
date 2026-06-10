using UnityEngine;

public class BakonCombat : MonoBehaviour, ICombatEnemy
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

    public Transform[] GetWaypoints()
    {
        return combatChecker != null ? combatChecker.Waypoints : null;
    }
}
