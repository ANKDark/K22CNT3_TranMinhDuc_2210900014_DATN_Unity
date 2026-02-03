using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalController : MonoBehaviour
{
    public string sceneName = "Dungeon_B2";

    private bool isPlayerInside = false;
    private Collider playerCollider;
    private InputSystem_Actions inputActions;

   [SerializeField] private GameObject textHint;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerCollider = other;
            if (textHint != null)
                textHint.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerCollider = null;
            if (textHint != null)
                textHint.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerInside && textHint != null && textHint.activeSelf)
        {
            if (Camera.main != null)
            {
                textHint.transform.rotation = Camera.main.transform.rotation;
            }
        }

        if (isPlayerInside && inputActions != null && inputActions.Player.Interact.WasPressedThisFrame())
        {
            if (playerCollider != null)
            {
                EnterPortal();
            }
        }
    }
    private void EnterPortal()
    {
        InventoryPlayer invPlayer = playerCollider.GetComponent<InventoryPlayer>();
        if (invPlayer != null)
        {
            invPlayer.inventory.Save();
            invPlayer.equipment.Save();

            PlayerPrefs.SetInt("IsNewGame", 0);
            PlayerPrefs.SetInt("HasSaveGame", 1);
            PlayerPrefs.Save();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}