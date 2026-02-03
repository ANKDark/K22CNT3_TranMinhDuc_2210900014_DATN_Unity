using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Behaviour Settings")]
    public bool usePatrolPoints = false;
    public float moveSpeed = 2f;
    public float waitTime = 3f;

    [Header("Wander Settings")]
    public float wanderRange = 5f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private int currentPatrolIndex = 0;
    private float waitCounter;
    private bool isWaiting;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        agent.avoidancePriority = Random.Range(30, 70);

        agent.stoppingDistance = 0.1f;
        agent.autoBraking = false;

        startPosition = transform.position;
        waitCounter = waitTime;

        if (usePatrolPoints && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    public void DoPatrolLogic()
    {
        agent.speed = moveSpeed;

        if (usePatrolPoints && patrolPoints.Length > 0)
        {
            MoveToWaypoint();
        }
        else
        {
            WanderAround();
        }
    }

    bool ReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);
    }

    void MoveToWaypoint()
    {
        if (ReachedDestination())
        {
            if (waitCounter > 0)
            {
                waitCounter -= Time.deltaTime;
                isWaiting = true;

                if (anim != null)
                    anim.SetFloat("Speed", 0f);
            }
            else
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                waitCounter = waitTime;
            }
        }
        else
        {
            isWaiting = false;
        }
    }

    void WanderAround()
    {
        if (ReachedDestination())
        {
            if (waitCounter > 0)
            {
                waitCounter -= Time.deltaTime;
                isWaiting = true;

                if (anim != null)
                    anim.SetFloat("Speed", 0f);
            }
            else
            {
                isWaiting = false;
                Vector3 randomPoint = GetRandomPoint(startPosition, wanderRange);
                agent.SetDestination(randomPoint);
                waitCounter = waitTime;
            }
        }
    }

    Vector3 GetRandomPoint(Vector3 center, float range)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }

    public void ReturnToStart()
    {
        agent.speed = moveSpeed * 1.5f;
        agent.SetDestination(startPosition);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? startPosition : transform.position,
            wanderRange
        );
    }
}
