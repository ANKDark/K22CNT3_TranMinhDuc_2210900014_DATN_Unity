using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [Header("Input System Settings")]
    private InputSystem_Actions inputActions;

    [Header("Skill Data")]
    public SkillData skillQData;
    public SkillData skillEData;
    public SkillData skillRData;

    [Header("UI References")]
    private SkillSlotUI skillSlotQ;
    private SkillSlotUI skillSlotE;
    private SkillSlotUI skillSlotR;

    [Header("Components")]
    private Animator anim;
    private PlayerStats playerStats;
    private InventoryPlayer inventoryPlayer;

    [SerializeField]
    private AudioSource audioSource;
    private bool isQOnCooldown = false;
    private bool isEOnCooldown = false;
    private bool isROnCooldown = false;
    private List<ItemBuff> activeBuffs = new List<ItemBuff>();
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        inventoryPlayer = GetComponent<InventoryPlayer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (skillSlotQ == null && PersistentUI.instance != null)
        {
            skillSlotQ = PersistentUI.instance.uiSlotQ;
        }

        if (skillSlotE == null && PersistentUI.instance != null)
        {
            skillSlotE = PersistentUI.instance.uiSlotE;
        }

        if (skillSlotR == null && PersistentUI.instance != null)
        {
            skillSlotR = PersistentUI.instance.uiSlotR;
        }
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Player.Disable();
    }

    void Update()
    {
        if (playerStats.isActing) return;

        if (playerStats != null && playerStats.isPlayerDead)
            return;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() || playerStats.currentHealth <= 0) return;

        if (playerStats.isHurting) return;

        if (inputActions != null && inputActions.Player.Skill1.WasPressedThisFrame() && !isQOnCooldown)
        {
            CastSkillQ();
        }
        if (inputActions != null && inputActions.Player.Skill2.WasPressedThisFrame() && !isEOnCooldown)
        {
            CastSkillE();
        }
        if (inputActions != null && inputActions.Player.Skill3.WasPressedThisFrame() && !isROnCooldown)
        {
            CastSkillR();
        }
    }

    private bool TryUseMana(int cost)
    {
        if (playerStats != null && playerStats.currentMana >= cost)
        {
            playerStats.UseMana(cost);
            return true;
        }
        Debug.Log("Không đủ năng lượng!");
        return false;
    }

    private void CastSkillQ()
    {
        if (!TryUseMana(skillQData.manaCost))
            return;

        playerStats.isActing = true;
        anim.SetTrigger("SkillQ");
        skillSlotQ.StartCooldown(skillQData.cooldownTime);
        StartCoroutine(CooldownRoutine(skillQData.cooldownTime, (val) => isQOnCooldown = val));

        StartCoroutine(SpawnSkillQRoutine(skillQData.spawnDelay));
    }

    IEnumerator SpawnSkillQRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (playerStats.isHurting || playerStats.isPlayerDead) yield break;

        int baseStrength = 0;
        if (inventoryPlayer != null)
            baseStrength = inventoryPlayer.GetAttributeValue(Attributes.Strength);

        float weaponBaseDamage = 15f;
        float totalDamage = (weaponBaseDamage + baseStrength) * skillQData.damageMultiplier;

        if (skillQData.vfxPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * 2.0f + Vector3.up * 0.8f;

            GameObject slashObj = Instantiate(skillQData.vfxPrefab, spawnPos, transform.rotation);

            ProjectileDamage proj = slashObj.GetComponent<ProjectileDamage>();
            if (proj != null)
            {
                proj.damage = totalDamage;
                proj.speed = 8.5f;
                proj.isSingleTarget = true;

                if (proj.speed > 0)
                    proj.lifeTime = skillQData.range / proj.speed;
                else
                    proj.lifeTime = 2f;
            }
        }
        if (skillQData.sfxSound != null && audioSource != null)
            audioSource.PlayOneShot(skillQData.sfxSound);
    }

    private void CastSkillE()
    {
        if (!TryUseMana(skillEData.manaCost))
            return;

        playerStats.isActing = true;
        anim.SetTrigger("SkillE");
        skillSlotE.StartCooldown(skillEData.cooldownTime);
        StartCoroutine(CooldownRoutine(skillEData.cooldownTime, (val) => isEOnCooldown = val));

        StartCoroutine(SpawnSkillERoutine(skillEData.spawnDelay));
    }

    IEnumerator SpawnSkillERoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (playerStats.isHurting || playerStats.isPlayerDead) yield break;

        int baseStrength = 0;
        if (inventoryPlayer != null)
        {
            baseStrength = inventoryPlayer.GetAttributeValue(Attributes.Strength);
        }

        float weaponBaseDamage = 25f;
        float totalDamage = (weaponBaseDamage + baseStrength) * skillEData.damageMultiplier;

        if (skillEData.vfxPrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1f + Vector3.up * 0f;

            Quaternion spawnRot = transform.rotation * skillEData.vfxPrefab.transform.rotation;

            GameObject crystalObj = Instantiate(skillEData.vfxPrefab, spawnPos, spawnRot);

            ProjectileDamage proj = crystalObj.GetComponent<ProjectileDamage>();

            if (proj != null)
            {
                proj.damage = totalDamage;
                proj.speed = 0f;
                proj.isSingleTarget = false;

                if (proj.speed > 0)
                    proj.lifeTime = skillEData.range / proj.speed;
                else
                    proj.lifeTime = 2f;
            }
        }
        if (skillEData.sfxSound != null && audioSource != null)
            audioSource.PlayOneShot(skillEData.sfxSound);
    }

    private void CastSkillR()
    {
        if (!TryUseMana(skillRData.manaCost))
            return;

        playerStats.isActing = true;
        anim.SetTrigger("SkillR");

        skillSlotR.StartCooldown(skillRData.cooldownTime);
        StartCoroutine(CooldownRoutine(skillRData.cooldownTime, (val) => isROnCooldown = val));

        StartCoroutine(SpawnSkillRRoutine(skillRData.spawnDelay));
    }

    IEnumerator SpawnSkillRRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (playerStats.isHurting || playerStats.isPlayerDead) yield break;

        if (skillRData.vfxPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0f;

            GameObject vfxBuffObj = Instantiate(
                skillRData.vfxPrefab,
                spawnPos,
                Quaternion.identity,
                transform
            );

            List<ItemBuff> tempBuffs = new List<ItemBuff>();
            if (inventoryPlayer != null && inventoryPlayer.attributes != null)
            {
                foreach (var attr in inventoryPlayer.attributes)
                {
                    int currentVal = attr.value.ModifiedValue;
                    int buffVal = Mathf.CeilToInt(currentVal * skillRData.damageMultiplier);

                    if (buffVal > 0)
                    {
                        ItemBuff buff = new ItemBuff(buffVal, buffVal);
                        buff.attribute = attr.type;
                        buff.value = buffVal;

                        attr.value.AddModifier(buff);
                        tempBuffs.Add(buff);
                    }
                }
            }

            playerStats.isInvincible = true;
            yield return new WaitForSeconds(skillRData.duration / 2f);

            playerStats.isInvincible = false;

            yield return new WaitForSeconds(skillRData.duration);

            if (inventoryPlayer != null)
            {
                foreach (var buff in tempBuffs)
                {
                    foreach (var attr in inventoryPlayer.attributes)
                    {
                        if (attr.type == buff.attribute)
                        {
                            attr.value.RemoveModifier(buff);
                            break;
                        }
                    }
                    activeBuffs.Remove(buff);
                }
            }
            Destroy(vfxBuffObj);
        }
        if (skillRData.sfxSound != null && audioSource != null)
            audioSource.PlayOneShot(skillRData.sfxSound);
    }

    private void OnDestroy()
    {
        if (inventoryPlayer != null && activeBuffs.Count > 0)
        {
            foreach (var buff in activeBuffs)
            {
                foreach (var attr in inventoryPlayer.attributes)
                {
                    if (attr.type == buff.attribute)
                    {
                        attr.value.RemoveModifier(buff);
                        break;
                    }
                }
            }
            activeBuffs.Clear();
        }
    }

    IEnumerator CooldownRoutine(float time, System.Action<bool> setCooldownState)
    {
        setCooldownState(true);
        yield return new WaitForSeconds(time);
        setCooldownState(false);
    }
}
