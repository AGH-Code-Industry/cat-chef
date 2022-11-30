using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct Input {
    public float x;
    public bool jumpDown;
    public bool jumpJustPressed;
    public float lastJumpDownTime;
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider2D;
    private PlayerInputActions playerInputActions;


    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();

        defaultGravityScale = rb.gravityScale;
        health = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        initialPosition = transform.position;
    }

    private void FixedUpdate() {
        ReadInput();
        CalculateGroundCheck();

        CalculateWalk();
        CalculateJump();
        CalculateJumpEndEarly();
        CalculateJumpApex();

        MovePlayer();
    }

    #region Health

    [Header("Health")] [SerializeField] private float maxHealth = 100f;
    [SerializeField] private HealthBar healthBar;
    private float health;
    private Vector3 initialPosition;
    public float damage = 20f;

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

    #region Move

    private float defaultGravityScale;

    private float horizonalVelocity, verticalVelocity;

    private void MovePlayer() {
        if (jumpEndedEarly && rb.velocity.y > 0) {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - jumpEndEarlyGravityModifier * Time.deltaTime);
        }

        rb.transform.position += Vector3.right * horizonalVelocity * Time.deltaTime;
    }

    #endregion

    #region Input

    private Input input;

    private void ReadInput() {
        bool prevJumpDown = input.jumpDown;
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        input.x = inputVector.x;
        input.jumpDown = playerInputActions.Player.Jump.IsPressed();
        input.jumpJustPressed = !prevJumpDown && input.jumpDown;

        if (input.jumpDown) {
            input.lastJumpDownTime = Time.time;
        }
    }

    #endregion

    #region Walk

    [Header("Move")] [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 70f;
    [SerializeField] private float apexSpeedBonus = 25f;

    private void CalculateWalk() {
        if (input.x != 0) {
            horizonalVelocity += acceleration * Time.deltaTime * input.x;
            horizonalVelocity = Mathf.Clamp(horizonalVelocity, -maxSpeed, maxSpeed);
            horizonalVelocity += input.x * apexSpeedBonus * apexPoint * Time.deltaTime;
        } else {
            horizonalVelocity = Mathf.MoveTowards(horizonalVelocity, 0, deceleration * Time.deltaTime);
        }
    }

    #endregion

    #region Jump

    [Header("Jump")] [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpForce = 25f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 100f;
    [SerializeField] private float jumpApexThreshold = 10f;
    [SerializeField] private float apexGravityModifier = .5f;
    [SerializeField] private float jumpBufferTime = .1f;
    private bool jumpEndedEarly = false;
    private bool onGround = false;
    private bool hasBufferedJump => input.lastJumpDownTime + jumpBufferTime > Time.time;
    private float apexPoint;

    private void CalculateJump() {
        if (onGround && (input.jumpJustPressed || hasBufferedJump) && rb.velocity.y == 0) {
            jumpEndedEarly = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce) ;
        } else {
        }
    }

    private void CalculateGroundCheck() {
        float extraHeight = .5f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size - Vector3.right * extraHeight, 0f, Vector2.down, extraHeight, groundMask);
        onGround = raycastHit.collider != null;
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
}
