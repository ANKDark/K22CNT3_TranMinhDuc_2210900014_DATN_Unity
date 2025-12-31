using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System Settings")]
    private InputSystem_Actions inputActions;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    public bool IsMoving { get; private set; }
    public bool IsRunning { get; private set; }

    private Vector3 moveDirection;
    private Vector3 velocity;
    private CharacterController characterController;
    private Animator anim;
    private float stepTimer;
    private PlayerStats playerStats;
    private Transform cameraTransform;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Player.Disable();
    }

    void Update()
    {
        if (playerStats != null && playerStats.isPlayerDead)
        {
            anim.SetBool("isDead", true);
            return;
        }

        if ((playerStats != null && playerStats.isHurting) || (playerStats != null && playerStats.isActing))
        {
            anim.SetFloat("Horizontal", 0);
            anim.SetFloat("Vertical", 0);
            return;
        }

        Move();
    }

    private void Move()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        anim.SetBool("isGrounded", isGrounded);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 inputVector = Vector2.zero;
        if (inputActions != null)
        {
            inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        }

        if (inputVector.magnitude < 0.1f)
        {
            inputVector = Vector2.zero;
        }

        float moveX = inputVector.x;
        float moveZ = inputVector.y;

        IsMoving = inputVector.sqrMagnitude > 0.01f;

        bool isAiming = false;
        if (inputActions != null) isAiming = inputActions.Player.Aim.IsPressed();

        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        }
        else
        {
            moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        }

        if (isAiming)
        {
            if (cameraTransform != null)
            {
                Vector3 lookDirection = cameraTransform.forward;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
                }
            }
        }
        else
        {
            if (IsMoving && moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        bool wantToRun = false;
        if (inputActions != null)
        {
            wantToRun = inputActions.Player.Sprint.IsPressed();
        }

        IsRunning = wantToRun && IsMoving;

        if (IsRunning && playerStats != null)
        {
            bool hasStamina = playerStats.UseStamina(15f * Time.deltaTime);
            if (!hasStamina)
            {
                IsRunning = false;
            }
        }

        float currentSpeed = IsRunning ? runSpeed : walkSpeed;

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        HandleFootsteps(IsRunning, moveX, moveZ);

        if (isGrounded && inputActions != null && inputActions.Player.Jump.WasPressedThisFrame())
        {
            Jump();
        }
        float animX = IsRunning ? moveX : moveX * 0.5f;
        float animZ = IsRunning ? moveZ : moveZ * 0.5f;

        anim.SetFloat("Horizontal", animX, 0.1f, Time.deltaTime);
        anim.SetFloat("Vertical", animZ, 0.1f, Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleFootsteps(bool isRunning, float inputX, float inputZ)
    {
        if (isGrounded && (inputX != 0 || inputZ != 0))
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                PlayStepSound(isRunning);
                stepTimer = isRunning ? runStepInterval : walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0;
        }
    }

    private void PlayStepSound(bool isRunning)
    {
        if (audioSource == null) return;

        AudioClip[] clips = isRunning ? runSounds : walkSounds;

        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.volume = isRunning ? 1f : 0.6f;
            audioSource.PlayOneShot(clips[randomIndex]);
        }
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        anim.SetTrigger("Jump");

        if (isGrounded)
        {
            if (playerStats != null) playerStats.isActing = false;
        }
        else
        {
            if (playerStats != null) playerStats.isActing = true;
        }
    }

    public void StopMoving()
    {
        anim.SetFloat("Horizontal", 0);
        anim.SetFloat("Vertical", 0);
    }
}