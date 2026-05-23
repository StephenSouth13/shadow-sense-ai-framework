using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Professional Player Controller for Stephen South (Invincible archetype).
/// Supports omni-directional flight, targeting, and cinematic melee combat.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Flight Dynamics")]
    public float flySpeed = 30f;
    public float boostMultiplier = 2.0f;
    public float rotationSpeed = 10f;
    public float verticalSpeed = 15f;

    [Header("Combat Settings")]
    public float attackRange = 5f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;

    [Header("Visuals")]
    public Animator animator;
    public ParticleSystem sonicBoomFX;

    private CharacterController controller;
    private PlayerStatsManager stats;
    private Vector3 moveInput;
    private float verticalInput;
    private bool isBoosting;
    private float lastAttackTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStatsManager>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleInput();
        HandleFlight();
        HandleRotation();
    }

    private void HandleInput()
    {
        // New Input System direct access for responsiveness
        if (Keyboard.current != null)
        {
            float x = (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0);
            float z = (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0);
            moveInput = new Vector3(x, 0, z).normalized;

            verticalInput = (Keyboard.current.spaceKey.isPressed ? 1 : 0) - (Keyboard.current.leftShiftKey.isPressed ? 1 : 0);
            isBoosting = Keyboard.current.leftCtrlKey.isPressed;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ExecuteAttack();
            }
        }
    }

    private void HandleFlight()
    {
        // If out of mana, force ground-only movement
        bool canFly = stats == null || stats.CanFly();
        if (!canFly && !controller.isGrounded)
        {
            verticalInput = -1f; // Force fall
            isBoosting = false;
        }

        float currentSpeed = flySpeed * (isBoosting && canFly ? boostMultiplier : 1.0f);
        
        // Horizontal flight relative to world
        Vector3 horizontalMove = moveInput * currentSpeed;
        
        // Vertical flight (restricted by mana)
        float vInput = canFly ? verticalInput : (controller.isGrounded ? 0 : -1f);
        Vector3 verticalMove = Vector3.up * vInput * verticalSpeed;

        Vector3 finalMove = horizontalMove + verticalMove;
        controller.Move(finalMove * Time.deltaTime);

        // Animation updates
        if (animator != null)
        {
            animator.SetFloat("ForwardSpeed", moveInput.z);
            animator.SetBool("IsFlying", !controller.isGrounded);
        }

        // FX
        if (isBoosting && canFly && sonicBoomFX != null && !sonicBoomFX.isPlaying)
            sonicBoomFX.Play();
    }

    private void HandleRotation()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ExecuteAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        if (animator != null) animator.SetTrigger("Attack");

        // Sphere cast for combat detection
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            IDamageable entity = hit.GetComponent<IDamageable>();
            if (entity != null)
            {
                entity.TakeDamage(attackDamage, gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 2f, attackRange);
    }
}
