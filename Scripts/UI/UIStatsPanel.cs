using TMPro;
using UnityEngine;

public class UIStatsPanel : MonoBehaviour
{
    public InventoryPlayer player;

    [Header("Text Components")]
    public TMP_Text strengthText;
    public TMP_Text agilityText;
    public TMP_Text intellectText;
    public TMP_Text staminaText;

    private void Start()
    {
        FindPlayer();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode
    )
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        player = FindFirstObjectByType<InventoryPlayer>();
    }

    private void Update()
    {
        if (player == null || player.attributes == null)
            return;

        foreach (var attr in player.attributes)
        {
            switch (attr.type)
            {
                case Attributes.Strength:
                    strengthText.text = $"Strength: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Agility:
                    agilityText.text = $"Agility: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Intellect:
                    intellectText.text = $"Intellect: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Stamina:
                    staminaText.text = $"Stamina: {attr.value.ModifiedValue}";
                    break;
            }
        }
    }
}
