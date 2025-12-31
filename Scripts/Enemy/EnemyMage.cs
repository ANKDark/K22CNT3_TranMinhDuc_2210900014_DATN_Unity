using UnityEngine;
using System.Collections;

public class EnemyMage : EnemyBase
{
    [Header("Skill Settings")]
    public float castTime = 2f;
    public float skillRadius = 3f;

    [Header("VFX Prefabs")]
    public GameObject warningVFX;
    public GameObject explosionVFX;
    public AudioClip castSound;
    public AudioClip explosionSound;

    private bool isCasting = false;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        if (isCasting)
        {
            agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);

            LookAtTarget();
            return;
        }

        base.Update();
    }

    public override void Attack()
    {
        if (isCasting) return;
        
        StartCoroutine(CastSkillRoutine());
    }

    IEnumerator CastSkillRoutine()
    {
        isCasting = true;
        lastAttackTime = Time.time;

        if (anim != null) anim.SetTrigger("Attack");

        if (castSound != null) audioSource.PlayOneShot(castSound);

        Vector3 targetPosition = target.position;

        if (Physics.Raycast(targetPosition + Vector3.up, Vector3.down, out RaycastHit hit, 5f)) 
        {
            targetPosition = hit.point + Vector3.up * 0.1f;
        }

        GameObject warningObj = Instantiate(warningVFX, targetPosition, Quaternion.identity);
        Destroy(warningObj, castTime);

        yield return new WaitForSeconds(castTime);

        if(explosionVFX != null) Instantiate(explosionVFX, targetPosition, Quaternion.identity);
        if(explosionSound != null) audioSource.PlayOneShot(explosionSound);

        Collider[] hitColliders = Physics.OverlapSphere(targetPosition, skillRadius);
        bool hitPlayer = false;
        
        foreach (var hitCol in hitColliders)
        {
            if (hitCol.CompareTag("Player"))
            {
                PlayerStats pStats = hitCol.GetComponent<PlayerStats>();
                if (pStats != null)
                {
                    pStats.TakeDamage(damage);
                    hitPlayer = true;
                }
            }
        }

        if (hitPlayer) Debug.Log("Mage đánh trúng Player!");
        else Debug.Log("Player đã né được chiêu!");

        isCasting = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
    }
}
