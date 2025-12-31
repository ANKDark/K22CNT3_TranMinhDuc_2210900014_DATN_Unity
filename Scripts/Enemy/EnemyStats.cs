using UnityEngine;
using System;

public class EnemyStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("VFX Settings")]
    [SerializeField] private GameObject healthBarUI;

    [SerializeField]
    private GameObject vfxBloodHit;

    [Header("Status")]
    public bool isHurting = false;

    [SerializeField]
    private GameObject vfxTeleportation;

    public event Action<float, float> OnHealthChanged;

    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        Debug.Log(transform.name + " has taken " + damageAmount + " damage!");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (!isHurting)
        {
            isHurting = true;
            if (anim != null) anim.SetTrigger("Hurt");
            StartCoroutine(ResetHurting());
        }

        if (vfxBloodHit != null)
        {
            GameObject vfxInstance = Instantiate(
                vfxBloodHit,
                transform.position + Vector3.up * 0.3f,
                Quaternion.identity
            );
            Destroy(vfxInstance, 1f);
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator ResetHurting()
    {
        yield return new WaitForSeconds(0.5f);
        isHurting = false;
    }

    [Header("Loot Settings")]
    public LootTable lootTable;

    void Die()
    {
        Debug.Log(transform.name + " đã chết!");
        
        if (lootTable != null)
        {
            Item droppedItem = lootTable.GetDroppedItem(out ItemObject droppedSource);
            if (droppedItem != null && droppedSource != null)
            {
                SpawnLoot(droppedSource, droppedItem);
            }
        }

        if (vfxTeleportation != null)
        {
            Vector3 spawnPos = transform.position;
            if (
                Physics.Raycast(
                    transform.position + Vector3.up,
                    Vector3.down,
                    out RaycastHit hit,
                    5f
                )
            )
            {
                spawnPos = hit.point;
            }
            GameObject vfxInstance = Instantiate(vfxTeleportation, spawnPos, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void SpawnLoot(ItemObject itemTemplate, Item itemInstance)
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 4f;
        randomOffset.y = 2.5f;
        Vector3 spawnPos = transform.position + randomOffset;

        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
        {
            spawnPos = hit.point + Vector3.up * 1f;
        }

        GameObject lootObj = new GameObject("Loot_" + itemInstance.name);
        lootObj.transform.position = spawnPos;

        SphereCollider col = lootObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;

        GroundItem groundItem = lootObj.AddComponent<GroundItem>();
        
        groundItem.item = itemTemplate;
        groundItem.SetItem(itemInstance);

        UniqueID uid = lootObj.AddComponent<UniqueID>();
        uid.uniqueId = System.Guid.NewGuid().ToString(); 
    }
}
