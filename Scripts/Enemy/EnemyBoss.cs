using UnityEngine;

public class EnemyBoss : EnemyBase
{
    [Header("Boss Settings")]
    public float rageHealthThreshold = 50f;
    private bool isEnraged = false;

    protected override void Update()
    {
        // Boss có thể có thêm logic riêng trong Update
        // Nhưng vẫn cần gọi base.Update() để di chuyển
        base.Update();

        // Kiểm tra chuyển phase
        if (!isEnraged && enemyStats != null && enemyStats.currentHealth < rageHealthThreshold)
        {
            EnterRageMode();
        }
    }

    private void EnterRageMode()
    {
        isEnraged = true;
        Debug.Log("BOSS ENRAGED!");
        // Tăng tốc, đổi màu, v.v...
        runSpeed *= 1.5f;
        damage *= 1.2f;
        if (anim != null) anim.SetTrigger("Rage");
    }

    public override void Attack()
    {
        lastAttackTime = Time.time;

        if (isEnraged)
        {
            // Skill đặc biệt khi nổi điên
            Debug.Log("Boss sử dụng kỹ năng ĐẶC BIỆT!");
            if (anim != null) anim.SetTrigger("AttackSpecial");
            
            // Ví dụ: Tạo vùng nổ xung quanh
            // Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
            // foreach(var h in hits) { ... }
        }
        else
        {
            // Đánh thường
            if (anim != null) anim.SetTrigger("Attack");
            Debug.Log("Boss đánh thường.");
            
            if (target != null)
            {
                PlayerStats p = target.GetComponent<PlayerStats>();
                if (p != null) p.TakeDamage(damage);
            }
        }
    }
}
