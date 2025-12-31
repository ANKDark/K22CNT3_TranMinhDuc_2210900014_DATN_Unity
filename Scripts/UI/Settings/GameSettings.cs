using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public Slider sensitivitySlider;

    private CameraTargetController cameraController;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<CameraTargetController>();

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }

        if (cameraController != null)
        {
            cameraController.SetSensitivity(savedSensitivity);
        }
    }

    public void SetMouseSensitivity(float value)
    {
        if (cameraController != null)
        {
            cameraController.SetSensitivity(value);
        }

        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }
}
