using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RobotBrain : MonoBehaviour
{
    public FieldOfView fieldOfView;
    public NavMeshAgent navMeshAgent;
    
    public enum State
    {
        Patrol, Chase, Investigate
    }
    
    public State currentState = State.Patrol;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint")
            .Select(go => go.transform).ToArray();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fieldOfView = GetComponent<FieldOfView>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                DoPatrol();
                if(fieldOfView.canSeePlayer)
                    currentState = State.Chase;
                break;
            case State.Chase:
                if(fieldOfView.lastKnownPlayerPosition != Vector3.zero)
                    navMeshAgent.SetDestination(fieldOfView.lastKnownPlayerPosition);
                if(!fieldOfView.canSeePlayer)
                    currentState = State.Investigate;
                break;
            case State.Investigate:
                if (fieldOfView.lastKnownPlayerPosition != Vector3.zero)
                {
                    navMeshAgent.SetDestination(fieldOfView.lastKnownPlayerPosition);
                    
                    if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
                       && !navMeshAgent.pathPending)
                    {
                        fieldOfView.lastKnownPlayerPosition = Vector3.zero;
                        currentState = State.Patrol;
                    }
                }
                if(fieldOfView.canSeePlayer) currentState = State.Chase;
                break;
        }
    }

    private void DoPatrol()
    {
        if (patrolPoints.Length == 0) return;
        
        navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        
        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
           && !navMeshAgent.pathPending)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
}