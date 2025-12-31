using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("Canvas Inventory")]
    public GameObject inventoryCanvas;
    public GameObject menuDiedPanel;
    private bool isOpen = false;
    private PlayerStats playerStats;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (inputActions != null) inputActions.UI.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null) inputActions.UI.Disable();
    }

    void Start()
    {
        FindPlayer();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (inventoryCanvas != null)
            inventoryCanvas.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (inputActions != null) inputActions.UI.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        if (menuDiedPanel != null)
            menuDiedPanel.SetActive(false);
        if (inventoryCanvas != null)
            inventoryCanvas.SetActive(false);
        isOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FindPlayer()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (playerStats != null && playerStats.isPlayerDead)
        {
            if (isOpen)
            {
                isOpen = false;
                if (inventoryCanvas != null)
                    inventoryCanvas.SetActive(false);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (menuDiedPanel != null)
            {
                menuDiedPanel.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            return;
        }

        if (inputActions != null && inputActions.UI.Inventory.WasPressedThisFrame())
        {
            isOpen = !isOpen;

            if (inventoryCanvas != null)
                inventoryCanvas.SetActive(isOpen);

            if (isOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (inputActions != null && inputActions.UI.Cancel.WasPressedThisFrame())
        {
            if (playerStats != null && playerStats.isPlayerDead)
                return;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GameManager.Instance.BackToMenu();
        }
    }
}
