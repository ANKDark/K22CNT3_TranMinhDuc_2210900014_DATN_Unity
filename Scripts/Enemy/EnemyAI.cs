using UnityEngine;
public class EnemyAI : EnemyBase
{
    [Header("Audio")]
    public AudioClip attackSound;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public override void Attack()
    {
        lastAttackTime = Time.time;
        if (anim != null) anim.SetTrigger("Attack");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void AnimationEvent_DealDamage()
    {
        if (target == null) return;

        if (targetStats == null || targetStats.isPlayerDead) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange + 0.5f)
        {
            targetStats.TakeDamage(damage);
        }
    }
}