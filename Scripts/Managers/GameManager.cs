using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool hasStarted = false;
    public Vector3 playerPosition;
    [HideInInspector] public string currentScene;
    public InventoryObject inventoryObject;

    private string inventorySnapshot;
    private string equipmentSnapshot;
    public bool isTeleporting = false;
    private bool isRestarting = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        hasStarted = PlayerPrefs.GetInt("HasStarted", 0) == 1;
        currentScene = PlayerPrefs.GetString("CurrentScene", "SampleScene");

        playerPosition = new Vector3(
            PlayerPrefs.GetFloat("px", -10.41003f),
            PlayerPrefs.GetFloat("py", 0.074f),
            PlayerPrefs.GetFloat("pz", -14.05812f)
        );
    }

    public void NewGame()
    {
        hasStarted = true;
        isRestarting = false;
        isTeleporting = false;

        PlayerPrefs.SetInt("HasStarted", 1);
        playerPosition = new Vector3(-10.41003f, 0.074f, -14.05812f);

        currentScene = "IntroScene";
        PlayerPrefs.SetString("CurrentScene", currentScene);
        PlayerPrefs.SetFloat("px", playerPosition.x);
        PlayerPrefs.SetFloat("py", playerPosition.y);
        PlayerPrefs.SetFloat("pz", playerPosition.z);
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.SetInt("HasSaveGame", 1);
        PlayerPrefs.DeleteKey("PlayerHealth");
        PlayerPrefs.DeleteKey("PlayerMana");
        PlayerPrefs.Save();

        SaveSystem.DeleteAllSaves();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(currentScene);
    }

    public void ContinueGame()
    {
        bool hasSave = PlayerPrefs.GetInt("HasSaveGame", 0) == 1;
        if (!hasSave)
            return;

        isRestarting = false;
        isTeleporting = false;
        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(currentScene);
    }

    public void RestartLevel()
    {
        InventoryPlayer invManager = FindFirstObjectByType<InventoryPlayer>();

        if (invManager != null)
        {
            isRestarting = true;
            isTeleporting = false;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void BackToMenu()
    {
        SaveGame();
        if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveGame()
    {
        InventoryPlayer invManager = FindFirstObjectByType<InventoryPlayer>();
        ChestInteractable[] chests = FindObjectsByType<ChestInteractable>(FindObjectsSortMode.None);

        if (invManager != null)
        {
            playerPosition = invManager.transform.position;
            PlayerPrefs.SetFloat("px", playerPosition.x);
            PlayerPrefs.SetFloat("py", playerPosition.y);
            PlayerPrefs.SetFloat("pz", playerPosition.z);
            
            currentScene = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("CurrentScene", currentScene);

            if (invManager.inventory != null) invManager.inventory.Save();
            if (invManager.equipment != null) invManager.equipment.Save();
            
            PlayerStats stats = invManager.GetComponent<PlayerStats>();
            if (stats != null)
            {
                PlayerPrefs.SetFloat("PlayerHealth", stats.currentHealth);
                PlayerPrefs.SetFloat("PlayerMana", stats.currentMana);
            }
        }

        foreach (var chest in chests)
        {
            if (chest.chestData != null) chest.chestData.Save();
        }

        PlayerPrefs.SetInt("HasSaveGame", 1);
        PlayerPrefs.Save();
    }

    private Coroutine autoSaveCoroutine;

    public void StartAutoSave()
    {
        if (autoSaveCoroutine != null) StopCoroutine(autoSaveCoroutine);
        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    private System.Collections.IEnumerator AutoSaveRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(30f);
        while (true)
        {
            yield return wait;
            SaveGame();
        }
    }

    public void LoadNextScene(string sceneName)
    {
        isTeleporting = true;

        currentScene = sceneName;
        PlayerPrefs.SetString("CurrentScene", sceneName);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InventoryPlayer invManager = FindFirstObjectByType<InventoryPlayer>();

        if (invManager != null)
        {
            if (!isTeleporting)
            {
                CharacterController cc = invManager.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                invManager.transform.position = playerPosition;
                if (cc != null) cc.enabled = true;
            }
            isTeleporting = false;

            if (isRestarting)
            {
                if (!string.IsNullOrEmpty(inventorySnapshot))
                {
                    JsonUtility.FromJsonOverwrite(inventorySnapshot, invManager.inventory);
                    invManager.inventory.RefreshInventory();
                }
                if (!string.IsNullOrEmpty(equipmentSnapshot))
                {
                    JsonUtility.FromJsonOverwrite(equipmentSnapshot, invManager.equipment);
                    invManager.equipment.RefreshInventory();
                }

                isRestarting = false;
                CreateCheckpoint(invManager);
            }
            else
            {
                bool isNewGame = PlayerPrefs.GetInt("IsNewGame", 0) == 1;

                if (isNewGame && !isRestarting)
                {
                    if (invManager.inventory != null)
                    {
                        invManager.inventory.Clear();
                        invManager.inventory.Save();
                    }

                    if (invManager.equipment != null)
                    {
                        invManager.equipment.Clear();
                        invManager.equipment.Save();
                    }

                    SaveData saveData = SaveSystem.Load();
                    saveData.collectedItems.Clear();
                    saveData.destroyedObjects.Clear();
                    SaveSystem.Save(saveData);


                }
                else
                {
                    if (invManager.inventory != null)
                        invManager.inventory.Load();

                    if (invManager.equipment != null)
                        invManager.equipment.Load();

                    PlayerStats stats = invManager.GetComponent<PlayerStats>();
                    if (stats != null)
                    {
                        if (PlayerPrefs.HasKey("PlayerHealth"))
                            stats.currentHealth = PlayerPrefs.GetFloat("PlayerHealth");
                        
                        if (PlayerPrefs.HasKey("PlayerMana"))
                            stats.currentMana = PlayerPrefs.GetFloat("PlayerMana");
                        
                        stats.Heal(0); 
                        stats.RestoreMana(0);
                    }
                }
                CreateCheckpoint(invManager);
            }
            PlayerEquipmentVisuals visuals = invManager.GetComponent<PlayerEquipmentVisuals>();

            if (visuals != null)
            {
                visuals.RestoreEquippedItems();
            }

            StartAutoSave();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CreateCheckpoint(InventoryPlayer invManager)
    {
        if (invManager.inventory != null)
            inventorySnapshot = JsonUtility.ToJson(invManager.inventory);

        if (invManager.equipment != null)
            equipmentSnapshot = JsonUtility.ToJson(invManager.equipment);

        playerPosition = invManager.transform.position;

        PlayerPrefs.SetFloat("px", playerPosition.x);
        PlayerPrefs.SetFloat("py", playerPosition.y);
        PlayerPrefs.SetFloat("pz", playerPosition.z);
        currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("CurrentScene", currentScene);
        PlayerPrefs.Save();
    }
}
