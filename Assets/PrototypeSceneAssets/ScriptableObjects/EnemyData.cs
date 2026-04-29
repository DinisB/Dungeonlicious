using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public int          maxHealth;
    public float        patrolSpeed;
    public float        chaseSpeed;
    public float        maxIdleTime;
    public float        maxPatrolTime;
    public float        maxAttackRange;
    public float        minAttackDot;
    public int          attackDamage;
    public float        attackCooldown;
    public float        hurtCooldown;
    public LayerMask    sensorLayerMask;
}
