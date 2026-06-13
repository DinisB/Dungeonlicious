using Assets.Script.FSM;
using Dungeonlicious.Assets.Script;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private GameObject playerAgent;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private int damageValue = 3;
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

    private SlimeHealth slimeHealth;

    [SerializeField] private float deathDuration = 0.4f;

    private float deathTimer;
    private Vector3 deathStartScale;
    
    [SerializeField] private ParticleSystem deathParticleSystem;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip slimeDeath;
    [SerializeField] private AudioClip slimeBurn;
    private bool attackSoundPlaying;

    private void Awake()
    {
        playerAgent = FindFirstObjectByType<PlayerHealth>().gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = playerAgent.GetComponent<IDamageable>();
        slimeHealth = GetComponent<SlimeHealth>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        State attack = new State(
            "attack",
            () => Debug.Log("Enter Attack"),
            AttackPlayer,
            ExitAttackState);

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

        State dead = new State(
            "dead",
            EnterDead,
            UpdateDead,
            null
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

        Transition chaseToDead =
            new Transition(() => slimeHealth.IsDead, null, dead);

        Transition attackToDead =
            new Transition(() => slimeHealth.IsDead, null, dead);

        Transition staggerToDead =
            new Transition(() => slimeHealth.IsDead, null, dead);

        chase.AddTransition(chaseToDead);
        chase.AddTransition(chaseToStagger);
        chase.AddTransition(chaseToAttack);
        attack.AddTransition(attackToDead);
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

    private void OnDestroy()
    {
        AudioSource.PlayClipAtPoint(slimeDeath, transform.position);

        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);
    }

    private void ChasePlayer()
    {
        if(HasLineOfSight() && CanReachPlayer())
        {
            agent.isStopped = false;

            Vector3 direction = (playerAgent.transform.position 
            - transform.position).normalized;

            direction.y = 0;

            transform.rotation = Quaternion.LookRotation(direction);

            agent.SetDestination(playerAgent.transform.position);
        }
    }
    private void AttackPlayer()
    {
        agent.isStopped = true;

        if (!attackSoundPlaying)
        {
            audioSource.clip = slimeBurn;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.loop = true;
            audioSource.Play();

            attackSoundPlaying = true;
        }

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;

            if (target != null && target.CanBeDamaged())
            {
                target.Damage(damageValue, gameObject);
            }
        }
    }

    private void ExitAttackState()
    {
        audioSource.Stop();
        audioSource.pitch = 1f;
        attackSoundPlaying = false;
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

    private void EnterDead()
    {
        agent.isStopped = true;
        agent.enabled = false;

        deathTimer = 0f;

        deathStartScale = transform.localScale;

        GetComponent<Collider>().enabled = false;
    }

    private void UpdateDead()
    {
        deathTimer += Time.deltaTime;

        float t = deathTimer / deathDuration;

        transform.localScale =
            Vector3.Lerp(deathStartScale, Vector3.zero, t);

        if (t >= 1f)
        {
            Destroy(gameObject);
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

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = playerAgent.transform.position + Vector3.up;

        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            return hit.transform == playerAgent.transform;
        }

        return false;
    }

    private bool CanReachPlayer()
    {
        NavMeshPath path = new NavMeshPath();

        if (agent.CalculatePath(playerAgent.transform.position, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }

        return false;
    }
}