using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [SerializeField] float floatForce = 1f;
    [SerializeField] float pushForce = 5f;
    [SerializeField] float shrinkAmount = 0.1f;
    [SerializeField] float minSize = 0.5f;
    [SerializeField] float maxVelocity = 2f;
    [SerializeField] float dampingForce = 0.98f;

    [Header("Debug")]
    [SerializeField] bool showDebug = false;

    private Rigidbody2D myRigidbody;
    private CircleCollider2D myCollider;
    private bool isBeingPushed = false;
    private LayerMask collisionMask;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<CircleCollider2D>();
        ConfigureRigidbody();
        ConfigureCollider();

        collisionMask = ~LayerMask.GetMask("Background");
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Background"), true);
    }

    void Start()
    {
        if (showDebug)
        {
            Debug.Log($"Bubble initialized at {transform.position}");
            Debug.Log($"Rigidbody2D settings - Mass: {myRigidbody.mass}, Drag: {myRigidbody.drag}, Gravity: {myRigidbody.gravityScale}");
            Debug.Log($"Layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
        }
    }

    private void ConfigureRigidbody()
    {
        myRigidbody.gravityScale = 0f;
        myRigidbody.mass = 1f;
        myRigidbody.drag = 2f;
        myRigidbody.angularDrag = 0.5f;
        myRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        myRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        myRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        myRigidbody.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void ConfigureCollider()
    {
        myCollider.isTrigger = false;
        myCollider.radius = transform.localScale.x * 0.45f;
    }

    void FixedUpdate()
    {
        if (isBeingPushed)
        {
            return;
        }

        // Aplicar fuerza de flotación solo si no hay obstáculos arriba
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, myCollider.radius * 2f, collisionMask);
        if (!hit)
        {
            float currentVelocityY = myRigidbody.velocity.y;
            float floatMultiplier = currentVelocityY < 0 ? 2f : 1f;
            myRigidbody.AddForce(Vector2.up * floatForce * floatMultiplier, ForceMode2D.Force);
        }

        // Aplicar un pequeño amortiguamiento al movimiento horizontal
        Vector2 currentVelocity = myRigidbody.velocity;
        currentVelocity.x *= dampingForce;
        myRigidbody.velocity = currentVelocity;

        // Limitar velocidad máxima
        if (myRigidbody.velocity.magnitude > maxVelocity)
        {
            myRigidbody.velocity = Vector2.ClampMagnitude(myRigidbody.velocity, maxVelocity);
        }

        if (showDebug)
        {
            Debug.Log($"Velocity: {myRigidbody.velocity}, Position: {transform.position}");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (showDebug)
        {
            Debug.Log($"Collision with: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Tag: {collision.gameObject.tag})");
        }

        // Ignorar colisiones con el background
        if (collision.gameObject.layer == LayerMask.NameToLayer("Background"))
        {
            Physics2D.IgnoreCollision(myCollider, collision.collider);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        else if (collision.gameObject.CompareTag("Enemy") ||
                 collision.gameObject.layer == LayerMask.NameToLayer("Hazards"))
        {
            ShrinkBubble();
        }
    }

    private void HandlePlayerCollision(Collision2D collision)
    {
        Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
        pushDirection.y += 0.5f;
        pushDirection.Normalize();

        if (showDebug)
        {
            Debug.Log($"Player push direction: {pushDirection}");
        }

        myRigidbody.velocity = Vector2.zero;
        myRigidbody.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        isBeingPushed = true;
        Invoke("ResetPushState", 0.3f);
    }

    void ResetPushState()
    {
        isBeingPushed = false;
    }

    void ShrinkBubble()
    {
        Vector3 currentScale = transform.localScale;
        Vector3 newScale = currentScale * (1f - shrinkAmount);

        if (newScale.x < minSize)
        {
            if (showDebug) Debug.Log("Bubble destroyed - too small");
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
            Destroy(gameObject);
        }
        else
        {
            transform.localScale = newScale;
            myCollider.radius = newScale.x * 0.45f;
        }
    }
}