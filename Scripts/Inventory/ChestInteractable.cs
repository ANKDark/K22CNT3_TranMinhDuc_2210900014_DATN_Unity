using UnityEngine;

public class ChestInteractable : MonoBehaviour
{
    [Header("Input System Settings")]
    private InputSystem_Actions inputActions;

    [Header("Settings")]
    public InventoryObject chestData;
    public Transform lidObject;
    private GameObject pressFPopup;

    [Header("UI Positioning")]
    [SerializeField]
    private float textHeightOffset = 0.75f;
    private Camera mainCamera;

    [Header("Animation")]
    public float openAngle = -100f;
    public float smooth = 5f;

    [Header("VFX Settings")]
    public GameObject vfxObject;
    public float dissolveDuration = 1.0f;

    [Header("Audio Settings")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip destroySound;
    private AudioSource _audioSource;
    private bool isOpen = false;
    private bool isPlayerNear = false;
    private bool isDestroying = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    [Header("Lock Settings")]
    public GameObject enemyCheck;
    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        if (chestData != null)
        {
            chestData = Instantiate(chestData);
        }
        inputActions = new InputSystem_Actions();
        GenerateUniqueSavePath();
    }

    private void GenerateUniqueSavePath()
    {
        if (chestData == null || string.IsNullOrEmpty(chestData.savePath)) return;

        string extension = System.IO.Path.GetExtension(chestData.savePath);
        string basePath = chestData.savePath.Substring(0, chestData.savePath.Length - extension.Length);

        string posID = $"{Mathf.RoundToInt(transform.position.x * 100)}_{Mathf.RoundToInt(transform.position.y * 100)}_{Mathf.RoundToInt(transform.position.z * 100)}";

        chestData.savePath = $"{basePath}_{posID}{extension}";
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (lidObject != null)
        {
            closedRot = lidObject.localRotation;
            openRot = Quaternion.Euler(openAngle, 0, 0);
        }

        if (pressFPopup == null)
        {
            if (PersistentUI.instance != null && PersistentUI.instance.canvasUI != null)
            {
                pressFPopup = PersistentUI.instance.canvasUI.transform.Find(
                    "TxtPressChest"
                )?.gameObject;
            }
        }

        mainCamera = Camera.main;
        if (pressFPopup != null)
            pressFPopup.SetActive(false);

        if (chestData != null && System.IO.File.Exists(Application.persistentDataPath + chestData.savePath))
        {
            chestData.Load();
        }
        else if (chestData != null)
        {
            chestData.InitializeBuffs();
        }

        CheckIfEmptyAndDestroy();
    }

    void OnEnable()
    {
        if (inputActions != null) inputActions.Player.Enable();
    }

    void OnDisable()
    {
        if (pressFPopup != null)
        {
            pressFPopup.SetActive(false);
        }
        if (inputActions != null) inputActions.Player.Disable();
    }

    void Update()
    {
        if (enemyCheck != null)
        {
            if (myCollider.enabled) myCollider.enabled = false;
        }
        else if (!isDestroying && !myCollider.enabled)
        {
            myCollider.enabled = true;
        }

        if (lidObject != null)
        {
            Quaternion target = isOpen ? openRot : closedRot;
            lidObject.localRotation = Quaternion.Slerp(
                lidObject.localRotation,
                target,
                Time.deltaTime * smooth
            );
        }

        if (isPlayerNear && inputActions != null && inputActions.Player.Interact.WasPressedThisFrame())
        {
            isOpen = !isOpen;
            if (isOpen)
            {
                if (_audioSource != null && openSound != null)
                    _audioSource.PlayOneShot(openSound);
                if (ChestUI.Instance != null)
                    ChestUI.Instance.OpenChestUI(chestData);
                if (pressFPopup != null)
                    pressFPopup.SetActive(false);
            }
            else
            {
                if (_audioSource != null && closeSound != null)
                    _audioSource.PlayOneShot(closeSound);
                if (ChestUI.Instance != null)
                    ChestUI.Instance.CloseChestUI();
                CheckIfEmptyAndDestroy();
                if (pressFPopup != null)
                    pressFPopup.SetActive(true);
            }
        }

        if (isOpen && ChestUI.Instance != null && !ChestUI.Instance.chestPanelRoot.activeSelf)
        {
            isOpen = false;
            if (_audioSource != null && closeSound != null)
                _audioSource.PlayOneShot(closeSound);
            CheckIfEmptyAndDestroy();
        }
    }

    void LateUpdate()
    {
        if (pressFPopup != null && isPlayerNear)
        {
            Vector3 targetPosition = transform.position + Vector3.up * textHeightOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);
            pressFPopup.transform.position = screenPosition;

            if (screenPosition.z > 0)
            {
                if (!pressFPopup.activeSelf)
                    pressFPopup.SetActive(true);
            }
            else
            {
                if (pressFPopup.activeSelf)
                    pressFPopup.SetActive(false);
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (!isOpen && pressFPopup != null)
                pressFPopup.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            isOpen = false;
            if (pressFPopup != null)
                pressFPopup.SetActive(false);
            if (ChestUI.Instance != null && ChestUI.Instance.chestPanelRoot.activeSelf)
                ChestUI.Instance.CloseChestUI();
            CheckIfEmptyAndDestroy();
        }
    }

    void CheckIfEmptyAndDestroy()
    {
        if (isDestroying)
            return;

        if (chestData.IsEmpty())
        {
            chestData.Save();
            isDestroying = true;

            if (pressFPopup != null)
                pressFPopup.SetActive(false);
            GetComponent<Collider>().enabled = false;

            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, transform.position);
            }
            ;

            if (vfxObject != null)
                Instantiate(vfxObject, transform.position, Quaternion.identity);
            StartCoroutine(DissolveItemRoutine(gameObject));
        }
    }

    System.Collections.IEnumerator DissolveItemRoutine(GameObject targetItem)
    {
        Renderer[] renderers = targetItem.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            float time = 0f;

            while (time < dissolveDuration)
            {
                time += Time.deltaTime;
                float dissolveValue = Mathf.Lerp(0.3f, 1.1f, time / dissolveDuration);
                foreach (var rend in renderers)
                {
                    rend.material.SetFloat("_DissolveAmount", dissolveValue);
                }
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }
        if (pressFPopup != null)
            pressFPopup.SetActive(false);
        Destroy(targetItem);
    }
}
