using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RobotBrain : MonoBehaviour
{
    public FieldOfView fieldOfView;
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    
    public enum State
    {
        Patrol, Chase, Investigate
    }
    
    public State currentState = State.Patrol;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Animation Settings")]
    public string idleState = "Monster07_Idle";
    public string runState = "Monster07_Run";
    public string attackState = "Monster07_Attack01";
    public float attackRange = 2.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint")
            .Select(go => go.transform).ToArray();
        navMeshAgent = GetComponent<NavMeshAgent>();
        fieldOfView = GetComponent<FieldOfView>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private float animatorUpdateTimer;
    private static readonly float AnimatorUpdateInterval = 0.1f;

    // Update is called once per frame
    void Update()
    {
        animatorUpdateTimer -= Time.deltaTime;
        if (animatorUpdateTimer <= 0)
        {
            animatorUpdateTimer = AnimatorUpdateInterval;
            UpdateAnimation();
        }

        switch (currentState)
{
            case State.Patrol:
                DoPatrol();
                if(fieldOfView.canSeePlayer)
                    currentState = State.Chase;
                break;
            case State.Chase:
                DoChase();
                break;
            case State.Investigate:
                DoInvestigate();
                break;
        }
    }

    private void DoChase()
    {
        if (fieldOfView.lastKnownPlayerPosition != Vector3.zero)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, fieldOfView.lastKnownPlayerPosition);

            if (distanceToPlayer <= attackRange)
            {
                // Within attack range: Stop and Attack
                navMeshAgent.isStopped = true;
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName(attackState))
                {
                    animator.CrossFade(attackState, 0.1f);
                }
            }
            else
            {
                // Outside range: Chase
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(fieldOfView.lastKnownPlayerPosition);
            }
        }

        if (!fieldOfView.canSeePlayer)
        {
            navMeshAgent.isStopped = false;
            currentState = State.Investigate;
        }
    }

    private void DoInvestigate()
    {
        if (fieldOfView.lastKnownPlayerPosition != Vector3.zero)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(fieldOfView.lastKnownPlayerPosition);
            
            if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
               && !navMeshAgent.pathPending)
            {
                fieldOfView.lastKnownPlayerPosition = Vector3.zero;
                currentState = State.Patrol;
            }
        }
        if(fieldOfView.canSeePlayer) currentState = State.Chase;
    }

    private void DoPatrol()
    {
        if (patrolPoints.Length == 0) return;
        
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        
        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
           && !navMeshAgent.pathPending)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // If currently attacking and the animation hasn't finished, let it play
        if (stateInfo.IsName(attackState) && stateInfo.normalizedTime < 0.95f)
        {
            return;
        }

        // Determine if we should be running or idling based on NavMesh velocity
        float speed = navMeshAgent.velocity.magnitude;
        bool isMoving = speed > 0.1f && !navMeshAgent.isStopped;

        string targetState = isMoving ? runState : idleState;

        // Apply state if not already playing
        if (!stateInfo.IsName(targetState))
        {
            animator.CrossFade(targetState, 0.2f);
        }
    }
}
