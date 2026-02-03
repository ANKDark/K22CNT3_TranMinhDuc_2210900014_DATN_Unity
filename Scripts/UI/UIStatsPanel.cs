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
                    strengthText.text = $"Sức mạnh: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Critical:
                    agilityText.text = $"Chí mạng: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Defense:
                    intellectText.text = $"Phòng thủ: {attr.value.ModifiedValue}";
                    break;
                case Attributes.Stamina:
                    staminaText.text = $"Thể lực: {attr.value.ModifiedValue}";
                    break;
            }
        }
    }
}
