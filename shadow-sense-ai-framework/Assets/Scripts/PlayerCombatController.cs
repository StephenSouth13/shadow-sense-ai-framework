using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Final Production-Ready Player Controller for Stephen South.
/// Features: Camera-relative movement, mana-restricted flight, 
/// and frame-perfect combat trigger integration with the New Input System.
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(PlayerStatsManager))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Locomotion")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float flySpeed = 35f;
    [SerializeField] private float boostMultiplier = 2.0f;
    [SerializeField] private float verticalSpeed = 20f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = 25f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visuals & FX")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem sonicBoomFX;

    // Component References
    private CharacterController controller;
    private PlayerStatsManager stats;
    private Transform mainCameraTransform;

    // Input Action References (Strict Integration)
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction secondaryAttackAction;
    private InputAction flightToggleAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    // State Variables
    private Vector3 currentVelocity;
    private bool isFlightMode = false;
    private float lastAttackTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStatsManager>();
        
        if (animator == null) 
            animator = GetComponentInChildren<Animator>();

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;

        InitializeInputs();
    }

    private void InitializeInputs()
    {
        var asset = InputSystem.actions;
        if (asset == null) return;

        moveAction = asset.FindAction("Player/Move");
        lookAction = asset.FindAction("Player/Look");
        attackAction = asset.FindAction("Player/Attack");
        secondaryAttackAction = asset.FindAction("Player/SecondaryAttack");
        flightToggleAction = asset.FindAction("Player/FlightToggle");
        jumpAction = asset.FindAction("Player/Jump");
        sprintAction = asset.FindAction("Player/Sprint");
        crouchAction = asset.FindAction("Player/Crouch");
    }

    private void Update()
    {
        if (stats != null && stats.IsDead()) return;

        HandleFlightToggle();
        HandleCombat();
        HandleMovement();
        UpdateAnimator();
    }

    private void HandleFlightToggle()
    {
        if (flightToggleAction != null && flightToggleAction.WasPressedThisFrame())
        {
            // Only allow flight if mana is sufficient
            if (!isFlightMode)
            {
                if (stats != null && stats.CanFly())
                {
                    isFlightMode = true;
                }
                else
                {
                    Debug.Log("Insufficient Energy for Flight.");
                }
            }
            else
            {
                isFlightMode = false;
            }
        }

        // Automatic fallback if mana reaches zero during flight
        if (isFlightMode && stats != null && !stats.CanFly())
        {
            isFlightMode = false;
        }
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        bool isBoosting = sprintAction != null && sprintAction.IsPressed();

        // Calculate Move Direction relative to Camera
        Vector3 forward = mainCameraTransform != null ? mainCameraTransform.forward : transform.forward;
        Vector3 right = mainCameraTransform != null ? mainCameraTransform.right : transform.right;
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * input.y + right * input.x).normalized;

        if (isFlightMode)
        {
            float verticalInput = 0;
            if (jumpAction != null && jumpAction.IsPressed()) verticalInput += 1f;
            if (crouchAction != null && crouchAction.IsPressed()) verticalInput -= 1f;

            float currentSpeed = flySpeed * (isBoosting ? boostMultiplier : 1.0f);
            
            // In Flight: Direct 3D movement without gravity
            Vector3 flightMove = (moveDir * currentSpeed) + (Vector3.up * verticalInput * verticalSpeed);
            controller.Move(flightMove * Time.deltaTime);
            
            // Handle Flight Rotation
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // FX logic
            if (isBoosting && sonicBoomFX != null && !sonicBoomFX.isPlaying)
                sonicBoomFX.Play();
            else if (!isBoosting && sonicBoomFX != null && sonicBoomFX.isPlaying)
                sonicBoomFX.Stop();
        }
        else
        {
            // Ground Locomotion
            float currentSpeed = walkSpeed * (isBoosting ? 1.5f : 1.0f);
            
            // Gravity logic
            if (controller.isGrounded)
            {
                currentVelocity.y = -2f; // Ensure grounding
            }
            else
            {
                currentVelocity.y -= gravity * Time.deltaTime;
            }

            Vector3 horizontalMove = moveDir * currentSpeed;
            Vector3 finalMove = horizontalMove + Vector3.up * currentVelocity.y;
            
            controller.Move(finalMove * Time.deltaTime);

            // Handle Ground Rotation
            if (moveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            if (sonicBoomFX != null && sonicBoomFX.isPlaying)
                sonicBoomFX.Stop();
        }
    }

    private void HandleCombat()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            PerformAttack("Punch");
        }
        else if (secondaryAttackAction != null && secondaryAttackAction.WasPressedThisFrame())
        {
            PerformAttack("Kick");
        }
    }

    private void PerformAttack(string animatorTrigger)
    {
        lastAttackTime = Time.time;
        if (animator != null) animator.SetTrigger(animatorTrigger);

        // Standardized damage check
        Vector3 origin = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, enemyLayer);
        
        foreach (var hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(attackDamage, gameObject);
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Drive Speed float with actual horizontal velocity
        Vector3 horizontalVel = controller.velocity;
        horizontalVel.y = 0;
        float speed = horizontalVel.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetBool("isFlying", isFlightMode);
    }

    private void OnDrawGizmosSelected()
    {
        // Combat range visualization
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + transform.forward * 1.5f + Vector3.up;
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
