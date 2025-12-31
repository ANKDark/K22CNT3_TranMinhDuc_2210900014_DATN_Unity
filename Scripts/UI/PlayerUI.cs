using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public InventoryPlayer inventoryPlayer;

    [Header("UI References")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    public Slider staminaBar;

    private void Start()
    {
        FindAndBindPlayer();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnManaChanged -= UpdateManaUI;
            playerStats.OnStaminaChanged -= UpdateStaminaUI;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndBindPlayer();
    }

    private void FindAndBindPlayer()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnManaChanged -= UpdateManaUI;
            playerStats.OnStaminaChanged -= UpdateStaminaUI;
        }

        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }

        if (inventoryPlayer == null)
        {
            inventoryPlayer = FindAnyObjectByType<InventoryPlayer>();
        }

        if (playerStats != null && inventoryPlayer != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
            playerStats.OnManaChanged += UpdateManaUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;

            UpdateHealthUI(playerStats.currentHealth, playerStats.maxHealth);
            UpdateManaUI(playerStats.currentMana, playerStats.maxMana);
            UpdateStaminaUI(playerStats.currentStamina, playerStats.maxStamina);
        }
    }

    private void UpdateHealthUI(float current, float max)
    {
        if (healthBar != null)
            healthBar.value = current;
        if (healthBar != null)
            healthBar.maxValue = max;
        if (healthText != null)
            healthText.text = $"{current:0}/{max:0}";
    }

    private void UpdateManaUI(float current, float max)
    {
        if (manaBar != null)
            manaBar.value = current;
        if (manaBar != null)
            manaBar.maxValue = max;
        if (manaText != null)
            manaText.text = $"{current:0}/{max:0}";
    }

    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaBar != null)
            staminaBar.value = current;
        if (staminaBar != null)
            staminaBar.maxValue = max;
    }
}
