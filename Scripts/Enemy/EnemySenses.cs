using UnityEngine;

public class EnemySenses : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 90f;
    [Header("Hearing Settings")]
    public float hearingRadius = 15f;
    public bool canHearPlayer = false;

    [Header("Layer Masks")]
    public LayerMask targetMask;
    public LayerMask groundMask;

    [Header("References")]
    public Transform playerTarget;
    public bool canSeePlayer = false;
    void Start()
    {
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }
        InvokeRepeating("UpdateSences", 0f, 0.2f);
    }

    void UpdateSences()
    {
        if (playerTarget == null)
            return;

        CheckVision();
        CheckHearing();
    }

    void CheckVision()
    {
        canSeePlayer = false;

        float distanceToTarget = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToTarget <= viewRadius)
        {
            Vector3 dirToTarget = (playerTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, dirToTarget, distanceToTarget, groundMask))
                {
                    canSeePlayer = true;
                }
            }
        }
    }

    void CheckHearing()
    {
        canHearPlayer = false;

        if (playerTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance > hearingRadius)
            return;

        PlayerMovement playerMovement = playerTarget.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        if (!playerMovement.IsMoving)
            return;

        float effectiveHearingRadius = hearingRadius;

        if (playerMovement.IsRunning)
        {
            effectiveHearingRadius *= 1.3f;
        }
        else
        {
            effectiveHearingRadius *= 0.6f;
        }

        if (distance <= effectiveHearingRadius)
        {
            canHearPlayer = true;
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        if (canSeePlayer && playerTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }
}
