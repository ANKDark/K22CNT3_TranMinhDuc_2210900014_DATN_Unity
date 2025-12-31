using UnityEngine;
public class EnemyAI : EnemyBase
{
    public override void Attack()
    {
        lastAttackTime = Time.time;
        if (anim != null) anim.SetTrigger("Attack");
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