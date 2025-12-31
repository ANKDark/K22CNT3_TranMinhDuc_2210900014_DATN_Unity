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
    [HideInInspector]
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
    }

    void Update()
    {
        if (isPlayerDead)
            return;

        RestoreManaOverTime();
        RestoreStaminaOverTime();
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible || isPlayerDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerHit?.Invoke();

        if (currentHealth > 0)
        {
            StartCoroutine(HitRecoveryRoutine());
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
        currentMana = Mathf.Min(maxMana, currentMana + 1f * Time.deltaTime);
        OnManaChanged?.Invoke(currentMana, maxMana);
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
