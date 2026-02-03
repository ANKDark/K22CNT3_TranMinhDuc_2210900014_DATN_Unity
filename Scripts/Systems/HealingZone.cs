using System.Collections;
using UnityEngine;

public class HealingZone : MonoBehaviour
{
    [Header("Settings")]
    public float healRate = 20f;
    public float manaRate = 10f;

    private PlayerStats playerStats;
    private Coroutine healingCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                if (healingCoroutine != null) StopCoroutine(healingCoroutine);
                healingCoroutine = StartCoroutine(HealRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
                healingCoroutine = null;
                playerStats = null;
            }
        }
    }

    private IEnumerator HealRoutine()
    {
        while (playerStats != null)
        {
            if (playerStats.currentHealth < playerStats.maxHealth)
            {
                playerStats.Heal(healRate * Time.deltaTime);
            }
            if (playerStats.currentMana < playerStats.maxMana)
            {
                playerStats.RestoreMana(manaRate * Time.deltaTime);
            }

            if (playerStats.currentHealth >= playerStats.maxHealth && 
                playerStats.currentMana >= playerStats.maxMana)
            {
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
}
