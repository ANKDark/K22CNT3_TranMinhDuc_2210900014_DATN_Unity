using UnityEngine;
using Unity.Cinemachine;
public class PersistentUI : MonoBehaviour
{
    public static PersistentUI instance;

    [Header("UI")]
    public GameObject inventoryUI;
    public GameObject canvasUI;
    public GameObject menuDeadUI;
    public GameObject inventoryToggleUI;

    [Header("Skill Slots")]
    public SkillSlotUI uiSlotQ;
    public SkillSlotUI uiSlotE;
    public SkillSlotUI uiSlotR;

    [Header("Camera")]
    public Camera mainCamera;
    public CinemachineCamera vCam;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasUI != null)
                canvasUI.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowCanvas()
    {
        if (canvasUI != null && !canvasUI.activeInHierarchy)
        {
            canvasUI.SetActive(true);
        }
    }

    public void HideCanvas()
    {
        if (canvasUI != null && canvasUI.activeInHierarchy)
        {
            canvasUI.SetActive(false);
        }
    }

    public void ShowCamera()
    {
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(true);

        if (vCam != null)
            vCam.gameObject.SetActive(true);
    }

    public void HideCamera()
    {
        if (mainCamera != null)
            mainCamera.gameObject.SetActive(false);

        if (vCam != null)
            vCam.gameObject.SetActive(false);
    }

}
