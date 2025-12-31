using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image cooldownOverlay;
    public Image iconSkill;
    public SkillData skillData;
    private float currentCooldownTime;
    private float maxCooldownTime;
    private bool isOnCooldown = false;

    void Start()
    {
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        if (iconSkill != null && skillData != null)
        {
            iconSkill.sprite = skillData.skillIcon;
        }
    }

    void Update()
    {
        if (isOnCooldown)
        {
            currentCooldownTime -= Time.deltaTime;
            if (cooldownOverlay != null && maxCooldownTime > 0)
            {
                cooldownOverlay.fillAmount = currentCooldownTime / maxCooldownTime;
            }

            if (currentCooldownTime <= 0)
            {
                isOnCooldown = false;
                currentCooldownTime = 0;
                if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
            }
        }
    }

    public void StartCooldown(float duration)
    {
        maxCooldownTime = duration;
        currentCooldownTime = duration;
        isOnCooldown = true;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 1;
    }
}
