using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    public static ChestUI Instance;

    [Header("References")]
    public GameObject chestPanelRoot;
    public Transform slotsGrid;
    public GameObject chestSlotPrefab;

    [Header("Runtime Data")]
    [HideInInspector]
    public ChestSlotUI hoveredSlot;
    private InventoryObject currentChestData;
    private InventoryObject playerInventory;

    [Header("Audio")]
    public AudioClip takeItemSound;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        chestPanelRoot.SetActive(false);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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

        Canvas canvas = GetComponentInParent<Canvas>();
        if (
            canvas != null
            && canvas.renderMode == RenderMode.ScreenSpaceCamera
            && canvas.worldCamera == null
        )
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void FindPlayer()
    {
        var playerMgr = FindFirstObjectByType<InventoryPlayer>();
        if (playerMgr != null)
            playerInventory = playerMgr.inventory;
    }

    private void Update()
    {
        if (chestPanelRoot.activeSelf)
        {
            if (
                Input.GetMouseButtonDown(0)
                && hoveredSlot != null
                && hoveredSlot.slotData.ItemObject != null
            )
            {
                TakeItemFromChest(hoveredSlot);
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                CloseChestUI();
            }
        }
    }

    public void OpenChestUI(InventoryObject data)
    {
        chestPanelRoot.SetActive(true);

        if (playerInventory == null)
            FindPlayer();

        currentChestData = data;

        RenderSlots();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseChestUI()
    {
        chestPanelRoot.SetActive(false);
        currentChestData = null;
        hoveredSlot = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void RenderSlots()
    {
        foreach (Transform child in slotsGrid)
            Destroy(child.gameObject);

        foreach (var slot in currentChestData.GetSlots)
        {
            var newSlotObj = Instantiate(chestSlotPrefab, slotsGrid);
            var slotLogic = newSlotObj.GetComponent<ChestSlotUI>();
            slot.slotDisplay = newSlotObj;
            slot.inventory = currentChestData;
            slotLogic.Init(slot, this);
        }
    }

    private void TakeItemFromChest(ChestSlotUI uiSlot)
    {
        Item item = uiSlot.slotData.item;
        int amount = uiSlot.slotData.amount;

        if (audioSource != null && takeItemSound != null)
        {
            audioSource.PlayOneShot(takeItemSound);
        }

        if (playerInventory.AddItem(item, amount))
        {
            uiSlot.slotData.UpdateSlot(new Item(), 0);
            uiSlot.UpdateSlotVisual();
        }
    }
}
