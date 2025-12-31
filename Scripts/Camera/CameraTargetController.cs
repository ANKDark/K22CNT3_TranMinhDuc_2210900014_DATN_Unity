using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{
    private CinemachineCamera vCam;
    private InputSystem_Actions inputActions;
    private CinemachineInputAxisController axisController;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        vCam = GetComponent<CinemachineCamera>();
        inputActions = new InputSystem_Actions();

        axisController = GetComponent<CinemachineInputAxisController>();
        if (axisController != null)
        {
            var lookActionRef = InputActionReference.Create(inputActions.Player.Look);

            for (int i = 0; i < axisController.Controllers.Count; i++)
            {
                axisController.Controllers[i].Input.InputAction = lookActionRef;
            }
        }
    }

    public void SetSensitivity(float value)
    {
        if (axisController != null)
        {
            foreach (var controller in axisController.Controllers)
            {
                controller.Input.Gain = value;
            }
        }
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    void Update()
    {
        if (vCam.Follow == null || vCam.LookAt == null)
        {
            FindPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform cameraTarget = player.transform.Find("CameraTarget");
        if (cameraTarget == null)
        {
            cameraTarget = player.transform;
        }

        vCam.Follow = cameraTarget;
        vCam.LookAt = cameraTarget;
    }
}