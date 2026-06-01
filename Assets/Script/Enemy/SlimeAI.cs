using Assets.Script.FSM;
using Dungeonlicious.Assets.Script.Enums;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private GameObject playerAgent;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float damageInterval = 1f;
    private float damageTimer;
    private NavMeshAgent agent;
    private StateMachine fsm;
    private IDamageable target;

    private bool isStaggered;
    private float staggerTimer;

    [SerializeField] private float staggerDuration = 0.3f;

    [SerializeField] private float staggerDistance = 1.5f;

    private Vector3 staggerStartPos;
    private Vector3 staggerTargetPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = playerAgent.GetComponent<IDamageable>();

        State attack = new State(
            "attack",
            () => Debug.Log("Enter Attack"),
            AttackPlayer,
            ()=> Debug.Log("Exit Attack"));

        State chase = new State(
            "chase",
            () => Debug.Log("Enter Chase"),
            ChasePlayer,
            ()=> Debug.Log("Exit Chase"));
        
        State stagger = new State(
            "stagger",
            () => agent.isStopped = true,
            UpdateStagger,
            () => isStaggered = false
        );

        Transition chaseToAttack = new Transition(
            () => Vector3.Distance(transform.position,
                playerAgent.transform.position) <= attackRange,
            null,
            attack
        );

        chase.AddTransition(chaseToAttack);

        Transition attackToChase = new Transition(
            () => Vector3.Distance(transform.position,
            playerAgent.transform.position) > attackRange,
            null,
            chase
        );

        Transition chaseToStagger = new Transition(
            () => isStaggered,
            null,
            stagger
        );

        Transition attackToStagger = new Transition(
            () => isStaggered,
            null,
            stagger
        );

        chase.AddTransition(chaseToStagger);
        attack.AddTransition(attackToStagger);
        attack.AddTransition(attackToChase);

        Transition staggerToChase = new Transition(
            () => !isStaggered,
            null,
            chase
        );

        stagger.AddTransition(staggerToChase);

        fsm = new StateMachine(chase);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update()?.Invoke();
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        Vector3 direction = (playerAgent.transform.position 
        - transform.position).normalized;

        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);

        agent.SetDestination(playerAgent.transform.position);
    }
    private void AttackPlayer()
    {
        agent.isStopped = true;

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;

            if (target != null && target.CanBeDamaged())
            {
                target.Damage(5, gameObject);
            }
        }
    }

    private void UpdateStagger()
    {
        staggerTimer += Time.deltaTime;

        float t = staggerTimer / staggerDuration;

        transform.position =
            Vector3.Lerp(
                staggerStartPos,
                staggerTargetPos,
                t);

        if (t >= 1f)
        {
            isStaggered = false;
        }
    }

    public void Stagger(Vector3 attackerPosition)
    {
        isStaggered = true;

        staggerTimer = 0f;

        staggerStartPos = transform.position;

        Vector3 direction =
            (transform.position - attackerPosition).normalized;

        direction.y = 0;

        staggerTargetPos =
            staggerStartPos + direction * staggerDistance;
    }
}