using UnityEngine;

public class EnemyBoss : EnemyBase
{
    public enum BossAction
    {
        None,
        Enrage,
        NormalAttack,
        Skill1,
        Skill2
    }

    [Header("Boss Phase")]
    public bool isEnraged = false;
    [SerializeField] private float enrageThreshold = 0.5f;
    [SerializeField] private GameObject enrageVFX;
    [SerializeField] private AudioClip enrageSound;

    [Header("Skill 1")]
    public AudioClip skill1Sound;
    public float skill1Cooldown = 8f;
    public float skill1DamageMult = 1.5f;
    public float skill1Radius = 3f;
    public GameObject skill1WarningVFX;
    public GameObject skill1HitVFX;

    [Header("Skill 2")]
    public AudioClip skill2Sound;
    public float skill2Cooldown = 12f;
    public float skill2DamageMult = 2.0f;
    public float skill2Radius = 6f;
    public float skill2ForwardOffset = 0f;
    public float skill2ProjectileSpeed = 15f;
    
    public float skill2VfxCorrectionOffset = -2.0f; 

    public GameObject skill2WarningVFX;
    public GameObject skill2HitVFX;

    [Header("VFX Settings")]
    public float skill1ForwardOffset = 1.5f;
    public float vfxGroundOffsetY = 0.05f;
    public float fallbackVfxLifetime = 2.5f;

    private float lastSkill1Time = -99f;
    private float lastSkill2Time = -99f;

    private bool isCasting = false;
    private bool hasTriggeredEnrage = false;

    private BossAction currentAction = BossAction.None;

    private Vector3 cachedSkill1ImpactPos;
    private Vector3 cachedSkill2CenterPos;

    private GameObject activeWarningObj;
    private GameObject activeImpactObj;

    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        CheckEnrage();

        if (isCasting)
        {
            if (agent != null) agent.isStopped = true;
            if (anim != null) anim.SetFloat("Speed", 0f);
            LookAtTarget(); 
            return;
        }

