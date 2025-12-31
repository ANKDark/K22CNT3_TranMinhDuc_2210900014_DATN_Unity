using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

public class PlayerCombat : MonoBehaviour
{
    [Header("Input System Settings")]
    private InputSystem_Actions inputActions;

    [Header("Weapon Settings")]
    public WeaponDamage equippedWeapon;
    public Transform slashSpawnPoint;
    public AudioClip[] attackSounds;

    [SerializeField]
    private AudioSource audioSource;
    private Animator anim;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;

    [Header("VFX Settings")]
    public GameObject[] slashVFXList;
    public GameObject BloodVFX;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();

        if (audioSource == null)
        {
            Debug.LogWarning("Chưa thêm AudioSource vào nhân vật");
        }
        if (equippedWeapon == null)
            equippedWeapon = GetComponentInChildren<WeaponDamage>();

        if (playerStats != null)
        {
            playerStats.OnPlayerHit += PlayerHitReaction;
        }
    }

    void OnEnable()
    {
        if (inputActions != null) inputActions.Player.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null) inputActions.Player.Disable();
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnPlayerHit -= PlayerHitReaction;
        }
    }

    private void PlayerHitReaction()
    {
        if (BloodVFX != null)
        {
            GameObject vfxBloodInstance = Instantiate(
                BloodVFX,
                transform.position + Vector3.up * 1.3f + transform.forward * 0.5f,
                Quaternion.identity
            );
            Destroy(vfxBloodInstance, 1f);
        }
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetTrigger("Hit");
        }
        CloseWeaponCollider();
    }

    void Update()
    {
        if (playerStats.isActing) return;

        if (playerStats != null && playerStats.isPlayerDead)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (playerStats != null && playerStats.isHurting)
            return;

        if (inputActions != null && inputActions.Player.Attack.WasPressedThisFrame())
        {
            Attack();
        }

        if (inputActions != null && inputActions.Player.Dodge.WasPressedThisFrame())
        {
            Dodging();
        }
    }

    private void Attack()
    {
        if (playerStats != null)
        {
            if (playerStats.UseStamina(10f))
            {
                anim.SetTrigger("Attack");
                playerMovement.StopMoving();
            }
        }
    }

    private void Dodging()
    {
        if (playerStats != null)
        {
            if (playerStats.UseStamina(20f))
            {
                anim.SetTrigger("Dodging");
            }
        }
    }


    public void OpenWeaponCollider()
    {
        if (equippedWeapon != null)
            equippedWeapon.EnableSwordCollider();
    }

    public void CloseWeaponCollider()
    {
        if (equippedWeapon != null)
            equippedWeapon.DisableSwordCollider();
    }

    public void TriggerSlashVFX(int index)
    {
        if (playerStats != null && playerStats.isHurting) return;

        if (slashVFXList != null && index >= 0 && index < slashVFXList.Length)
        {
            GameObject prefabToSpawn = slashVFXList[index];

            if (prefabToSpawn != null && slashSpawnPoint != null)
            {
                GameObject vfxInstance = Instantiate(
                    prefabToSpawn,
                    slashSpawnPoint.position,
                    slashSpawnPoint.rotation
                );
                Destroy(vfxInstance, 2f);
            }
            else
            {
                if (prefabToSpawn == null)
                    Debug.LogError($"Lỗi: Element {index} trong 'Slash VFX List' đang để trống!");
                if (slashSpawnPoint == null)
                    Debug.LogError("Lỗi: Chưa gán 'Slash Spawn Point' trong Inspector!");
            }
        }
    }

    public void TriggerSlashSound(int index)
    {
        if (playerStats != null && playerStats.isHurting) return;

        if (
            audioSource != null
            && attackSounds != null
            && index >= 0
            && index < attackSounds.Length
        )
        {
            AudioClip clipToPlay = attackSounds[index];

            if (clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);

                audioSource.PlayOneShot(clipToPlay);
            }
            else
            {
                Debug.LogWarning($"Lỗi: Element {index} trong 'Attack Sounds' đang để trống!");
            }
        }
    }
}
