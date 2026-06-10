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
    private int projectileDamage = 5;

    private float repositionTimer;
    private float attackCooldownTimer;
    [SerializeField] private float cooldownLimit = 5f;

    private bool reachedWaypoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BakonCombat combat = GetComponent<BakonCombat>();
        playerAgent = FindFirstObjectByType<PlayerHealth>().gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = playerAgent.GetComponent<IDamageable>();
        bakonHealth = GetComponent<BakonHealth>();

        if (combat != null)
        {
            waypoints = combat.GetWaypoints();
        }

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
            () => wasHit,
            null,
        stagger));

        attack.AddTransition(new Transition(
            () => repositionTimer >= 3f,
            null,
        move));

        attack.AddTransition(new Transition(
            () => wasHit,
            null,
        stagger));

        move.AddTransition(new Transition(
            () => bakonHealth.CurrentHealth <= 0,
            null,
        dead));

        move.AddTransition(new Transition(
            () => wasHit,
            null,
            stagger));

        move.AddTransition(
            new Transition(
            () => reachedWaypoint,
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
        attackCooldownTimer -= Time.deltaTime;

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
        repositionTimer = 0f;
    }

    public void AttackPlayer()
    {
        //aim at player for 1-2 seconds, use Raycast to check if projectile path is clear then spawn projectile
        //if Raycast fails to return clear path to player switch to "move" State
        Vector3 targetPos =
        playerAgent.transform.position + Vector3.up * 1.5f;

        Vector3 dir =
            (targetPos - transform.position).normalized;

        transform.forward =
            Vector3.Lerp(
                transform.forward,
                dir,
                Time.deltaTime * 10f);

        float angle = Vector3.Angle(transform.forward, dir);

        if (HasClearShot())
        {
            repositionTimer = 0f;

            if (attackCooldownTimer <= 0f && angle < 30f)
            {
                Shoot();
                attackCooldownTimer = cooldownLimit;
            }
        }
        else
        {
            repositionTimer += Time.deltaTime;
        }
    }

    public void MoveToPosition()
    {
        if (agent.pathPending)
        return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            reachedWaypoint = true;
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

        Debug.Log(agent.pathStatus);
    }

    private void EnterMove()
    {
        agent.isStopped = false;
        reachedWaypoint = false;
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
    }

    private bool HasClearShot()
    {
        Vector3 origin =
            firePoint.position;

        Vector3 target =
            playerAgent.transform.position + Vector3.up * 1.5f;

        Vector3 dir = target - origin;

        if (Physics.Raycast(
            origin,
            dir.normalized,
            out RaycastHit hit,
            dir.magnitude))
        {
            return hit.collider.GetComponentInParent<PlayerHealth>() != null;
        }

        return false;
    }
}
