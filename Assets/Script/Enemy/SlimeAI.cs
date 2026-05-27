using Assets.Script.FSM;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    [SerializeField] private GameObject playerAgent;
    private NavMeshAgent agent;
    private StateMachine fsm;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        State chase = new State(
            "chase",
            () => Debug.Log("Enter Chase"),
            ChasePlayer,
            ()=> Debug.Log("Exit Chase"));
        

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

        agent.SetDestination(playerAgent.transform.position);
    }
}
