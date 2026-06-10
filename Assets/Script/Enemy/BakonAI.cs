using System;
using Assets.Script.FSM;
using Dungeonlicious.Assets.Script;
using UnityEngine;
using UnityEngine.AI;

public class BakonAI : MonoBehaviour
{
    private GameObject playerAgent;
    private StateMachine fsm;
    private NavMeshAgent agent;
    private IDamageable target;
    private float attackTimer;
    [SerializeField] private float attackCooldown = 3f;
    private bool wasHit;
    private BakonHealth bakonHealth;
    [SerializeField] private float staggerTimer;
    [SerializeField] private Transform[] waypoints;
    private Transform currentWaypoint;

    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private float projectileSpeed = 10f;

    [SerializeField]
    private int projectileDamage = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAgent = FindFirstObjectByType<PlayerHealth>().gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = playerAgent.GetComponent<IDamageable>();
        bakonHealth = GetComponent<BakonHealth>();

        bakonHealth.OnDamaged += HandleDamaged;

        State attack = new State(
            "attack",
            EnterAttack,
            AttackPlayer,
            () => Debug.Log("Exit Attack")
        );

        State move = new State(
            "move",
            EnterMove,
            MoveToPosition,
            null
        );

        State stagger = new State(
            "stagger",
            EnterStagger,
            Stagger,
            () => Debug.Log("Exit Stagger")
        );

        State dead = new State(
            "dead",
            EnterDead,
            null,
            null
        );

        attack.AddTransition(
            new Transition(
            () => !CanSeePlayer(),
            null,
        move));

        move.AddTransition(new Transition(
            () => bakonHealth.CurrentHealth <= 0,
            null,
        dead));

        move.AddTransition(new Transition(
            () => wasHit,
            null,
            stagger));

        move.AddTransition(new Transition(
            () => CanSeePlayer(),
            null,
            attack));

        stagger.AddTransition(
            new Transition(
            () => !wasHit,
            null,
        move));

        attack.AddTransition(
            new Transition(
            () => bakonHealth.CurrentHealth <= 0,
            null,
        dead));

        stagger.AddTransition(
            new Transition(
            () => bakonHealth.CurrentHealth <= 0,
            null,
        dead));

        fsm = new StateMachine(move);

    }

    // Update is called once per frame
    void Update()
    {
        if (bakonHealth.IsDead)
        {
            return;
        }

        fsm.Update()?.Invoke();
    }

    private void OnDestroy()
    {
        if (bakonHealth != null)
            bakonHealth.OnDamaged -= HandleDamaged;
    }

    private void EnterAttack()
    {
        agent.isStopped = true;
        attackTimer = 0f;
    }

    public void AttackPlayer()
    {
        //aim at player for 1-2 seconds, use Raycast to check if projectile path is clear then spawn projectile
        //if Raycast fails to return clear path to player switch to "move" State
        agent.isStopped = true;

        Vector3 targetPosition = playerAgent.transform.position + Vector3.up * 1.5f;

        Vector3 dir = (targetPosition - transform.position).normalized;

        transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime* 5f);

        attackTimer += Time.deltaTime;

        if(attackTimer >= attackCooldown)
        {
            if(CanSeePlayer())
            {
                Shoot();
            }
            attackTimer = 0f;
        }
    }

    public void MoveToPosition()
    {
        //move to a specific map waypoint/ or random navmesh position if failed to aim at player or if attacked by player

        if (wasHit) return;

        agent.isStopped = false;

        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SelectRandomWaypoint();
        }
    }

    private void EnterStagger()
    {
        agent.isStopped = true;
        agent.ResetPath();
        attackTimer = 0f;
        staggerTimer = 0f;
    }

    public void Stagger()
    {
        //when attacked by enemy, shake model and flash
        staggerTimer += Time.deltaTime;

        if(staggerTimer >= 0.5f)
        {
            wasHit = false;
        }
    }
    public void EnterDead()
    {
        //when hp <= 0, destroy self object (perhaps add a particle system affect)
        agent.isStopped = false;
        agent.enabled = false;

        Destroy(gameObject, 2f);
    }

    private bool CanSeePlayer()
    {
        Vector3 dir =
        playerAgent.transform.position - transform.position;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            dir.normalized,
            out RaycastHit hit,
            50f))
        {
            return hit.collider.GetComponentInParent<PlayerHealth>() != null;
        }

        return false;
    }

    private void SelectRandomWaypoint()
    {
        if(waypoints.Length == 0)
        {
            return;
        }

        Transform nextWaypoint;

        do
        {
            nextWaypoint = waypoints[UnityEngine.Random.Range(0, waypoints.Length)];
        }
        while(waypoints.Length > 1 && nextWaypoint == currentWaypoint);

        currentWaypoint = nextWaypoint;

        agent.SetDestination(currentWaypoint.position);
    }

    private void EnterMove()
    {
        agent.isStopped = false;
        SelectRandomWaypoint();
    }

    private void Shoot()
    {
        Vector3 targetPosition =
        playerAgent.transform.position + Vector3.up * 1.5f;

        Vector3 direction =
            (targetPosition - firePoint.position).normalized;

        GameObject projectileObject =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(direction));

        EnemyProjectile projectile =
            projectileObject.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(direction, projectileDamage);
        }
    }

    private void HandleDamaged()
    {
        wasHit = true;
        EnterStagger();
        Debug.Log("HIT REGISTERED");
    }
}