        base.Update();
    }

    private void CheckEnrage()
    {
        if (hasTriggeredEnrage || enemyStats == null) return;
        if (enemyStats.maxHealth <= 0f) return;

        float hp = enemyStats.currentHealth / enemyStats.maxHealth;
        if (hp <= enrageThreshold)
        {
            StartEnrage();
        }
    }

    private void StartEnrage()
    {
        hasTriggeredEnrage = true;
        isCasting = true;
        isEnraged = true;

        lastSkill1Time = -99f;
        lastSkill2Time = -99f;

        currentAction = BossAction.Enrage;

        if (anim != null) anim.SetTrigger("Enrage");
        if (enrageSound != null) audioSource.PlayOneShot(enrageSound);
        if (enrageVFX != null) SpawnAndAutoDestroy(enrageVFX, transform.position, Quaternion.identity);

        StartCoroutine(ActionTimeout(3.0f));
    }

    public override void Attack()
    {
        if (isCasting || target == null) return;
        if (targetStats != null && targetStats.isPlayerDead) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (isEnraged && Time.time >= lastSkill2Time + skill2Cooldown && dist <= skill2Radius)
        {
            StartSkill2();
            return;
        }

        if (Time.time >= lastSkill1Time + skill1Cooldown && dist <= skill1Radius + 1.5f)
        {
            StartSkill1();
            return;
        }

        StartNormalAttack();
    }

    private void StartNormalAttack()
    {
        isCasting = true;
        currentAction = BossAction.NormalAttack;
        lastAttackTime = Time.time;
        LookAtTarget();
        if (anim != null) anim.SetTrigger("Attack");
        
        StartCoroutine(ActionTimeout(3f));
    }

    private void StartSkill1()
    {
        isCasting = true;
        currentAction = BossAction.Skill1;

        lastSkill1Time = Time.time;
        lastAttackTime = Time.time + 1.2f; 

        LookAtTarget();
        cachedSkill1ImpactPos = transform.position + transform.forward * skill1ForwardOffset;

        if (anim != null) anim.SetTrigger("Skill1");
        if (skill1Sound != null && audioSource != null) audioSource.PlayOneShot(skill1Sound);
        
        StartCoroutine(ActionTimeout(5f));
    }

    private void StartSkill2()
    {
        isCasting = true;
        currentAction = BossAction.Skill2;

        lastSkill2Time = Time.time;
        lastAttackTime = Time.time + 2.0f;

        cachedSkill2CenterPos = transform.position + transform.forward * skill2ForwardOffset;

        if (anim != null) anim.SetTrigger("Skill2");
        if (skill2Sound != null && audioSource != null) audioSource.PlayOneShot(skill2Sound);
        
        StartCoroutine(ActionTimeout(6f));
    }

    public void AnimationEvent_NormalAttackEnd()
    {
        if (currentAction != BossAction.NormalAttack) return;
        Debug.Log("<color=white>[Boss Event]</color> Normal Attack End");
        ResetState();
    }

    public void AnimationEvent_BossNormalHit()
    {
        if (currentAction != BossAction.NormalAttack) return;
        if (target == null || targetStats == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange + 1f)
        {
            targetStats.TakeDamage(damage);
        }
    }

    public void AnimationEvent_EnrageEnd()
    {
        if (currentAction != BossAction.Enrage) return;
        Debug.Log("<color=red>[Boss Event]</color> Enrage End");

        runSpeed *= 1.3f;
        if (agent != null) agent.speed = runSpeed;
        damage *= 1.2f;

        ResetState();
    }

    public void AnimationEvent_Skill1_Warning()
    {
        if (currentAction != BossAction.Skill1) return;

        ClearVFX();

        if (skill1WarningVFX != null)
        {
            Vector3 pos = cachedSkill1ImpactPos + Vector3.up * vfxGroundOffsetY;
            Quaternion rot = Quaternion.LookRotation(transform.forward);
            activeWarningObj = SpawnAndAutoDestroy(skill1WarningVFX, pos, rot);
        }
    }

    public void AnimationEvent_Skill1_Impact()
    {
        if (currentAction != BossAction.Skill1) return;

        ClearVFX();

        if (skill1HitVFX != null)
        {
            activeImpactObj = SpawnAndAutoDestroy(skill1HitVFX, cachedSkill1ImpactPos, Quaternion.identity);
        }

        DealAreaDamage(cachedSkill1ImpactPos, skill1Radius, damage * skill1DamageMult);
    }

    public void AnimationEvent_Skill1_End()
    {
        if (currentAction != BossAction.Skill1) return;
        Debug.Log("<color=orange>[Boss Event]</color> Skill 1 End");
        ResetState();
    }

    public void AnimationEvent_Skill2_Warning()
    {
        if (currentAction != BossAction.Skill2) return;

        ClearVFX();

        if (skill2WarningVFX != null)
        {
            Vector3 correction = transform.forward * skill2VfxCorrectionOffset;
            Vector3 pos = cachedSkill2CenterPos + correction + Vector3.up * vfxGroundOffsetY;
            
            Quaternion rot = Quaternion.LookRotation(transform.forward);
            
            activeWarningObj = SpawnAndAutoDestroy(skill2WarningVFX, pos, rot);
        }
    }

    public void AnimationEvent_Skill2_Impact()
    {
        if (currentAction != BossAction.Skill2) return;

        ClearVFX();

        if (skill2HitVFX != null)
        {
            Vector3 startPos = transform.position + Vector3.up * 1.5f + transform.forward * 0.5f;

            Vector3 targetPos = cachedSkill2CenterPos + Vector3.up * 1.3f;

            GameObject projectile = Instantiate(skill2HitVFX, startPos, Quaternion.LookRotation(targetPos - startPos));
            
            StartCoroutine(ProcessSkill2Projectile(projectile, targetPos));
        }
    }

    private System.Collections.IEnumerator ProcessSkill2Projectile(GameObject projectile, Vector3 targetPos)
    {
        float speed = skill2ProjectileSpeed > 0 ? skill2ProjectileSpeed : 15f;
        float traveledDistance = 0f;
        float maxDistance = 10f; 
        
        Vector3 direction = transform.forward;
        
        if (projectile != null) projectile.transform.rotation = Quaternion.LookRotation(direction);

        while (projectile != null)
        {
            float step = speed * Time.deltaTime;
            
            projectile.transform.position += direction * step;
            traveledDistance += step;

            Collider[] hits = Physics.OverlapSphere(projectile.transform.position, 0.5f);
            bool hitSomething = false;

            foreach (var h in hits)
            {
                if (h.gameObject == gameObject || h.isTrigger) continue;

                if (h.CompareTag("Player"))
                {
                    PlayerStats ps = h.GetComponent<PlayerStats>();
                    if (ps != null && !ps.isPlayerDead)
                    {
                        ps.TakeDamage(damage * skill2DamageMult);
                    }
                    hitSomething = true;
                    break;
                }
                else if (h.CompareTag("Wall") || h.CompareTag("Environment") || h.gameObject.layer == LayerMask.NameToLayer("Default"))
                {
                    hitSomething = true;
                    break;
                }
            }

            if (hitSomething)
            {
                Destroy(projectile);
                yield break;
            }

            if (traveledDistance >= maxDistance)
            {
                Destroy(projectile);
                yield break;
            }

            yield return null;
        }
    }

    public void AnimationEvent_Skill2_End()
    {
        if (currentAction != BossAction.Skill2) return;
        Debug.Log("<color=orange>[Boss Event]</color> Skill 2 End");
        ResetState();
    }

    private void ResetState()
    {
        ClearVFX();
        currentAction = BossAction.None;
        isCasting = false;
    }

    private System.Collections.IEnumerator ActionTimeout(float delay)
    {
        BossAction actionAtStart = currentAction;
        yield return new WaitForSeconds(delay);
        
        if (isCasting && currentAction == actionAtStart)
        {
            Debug.LogWarning($"<color=yellow>[Boss Safeguard]</color> Action {actionAtStart} timed out! Resetting state automatically.");
            ResetState();
        }
    }

    private GameObject SpawnAndAutoDestroy(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject go = Instantiate(prefab, pos, rot);
        float life = fallbackVfxLifetime;

        ParticleSystem ps = go.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            float duration = main.duration;
            float startLife = main.startLifetime.mode == ParticleSystemCurveMode.Constant
                ? main.startLifetime.constant
                : main.startLifetime.constantMax;
            life = duration + startLife + 0.15f;
        }

        Destroy(go, Mathf.Max(0.25f, life));
        return go;
    }

    private void ClearVFX()
    {
        if (activeWarningObj != null)
        {
            Destroy(activeWarningObj);
            activeWarningObj = null;
        }

        if (activeImpactObj != null)
        {
            Destroy(activeImpactObj);
            activeImpactObj = null;
        }
    }

    private void DealAreaDamage(Vector3 center, float radius, float dmg)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Player")) continue;
            PlayerStats ps = h.GetComponent<PlayerStats>();
            if (ps != null && !ps.isPlayerDead)
            {
                ps.TakeDamage(dmg);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.forward * skill1ForwardOffset, skill1Radius);

        Gizmos.color = new Color(0.5f, 0f, 0.5f, 1f);
        Gizmos.DrawWireSphere(transform.position, skill2Radius);
    }
}
