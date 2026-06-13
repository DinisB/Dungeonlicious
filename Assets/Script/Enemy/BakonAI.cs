using System;
using System.Collections;
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

    [Header("Shake")]
    private float shakeDuration = 0.15f;
    private float shakeMagnitude = 0.1f;

    [Header("Flash")]
    private float flashDuration = 0.1f;
    private Color hitColor = Color.white;

    private Vector3 originalPosition;
    private Renderer[] renderers;
    private Color[] originalColors;

    [SerializeField] private ParticleSystem deathParticleSystem;

    [SerializeField] private float movementRadius = 10f;
    [SerializeField] private float minMovementDistance = 4f;

    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private float ammoRestoreTime = 2f;

    private int currentAmmo;
    private float ammoRestoreTimer;
    private bool restoringAmmo;

    [SerializeField] private Animator anim;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip chickenShot;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        anim = GetComponentInChildren<Animator>();
        
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        BakonCombat combat = GetComponent<BakonCombat>();
        playerAgent = FindFirstObjectByType<PlayerHealth>().gameObject;
        agent = GetComponent<NavMeshAgent>();
        target = playerAgent.GetComponent<IDamageable>();
        bakonHealth = GetComponent<BakonHealth>();

        anim = GetComponentInChildren<Animator>();

        currentAmmo = maxAmmo;

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
            () => anim.SetBool("Move", false)
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
        {
            bakonHealth.OnDamaged -= HandleDamaged;
        }

        AudioSource.PlayClipAtPoint(deathSound, transform.position);
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);
    }

    private void EnterAttack()
    {
        agent.isStopped = true;
        repositionTimer = 0f;

        if (currentAmmo <= 0)
        {
            restoringAmmo = true;
            ammoRestoreTimer = ammoRestoreTime;
        }
    }

    public void AttackPlayer()
    {
        if (restoringAmmo)
        {
            ammoRestoreTimer -= Time.deltaTime;

            if (ammoRestoreTimer <= 0f)
            {
                currentAmmo = maxAmmo;
                restoringAmmo = false;

                repositionTimer = 3f;
            }

            return;
        }
        Vector3 targetPos =
        playerAgent.transform.position /*+ Vector3.up*/;

        Vector3 dir =
            (targetPos - transform.position).normalized;

        transform.forward =
            Vector3.Lerp(
                transform.forward,
                dir,
                Time.deltaTime * 10f);

        float angle = Vector3.Angle(transform.forward, dir);

        if (HasClearShot() && CanReachPlayer())
        {
            repositionTimer = 0f;

            if (attackCooldownTimer <= 0f && angle < 30f)
            {
                Shoot();
                currentAmmo--;

                attackCooldownTimer = cooldownLimit;

                if (currentAmmo <= 0)
                {
                    restoringAmmo = true;
                    ammoRestoreTimer = ammoRestoreTime;
                }
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
        PlayDamageEffect();
        staggerTimer += Time.deltaTime;

        if(staggerTimer >= 0.5f)
        {
            wasHit = false;
        }
    }
    public void EnterDead()
    {
        //when hp <= 0, destroy self object (perhaps add a particle system affect)
        //agent.isStopped = false;
        //agent.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    private void SelectRandomWaypoint()
    {
        if (TryGetRandomPoint(transform.position, movementRadius, out Vector3 destination))
        {
            agent.SetDestination(destination);
        }
        /*
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
        */
    }

    private void EnterMove()
    {
        anim.SetBool("Move", true);
        agent.isStopped = false;
        reachedWaypoint = false;
        SelectRandomWaypoint();
    }

    private void Shoot()
    {
        audioSource.PlayOneShot(chickenShot);

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

    public void PlayDamageEffect()
    {
        StartCoroutine(Shake());
        StartCoroutine(Flash());
    }

    private IEnumerator Shake()
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            transform.localPosition =
                originalPos + UnityEngine.Random.insideUnitSphere * shakeMagnitude;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private IEnumerator Flash()
    {
        foreach (var rend in renderers)
        {
            if (rend.material.HasProperty("_Color"))
                rend.material.color = hitColor;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = originalColors[i];
        }
    }

    private IEnumerator DeathRoutine()
    {
        agent.isStopped = true;
        agent.enabled = false;

        Quaternion start = transform.rotation;
        Quaternion end = start * Quaternion.Euler(0, 0, 90);

        float duration = 0.5f;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(start, end, t / duration);
            yield return null;
        }

        Destroy(gameObject, 2f);
    }

    private bool TryGetRandomPoint(
    Vector3 center,
    float radius,
    out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint =
                center + UnityEngine.Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(
                randomPoint,
                out NavMeshHit hit,
                2f,
                NavMesh.AllAreas))
            {
                float distance =
                    Vector3.Distance(center, hit.position);

                if (distance < minMovementDistance)
                    continue;

                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        result = center;
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
