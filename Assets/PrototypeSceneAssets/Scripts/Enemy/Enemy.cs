using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData  _data;
    [SerializeField] private Transform  _waypoints;

    [SerializeField] private GameObject _damageAreaPrefab;
    [SerializeField] private Transform _attackPoint;

    private enum State { Idling, Patrolling, Chasing, Attacking, Hurting, Dead };

    private NavMeshAgent    _agent;
    private Animator        _animator;
    private State           _state;
    private float           _stateTimer;
    private int             _health;
    private int             _nextWaypoint;
    private IDamageable     _target;
    private float           _lastAttackTime;

    public LayerMask sensorLayerMask
    {
        get
        {
            return _data.sensorLayerMask;
        }
    }

    public bool isAlive
    {
        get
        {
            return _state != State.Dead;
        }
    }

    private void Start()
    {
        _agent          = GetComponent<NavMeshAgent>();
        _animator       = GetComponent<Animator>();
        _health         = _data.maxHealth;
        _target         = null;
        _lastAttackTime = float.MinValue;

        EnterIdlingState();
    }

    private void EnterIdlingState()
    {
        _state = State.Idling;

        _stateTimer = Random.Range(0f, _data.maxIdleTime);

        _agent.isStopped = true;
    }

    private void EnterPatrollingState()
    {
        _state = State.Patrolling;

        _nextWaypoint   = Random.Range(0, _waypoints.childCount);
        _stateTimer     = _data.maxPatrolTime;

        _agent.SetDestination(_waypoints.GetChild(_nextWaypoint).position);

        _agent.speed        = _data.patrolSpeed;
        _agent.isStopped    = false;
    }

    private void EnterChasingState()
    {
        _state = State.Chasing;

        _agent.SetDestination(_target.position);

        _agent.speed        = _data.chaseSpeed;
        _agent.isStopped    = false;
    }

    private void EnterAttackingState()
    {
        _state = State.Attacking;

        _agent.isStopped    = true;
        _agent.velocity     = Vector3.zero;
    }

    private void EnterHurtingState()
    {
        _state      = State.Hurting;
        _stateTimer = _data.hurtCooldown;

        _agent.isStopped    = true;
        _agent.velocity     = Vector3.zero;

        _animator.SetTrigger("Hurt");
    }

    private void EnterDeadState()
    {
        _state = State.Dead;

        _agent.isStopped    = true;
        _agent.velocity     = Vector3.zero;   

        _animator.SetTrigger("Death");
    }

    public void SetTarget(IDamageable target)
    {
        if (_target == null || (target == _target && _agent.destination != _target.position) || 
            Vector3.Distance(transform.position, target.position) < Vector3.Distance(transform.position, _agent.destination))
        {
            _target = target;
            EnterChasingState();
        }
    }

    public void ClearTarget(IDamageable target)
    {
        if (_target == target)
            _target = null;
    }

    public void DamageTarget(IDamageable target)
    {
        if (target.CanBeDamaged())
            target.Damage(_data.attackDamage, gameObject);
    }

    void Update()
    {
        switch (_state)
        {
            case State.Idling:
                UpdateIdlingState();
                break;
            
            case State.Patrolling:
                UpdatePatrollingState();
                break;

            case State.Chasing:
                UpdateChasingState();
                break;

            case State.Attacking:
                UpdateAttackingState();
                break;

            case State.Hurting:
                UpdateHurtingState();
                break;
        }
    }

    private void UpdateIdlingState()
    {
        _stateTimer -= Time.deltaTime;

        if (_stateTimer <= 0f)
            EnterPatrollingState();
    }

    private void UpdatePatrollingState()
    {
        _stateTimer -= Time.deltaTime;

        if (_agent.remainingDistance == 0f || _stateTimer <= 0f)
            EnterIdlingState();
    }

    private void UpdateChasingState()
    {
        if (IsTargetInAttackRange())
            EnterAttackingState();
        else if (_agent.remainingDistance == 0f)
            EnterIdlingState();
    }

    private bool IsTargetInAttackRange()
    {
        if (_target != null)
        {
            float distance = Vector3.Distance(_target.position, transform.position);
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, directionToTarget);

            return distance <= _data.maxAttackRange && dot >= _data.minAttackDot;
        }
        else
            return false;
    }

    private void UpdateAttackingState()
    {
        if (Time.time - _lastAttackTime > _data.attackCooldown)
        {
            if (_target.CanBeDamaged() && IsTargetInAttackRange())
                Attack();
            else if (!_target.CanBeDamaged())
                EnterIdlingState();
            else if (_target != null)
                EnterChasingState();
            else
                EnterIdlingState();
        }
    }

    private void Attack()
    {
        _lastAttackTime = Time.time;

        _animator.SetTrigger("Attack");
    }

    private void UpdateHurtingState()
    {
        _stateTimer -= Time.deltaTime;

        if (_stateTimer <= 0f)
        {
            if (_target != null)
                EnterChasingState();
            else
                EnterPatrollingState();
        }
    }

    public Vector3 position
    {
        get
        {
            return transform.position;
        }
    }

    public bool CanBeDamaged()
    {
        return _health > 0;
    }

    public void Damage(int amount, GameObject damager)
    {
        _health -= amount;

        IDamageable damageable = damager.GetComponent<IDamageable>();

        if (damageable != null)
            _target = damageable;

        if (_health > 0)
            EnterHurtingState();
        else
            EnterDeadState();
    }

    public void SpawnDamageArea()
    {
        GameObject obj = Instantiate(_damageAreaPrefab, _attackPoint.position, transform.rotation);

        DamageAreaSlime area = obj.GetComponent<DamageAreaSlime>();
        area.Initialize(_data.attackDamage, gameObject);

    }

}
