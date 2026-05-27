using Assets.Script.FSM;
using Dungeonlicious.Assets.Script.Enums;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private GameObject playerAgent;
    [SerializeField] private float telegraphMovementValue = 1f;
    [SerializeField] private float minDistanceToAttack = 1f;
    private NavMeshAgent agent;
    private StateMachine fsm;


    private AttackPhase currentAttackPhase;
    private float attackTimer = 0f;
    [SerializeField] private float telegraphDuration = 0.25f;
    [SerializeField] private float slamDuration = 0.25f;
    [SerializeField] private float cooldownDuration = 0.25f;
    [SerializeField] private float slamDistance = 2f;
    private Vector3 phaseStartPos;
    private Vector3 phaseTargetPos;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        State chase = new State(
            "chase",
            () => 
            {
                Debug.Log("Enter Chase");

                attackTimer = 0f;
                isTelegraphing = true;

                attackStartPos = transform.position;

                telegraphTargetPos = 
                attackStartPos - transform.forward * telegraphMovementValue;

                slamTargetPos =
                attackStartPos + transform.forward * 2f;
            },
            ChasePlayer,
            ()=> Debug.Log("Exit Chase"));
        
        State attack = new State(
            "attack",
            () => Debug.Log("Enter Attack"),
            BodySlam, //attack state action
            () => Debug.Log("Exit Attack")
        );

        Transition chase2Attack = new Transition(
            () => (playerAgent.transform.position 
            - transform.position).magnitude <= minDistanceToAttack,
            () => {Debug.Log("Transition from Chase to Attack");},
            attack
        );

        chase.AddTransition(chase2Attack);

        Transition attack2Chase = new Transition(
            () => (playerAgent.transform.position 
            - transform.position).magnitude > minDistanceToAttack,
            () => {Debug.Log("Transition from Attack to Chase");},
            chase
        );

        attack.AddTransition(attack2Chase);

        fsm = new StateMachine(chase);
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update()?.Invoke();
    }

    private void ChasePlayer()
    {
        Vector3 direction = (playerAgent.transform.position 
        - transform.position).normalized;

        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);

        agent.SetDestination(playerAgent.transform.position);
    }

    private void BodySlam()
    {
        /*
        PerformAttackTelegraph();

        float elapsedTime = 0f;
        float duration = 0.25f;

        float t = Mathf.SmoothStep(
            0,
            1,
            elapsedTime / duration
        );

        Vector3 startPos = transform.position;

        Vector3 targetPos = startPos + transform.forward * 2f;

        transform.position = Vector3.Lerp(
            startPos,
            targetPos,
            t
        );
        */
    }

    private void PerformAttackTelegraph()
    {
        float elapsedTime = 0f;
        float duration = 0.25f;

        float t = Mathf.SmoothStep(
            0,
            1,
            elapsedTime / duration
        );

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos - transform.forward * 1f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(
                startPos,
                targetPos,
                t
        );  
        }
    }
}
