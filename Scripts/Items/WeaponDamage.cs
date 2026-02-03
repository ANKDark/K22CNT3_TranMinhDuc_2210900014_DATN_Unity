using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [Header("Weapon Stats")]
    public float baseDamage = 10f;
    [Header("References")]
    private InventoryPlayer inventoryPlayer;
    public Collider swordCollider;
    void Start()
    {
        inventoryPlayer = GetComponentInParent<InventoryPlayer>();
        if (inventoryPlayer == null)
        {
            Debug.LogWarning("Không tìm thấy InventoryPlayer trên cha của vũ khí");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (inventoryPlayer != null && other.gameObject == inventoryPlayer.gameObject)
        {
            return;
        }

        IDamageable damageableTarget = other.GetComponent<IDamageable>();

        if (damageableTarget != null)
        {
            float finalDamage = baseDamage;
            float critMultiplier = 2f;
            if (inventoryPlayer != null)
            {
                int strength = inventoryPlayer.GetAttributeValue(Attributes.Strength);
                int critical = inventoryPlayer.GetAttributeValue(Attributes.Critical);

                bool isCritical = Random.Range(0f, 100f) < critical;

                finalDamage += strength;
                if (isCritical)
                {
                    finalDamage *= critMultiplier;
                }
                
                damageableTarget.TakeDamage(finalDamage, isCritical);
            }
            else
            {
                damageableTarget.TakeDamage(finalDamage, false);
            }
        }
    }

    public void EnableSwordCollider()
    {
        if (swordCollider != null)
            swordCollider.enabled = true;
    }

    public void DisableSwordCollider()
    {
        if (swordCollider != null)
            swordCollider.enabled = false;
    }
}
