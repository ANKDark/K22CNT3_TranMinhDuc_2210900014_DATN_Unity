using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Combat Settings")]
    public float damage = 10f;
    public float def = 10f;
    public float attackCooldown = 2f;
    public float chaseRange = 10f;
    public float maxChaseRange = 20f;
    public float attackRange = 1.5f;
    public float runSpeed = 5f;
    public float turnSpeed = 5f;

    [Header("References")]
    public Transform target;

    protected EnemySenses senses;
    protected float lastAttackTime;
    protected NavMeshAgent agent;
    protected Animator anim;
    protected EnemyPatrol patrolScript;
    protected Vector3 startPosition;
    protected EnemyStats enemyStats;
    protected PlayerStats targetStats;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;
        anim = GetComponent<Animator>();
        patrolScript = GetComponent<EnemyPatrol>();
        startPosition = transform.position;
        senses = GetComponent<EnemySenses>();
        enemyStats = GetComponent<EnemyStats>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            targetStats = target.GetComponent<PlayerStats>();
        }
    }

    protected virtual void Update()
    {
        if (target == null) return;

        if (targetStats != null && targetStats.isPlayerDead)
        {
            agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }
        
        if (enemyStats != null)
        {
            if (enemyStats.currentHealth <= 0) return;
            
            if (enemyStats.isHurting)
            {
                agent.isStopped = true;
                if (anim != null) anim.SetFloat("Speed", 0f);
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        float distanceToHome = Vector3.Distance(transform.position, startPosition);
        bool isTooFarFromHome = distanceToHome > maxChaseRange;

        if (senses != null && senses.canSeePlayer && !isTooFarFromHome)
        {
            agent.speed = runSpeed;

            if (distanceToPlayer > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);

                if (anim != null)
                {
                    anim.SetFloat("Speed", agent.velocity.magnitude);
                }
            }
            else
            {
                agent.isStopped = true;
                LookAtTarget();

                if (anim != null)
                {
                    anim.SetFloat("Speed", 0f);
                }

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                }
            }
        }
        else if (senses != null && senses.canHearPlayer && !isTooFarFromHome)
        {
            agent.speed = runSpeed;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            LookAtTarget();

            if (anim != null)
            {
                anim.SetFloat("Speed", agent.velocity.magnitude);
            }
        }
        else
        {
            if (patrolScript != null)
            {
                if (isTooFarFromHome)
                {
                    if (agent.isStopped) agent.isStopped = false; 
                    
                    patrolScript.ReturnToStart();
                    if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
                }
                else
                {
                     if (agent.isStopped) agent.isStopped = false;

                    patrolScript.DoPatrolLogic();
                    if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
                }
            }
            else
            {
                agent.isStopped = true;
                if (anim != null) anim.SetFloat("Speed", 0f);
            }
        }
    }

    protected void LookAtTarget()
    {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    public abstract void Attack();

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, maxChaseRange);
    }
}
