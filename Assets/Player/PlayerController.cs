using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct Input {
    public float x;
    public bool attackDown;
    public bool jumpDown;
    public float lastJumpDownTime;
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private PlayerInputActions playerInputActions;
    private Animator animator;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        
        animator = GetComponent<Animator>();
        weaponCollider = meleeWeapon.gameObject.GetComponentInChildren<PolygonCollider2D>();
        weaponAnimator = meleeWeapon.GetComponent<Animator>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        defaultGravityScale = rb.gravityScale;
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        initialPosition = transform.position;
        availableAirJumps = maxAirJumps;
    }

    private void FixedUpdate() {
        ReadInput();
        CalculateDirection();
        CalculateGroundCheck();
        CalculateWallCheck();

        CalculateWalk();
        CalculateQueueJump();
        CalculateJumpEndEarly();
        CalculateJumpApex();
        CalculateWallSlideDown();

        CalculateAttack();

        CalculateRoll();

        UpdateVelocity();
    }

    #region Health

    [Header("Health")] [SerializeField] private float maxHealth = 100f;
    [SerializeField] private HealthBar healthBar;
    private float health;
    private Vector3 initialPosition;

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Spikes") {
            TakeDamage(other.gameObject.GetComponent<Spikes>().damage);
        }
    }

    private void TakeDamage(float amount) {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health == 0) {
            // Die();
            Debug.Log("Umarłeś");
            transform.position = initialPosition;
            health = maxHealth;
        }
        healthBar.SetHealth(health);
    }

    private void Die() {
        Destroy(gameObject);
    }

    #endregion

    #region UpdateVelocity

    private float defaultGravityScale;

    private float horizontalVelocity;

    private void UpdateVelocity() {
        Vector2 velocity = rb.velocity;

        velocity.x = horizontalVelocity;

        if (jumpEndedEarly && rb.velocity.y > 0) {
            velocity.y = rb.velocity.y - jumpEndEarlyGravityModifier * Time.deltaTime;
        }
        if (slidingDownTheWall) {
            velocity.y = Mathf.Clamp(velocity.y, -wallSlidingMaxSpeed, float.MaxValue);
        }
        velocity.y = Mathf.Clamp(velocity.y, -maxFallingSpeed, float.MaxValue);

        rb.velocity = velocity;
    }

    #endregion

    #region Input

    private Input input;

    private void ReadInput() {
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        input.x = inputVector.x;

        input.jumpDown = playerInputActions.Player.Jump.IsPressed();
        if (input.jumpDown) {
            input.lastJumpDownTime = Time.time;
        }

        input.attackDown = playerInputActions.Player.Attack.IsPressed();
    }

    #endregion

    #region Walk

    [Header("Move")] [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private float apexSpeedBonus = 25f;
    private Vector2 direction = Vector2.right;

    private void CalculateWalk() {
        if (isRolling) return;

        if (input.x != 0) {
            horizontalVelocity += acceleration * Time.deltaTime * input.x;
            horizontalVelocity = Mathf.Clamp(horizontalVelocity, -maxSpeed, maxSpeed);
            horizontalVelocity += input.x * apexSpeedBonus * apexPoint * Time.deltaTime;
        } else {
            horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, 0, deceleration * Time.deltaTime);
        }
    }

    private void CalculateDirection() {
        if (isRolling) return;
        if (input.x > 0 && direction == Vector2.left) FlipDirection();
        if (input.x < 0 && direction == Vector2.right) FlipDirection();
    }

    private void FlipDirection() {
        direction = direction == Vector2.right ? Vector2.left : Vector2.right;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    #endregion

    #region Jump

    [Header("Jump")] [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpForce = 25f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 100f;
    [SerializeField] private float jumpApexThreshold = 10f;
    [SerializeField] private float apexGravityModifier = .5f;
    [SerializeField] private float jumpBufferTime = .1f;
    [SerializeField] private float groundCheckExtraHeight = .5f;
    [SerializeField] private float maxFallingSpeed = 25f;
    [SerializeField] private float wallJumpHorizontalSpeed = 20f;
    [SerializeField] private int maxAirJumps = 1;
    private bool jumpEndedEarly = false;
    private bool onGround = false;
    private bool hasBufferedJump => input.lastJumpDownTime + jumpBufferTime > Time.time;
    private float apexPoint;
    private int availableAirJumps;

    public void OnJump() {
        if (onGround && rb.velocity.y == 0) {
            Jump();
        } else if(touchingWallFront ) {
            Jump();
            horizontalVelocity = rb.velocity.x - direction.x * wallJumpHorizontalSpeed;
         }else if (availableAirJumps > 0 ) {
            Jump();
            availableAirJumps--;
        }
    }

    private void Jump() {
        jumpEndedEarly = false;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void CalculateGroundCheck() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, groundCheckExtraHeight, groundMask);
        onGround = raycastHit.collider != null;
        if (onGround) {
            availableAirJumps = maxAirJumps;
        }
    }

    public void CalculateQueueJump() {
        if (onGround && hasBufferedJump && rb.velocity.y == 0) {
            Jump();
        } 
    }

    private void CalculateJumpEndEarly() {
        if (!jumpEndedEarly && !input.jumpDown && rb.velocity.y > 0) {
            jumpEndedEarly = true;
        }
    }

    private void CalculateJumpApex() {
        if (!onGround) {
            apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(rb.velocity.y));
            rb.gravityScale = Mathf.Lerp(defaultGravityScale, defaultGravityScale * apexGravityModifier, apexPoint);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos() {
        if (!boxCollider2D) return;
        DrawGroundCheckGizmo();
        DrawWallCheckGizmo();
    }

    private void DrawGroundCheckGizmo() {
        Gizmos.color = onGround ? Color.magenta : Color.gray;
        DrawBoxCastGizmo(
            Vector3.zero,
            Vector2.down,
            groundCheckExtraHeight
        );
    }

    private void DrawWallCheckGizmo() {
        Gizmos.color = touchingWallFront ? Color.blue : Color.gray;
        DrawBoxCastGizmo(
            Vector3.up * wallCheckExtraWidth,
            direction,
            wallCheckExtraWidth
        );
    }

    private void DrawBoxCastGizmo(Vector3 sizeReduction, Vector2 direction, float distance) {
        Vector2 center = boxCollider2D.bounds.center;
        Vector2 size = boxCollider2D.bounds.size - sizeReduction;
        
        Vector2[] corners = {
            center + new Vector2(size.x, size.y) / 2 + direction * distance,
            center + new Vector2(size.x, -size.y) / 2 + direction * distance,
            center + new Vector2(-size.x, -size.y) / 2 + direction * distance,
            center + new Vector2(-size.x, size.y) / 2 + direction * distance,
        };
        
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }

    #endregion

    #region WallJump

    [Header("Wall Jump")] 
    [SerializeField] float wallCheckExtraWidth = 0.2f;
    [SerializeField] float wallSlidingMaxSpeed = 3f;
    private bool touchingWallFront = false;
    private bool slidingDownTheWall = false;

    private void CalculateWallCheck() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size - Vector3.right * wallCheckExtraWidth, 0f, direction, wallCheckExtraWidth, groundMask);
        touchingWallFront = raycastHit.collider != null;
    }

    private void CalculateWallSlideDown() {
        slidingDownTheWall = touchingWallFront && input.x != 0;
    }

    #endregion

    #region Attack

    [Header("Melee Weapon")]
    [SerializeField] private MeleeWeapon meleeWeapon;
    PolygonCollider2D weaponCollider;
    Animator weaponAnimator;

    private void CalculateAttack() {
        if (input.attackDown) {
            weaponCollider.enabled = true;
            weaponAnimator.Play("Attack");
        } 
    }

    private void OnAttackEnd() {
        weaponCollider.enabled = false;
    }

    #endregion
    
    #region Roll

    [Header("Roll")]
    [SerializeField] private float rollDistance = 6f;
    [SerializeField] private float rollDurationSeconds = .3f;
    [SerializeField] private LayerMask enemyLayer;
    private bool isRolling = false;
    private float rollStartX;

    public void OnRoll() {
        if (onGround && rb.velocity.y == 0 && !isRolling) {
            isRolling = true;
            rollStartX = transform.position.x;
            animator.Play("Roll");
            Physics2D.IgnoreLayerCollision(gameObject.layer, (int)Mathf.Log(enemyLayer.value, 2)); 
        }
    }

    private void CalculateRoll() {
        if (!isRolling) return;

        float distanceRolled = Mathf.Abs(rollStartX - transform.position.x);
        if (touchingWallFront || distanceRolled >= rollDistance) {
            isRolling = false;
            Physics2D.IgnoreLayerCollision(gameObject.layer, (int)Mathf.Log(enemyLayer.value, 2), false); 
        } else {
            float rollSpeed = rollDistance / rollDurationSeconds;
            horizontalVelocity = rollSpeed * direction.x;
        }
    }

    #endregion
}
