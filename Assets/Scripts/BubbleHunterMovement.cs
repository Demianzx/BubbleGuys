using UnityEngine;
using UnityEngine.InputSystem;

public class BubbleHunterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 50f;
    [SerializeField] float deceleration = 30f;
    [SerializeField] float airControl = 0.7f;

    [Header("Combat Settings")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;
    [SerializeField] float shootCooldown = 0.5f;



    // Component references
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;

    // Movement variables
    Vector2 moveInput;
    Vector2 currentVelocity;
    float lastShootTime;
    bool isAlive = true;

    void Start()
    {
        // Get component references
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();

        // Configure rigidbody for smooth movement
        myRigidbody.gravityScale = 0f;
        myRigidbody.drag = 3f;
    }

    void Update()
    {
        if (!isAlive) { return; }

        HandleMovement();
        FlipSprite();
        CheckForDeath();
    }

    void HandleMovement()
    {
        // Calculate target velocity based on input
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Apply acceleration or deceleration
        float controlModifier = myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) ? 1f : airControl;
        float accelRate = (targetVelocity.magnitude > 0.01f) ? acceleration : deceleration;

        // Smoothly interpolate to target velocity
        currentVelocity = Vector2.MoveTowards(
            myRigidbody.velocity,
            targetVelocity,
            accelRate * controlModifier * Time.deltaTime
        );

        // Apply movement
        myRigidbody.velocity = currentVelocity;

        // Update animator
        bool isMoving = currentVelocity.magnitude > 0.1f;
        myAnimator.SetBool("isMoving", isMoving);
    }

    void FlipSprite()
    {
        bool hasHorizontalSpeed = Mathf.Abs(currentVelocity.x) > 0.1f;

        if (hasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(currentVelocity.x), 1f);
        }
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        moveInput = value.Get<Vector2>();
    }

    void OnFire(InputValue value)
    {
        if (!isAlive) { return; }

        // Check cooldown
        if (Time.time - lastShootTime < shootCooldown) { return; }

        // Calculate shoot direction based on movement or facing direction
        Vector2 shootDirection;
        if (moveInput.magnitude > 0.1f)
        {
            // Use movement direction if moving
            shootDirection = moveInput.normalized;
        }
        else
        {
            // Use facing direction if standing still
            shootDirection = new Vector2(transform.localScale.x, 0f);
        }

        // Shoot bullet
        GameObject newBullet = Instantiate(bullet, gun.position, transform.rotation);
        newBullet.GetComponent<BubbleHunterBullet>().SetDirection(shootDirection);
        lastShootTime = Time.time;
    }

    void CheckForDeath()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            Die();
        }
    }

    void Die()
    {
        isAlive = false;
        myAnimator.SetTrigger("isDying");
        myRigidbody.velocity = Vector2.zero;
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }
}