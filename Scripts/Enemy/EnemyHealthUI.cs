using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Health Bar UI")]
    public Canvas enemyCanvas;
    public Image imageFillHealth;

    [Header("Target")]
    public EnemyStats enemyStats;

    private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;

        if (enemyStats == null) 
            enemyStats = GetComponentInParent<EnemyStats>();

        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged += UpdateHealthUI;

            UpdateHealthUI(enemyStats.currentHealth, enemyStats.maxHealth);
        }
    }

    void UpdateHealthUI(float current, float max)
    {
        if (imageFillHealth != null)
        {
            imageFillHealth.fillAmount = current / max;
        }
    }

    void LateUpdate()
    {
        if (enemyCanvas != null && mainCamera != null)
        {
           enemyCanvas.transform.rotation = Quaternion.LookRotation(enemyCanvas.transform.position - mainCamera.transform.position);
        }
    }

    void OnDestroy()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged -= UpdateHealthUI;
        }
    }
}
