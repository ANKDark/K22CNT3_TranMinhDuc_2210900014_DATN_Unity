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

            if (inventoryPlayer != null)
            {
                int strength = inventoryPlayer.GetAttributeValue(Attributes.Strength);
                finalDamage += strength;

                Debug.Log($"Dame kiem: {baseDamage} + Sức mạnh: {strength * 2f} = Tổng dame: {finalDamage}");
            }
            damageableTarget.TakeDamage(finalDamage);
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
