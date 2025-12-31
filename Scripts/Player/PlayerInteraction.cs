using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input System Settings")]
    private InputSystem_Actions inputActions;

    [Header("Dependencies")]
    public InventoryPlayer inventoryManager;

    [Header("Interaction Settings")]
    private ItemPickUp pickupPopup;
    private GroundItem itemInRange;

    [Header("UI Positioning")]
    [SerializeField]
    private float textHeightOffset = 0.75f;
    private Camera mainCamera;

    [Header("VFX Settings")]
    public GameObject pickupParticles;
    public float dissolveDuration = 1.0f;

    [Header("Audio Settings")]
    [SerializeField]
    private AudioSource _audioSource;
    public AudioClip pickupSound;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (inputActions != null) inputActions.Player.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null) inputActions.Player.Disable();
    }

    void Start()
    {
        mainCamera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (inventoryManager == null)
            inventoryManager = GetComponent<InventoryPlayer>();
        if (pickupPopup == null)
        {
            if (PersistentUI.instance != null && PersistentUI.instance.canvasUI != null)
            {
                pickupPopup = PersistentUI.instance.canvasUI.GetComponentInChildren<ItemPickUp>(
                    true
                );
            }
        }
        if (pickupPopup != null)
            pickupPopup.HidePickUp();
    }

    void Update()
    {
        if (itemInRange != null && inputActions != null && inputActions.Player.Interact.WasPressedThisFrame())
        {
            PickUpItem();
        }
    }

    private void LateUpdate()
    {
        if (pickupPopup != null && itemInRange != null)
        {
            Vector3 targetPosition = itemInRange.transform.position + Vector3.up * textHeightOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
            pickupPopup.transform.position = screenPosition;

            if (screenPosition.z > 0)
            {
                if (!pickupPopup.gameObject.activeSelf)
                    pickupPopup.gameObject.SetActive(true);
            }
            else
            {
                if (pickupPopup.gameObject.activeSelf)
                    pickupPopup.gameObject.SetActive(false);
            }
        }
        else if (pickupPopup != null && pickupPopup.gameObject.activeSelf)
        {
            pickupPopup.HidePickUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var groundItem = other.GetComponent<GroundItem>();
        if (groundItem != null)
        {
            itemInRange = groundItem;
            if (pickupPopup != null)
                pickupPopup.ShowPickUp(groundItem.item);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var groundItem = other.GetComponent<GroundItem>();
        if (groundItem != null && groundItem == itemInRange)
        {
            itemInRange = null;
            if (pickupPopup != null)
                pickupPopup.HidePickUp();
        }
    }

    private void PickUpItem()
    {
        if (itemInRange == null)
            return;
        if (inventoryManager == null)
            return;

        var uid = itemInRange.GetComponent<UniqueID>();
        GameObject itemObj = itemInRange.gameObject;
        
        Item _item;
        if (itemInRange.itemInstance != null && itemInRange.itemInstance.Id != -1)
        {
            _item = itemInRange.itemInstance;
        }
        else
        {
            _item = new Item(itemInRange.item);
        }

        if (inventoryManager.TryAddItem(_item))
        {
            if (uid != null && !string.IsNullOrEmpty(uid.uniqueId))
            {
                SaveData saveData = SaveSystem.Load();
                if (!saveData.collectedItems.Contains(uid.uniqueId))
                {
                    saveData.collectedItems.Add(uid.uniqueId);
                    SaveSystem.Save(saveData);
                }
            }
            if (pickupSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(pickupSound);
            }
            if (inventoryManager.inventory != null)
                inventoryManager.inventory.Save();
            if (inventoryManager.equipment != null)
                inventoryManager.equipment.Save();

            if (pickupParticles != null)
            {
                Instantiate(pickupParticles, itemObj.transform.position, Quaternion.identity);
            }

            Collider itemCol = itemObj.GetComponent<Collider>();
            if (itemCol != null)
                itemCol.enabled = false;

            if (pickupPopup != null)
                pickupPopup.HidePickUp();

            StartCoroutine(DissolveItemRoutine(itemObj));

            itemInRange = null;
        }
    }

    System.Collections.IEnumerator DissolveItemRoutine(GameObject targetItem)
    {
        Renderer itemRenderer = targetItem.GetComponentInChildren<Renderer>();

        if (itemRenderer != null)
        {
            float time = 0f;
            Material mat = itemRenderer.material;

            while (time < dissolveDuration)
            {
                time += Time.deltaTime;
                float dissolveValue = Mathf.Lerp(0.3f, 1.1f, time / dissolveDuration);
                mat.SetFloat("_DissolveAmount", dissolveValue);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }

        Destroy(targetItem);
    }
}
