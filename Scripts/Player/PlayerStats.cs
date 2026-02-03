using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina (From Attributes)")]
    public float currentStamina;

    [Header("Action Status")]
    public bool isActing = false;

    [Header("Mana")]
    public float maxMana = 50f;
    public float currentMana;

    [Header("Status")]
    public bool isHurting = false;
    public float hitRecoveryTime = 0.5f;

    [Header("Regeneration")]
    public float baseManaRegen = 1f;
    private float currentManaMultiplier = 1f;

    
    [Header("Audio")]
    public AudioClip hurtSound;
    private AudioSource audioSource;

    
    public bool isInvincible = false;

    [HideInInspector]
    public bool isPlayerDead = false;
    public event Action<bool> OnPlayerDead;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action OnPlayerHit;

    private InventoryPlayer inventoryPlayer;

    public float maxStamina
    {
        get
        {
            if (inventoryPlayer != null)
                return inventoryPlayer.GetAttributeValue(Attributes.Stamina);
            return 50f;
        }
    }

    private void Awake()
    {
        inventoryPlayer = GetComponent<InventoryPlayer>();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        audioSource = GetComponentInChildren<AudioSource>();


    }

    void Update()
    {
        if (isPlayerDead)
            return;

        RestoreManaOverTime();
        RestoreStaminaOverTime();
    }

    public void TakeDamage(float amount, bool isCritical = false)
    {
        if (isInvincible || isPlayerDead) return;
        float def = inventoryPlayer.GetAttributeValue(Attributes.Defense);
        def = Mathf.Max(0, def);
        float finalDamage = amount * (100 / (100f + def));
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerHit?.Invoke();

        if (currentHealth > 0)
        {
            StartCoroutine(HitRecoveryRoutine());
            if (audioSource != null && hurtSound != null)
            {
                audioSource.PlayOneShot(hurtSound);
            }
        }


        if (currentHealth <= 0)
        {
            isPlayerDead = true;
            OnPlayerDead?.Invoke(isPlayerDead);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Max(0, currentMana - amount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void RestoreManaOverTime()
    {
        float regenAmount = (baseManaRegen * currentManaMultiplier) * Time.deltaTime;
        currentMana = Mathf.Min(maxMana, currentMana + regenAmount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void HealOverTime(float amount, float duration)
    {
        StartCoroutine(HealOverTimeRoutine(amount, duration));
    }

    private IEnumerator HealOverTimeRoutine(float amount, float duration)
    {
        float timer = 0f;
        float startHealth = currentHealth;
        // Logic: Add 'amount' over 'duration' seconds. 
        // We will add per frame.
        float rate = amount / duration;

        while (timer < duration)
        {
            if (isPlayerDead) yield break;

            float healTick = rate * Time.deltaTime;
            Heal(healTick); 
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void BuffManaRegen(float multiplier, float duration)
    {
        StartCoroutine(BuffManaRoutine(multiplier, duration));
    }

    private IEnumerator BuffManaRoutine(float multiplier, float duration)
    {
        currentManaMultiplier = multiplier;
        Debug.Log($"<color=blue>Mana Regen x{multiplier} started!</color>");
        yield return new WaitForSeconds(duration);
        currentManaMultiplier = 1f;
        Debug.Log($"<color=blue>Mana Regen ended.</color>");
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            return true;
        }
        return false;
    }

    public void ResetAction()
    {
        isActing = false;
    }

    public void RestoreStaminaOverTime()
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + 5f * Time.deltaTime);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    IEnumerator HitRecoveryRoutine()
    {
        isHurting = true;
        yield return new WaitForSeconds(hitRecoveryTime);
        isHurting = false;
    }
}
