using UnityEngine;
using UnityEngine.InputSystem;

public class BubblePullSystem : MonoBehaviour
{
    [Header("Pull Settings")]
    [SerializeField] float pullRadius = 5f;
    [SerializeField] float pullForce = 10f;
    [SerializeField] float maxPullSpeed = 5f;
    [SerializeField] LayerMask bubbleLayer;

    [Header("Visual Feedback")]
    [SerializeField] bool showPullRadius = false;

    private bool isPulling = false;
    private CircleCollider2D pullTrigger;
    private PlayerInput playerInput;

    void Start()
    {
        pullTrigger = gameObject.AddComponent<CircleCollider2D>();
        pullTrigger.radius = pullRadius;
        pullTrigger.isTrigger = true;
        playerInput = GetComponent<PlayerInput>();
    }

    void OnPull(InputValue value)
    {
        isPulling = value.isPressed;

        // Si se suelta el botón, liberar todas las burbujas
        if (!isPulling)
        {
            ReleaseBubbles();
        }
    }

    void OnDisable()
    {
        // Asegurarse de liberar las burbujas si se desactiva el componente
        isPulling = false;
        ReleaseBubbles();
    }

    void ReleaseBubbles()
    {
        Collider2D[] nearbyBubbles = Physics2D.OverlapCircleAll(transform.position, pullRadius, bubbleLayer);
        foreach (Collider2D bubbleCollider in nearbyBubbles)
        {
            if (bubbleCollider != null)
            {
                BubbleController bubble = bubbleCollider.GetComponent<BubbleController>();
                if (bubble != null)
                {
                    bubble.SetPullState(false);
                }
            }
        }
    }

    void Update()
    {
        // Verificar constantemente si el botón está presionado
        if (playerInput != null)
        {
            var pullAction = playerInput.actions["Pull"];
            isPulling = pullAction.IsPressed();

            if (!isPulling)
            {
                ReleaseBubbles();
            }
        }
    }

    void FixedUpdate()
    {
        if (isPulling)
        {
            PullNearbyBubbles();
        }
    }

    void PullNearbyBubbles()
    {
        Collider2D[] nearbyBubbles = Physics2D.OverlapCircleAll(transform.position, pullRadius, bubbleLayer);

        foreach (Collider2D bubbleCollider in nearbyBubbles)
        {
            if (bubbleCollider != null)
            {
                BubbleController bubble = bubbleCollider.GetComponent<BubbleController>();
                Rigidbody2D bubbleRb = bubbleCollider.GetComponent<Rigidbody2D>();

                if (bubble != null && bubbleRb != null)
                {
                    Vector2 pullDirection = (transform.position - bubbleCollider.transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, bubbleCollider.transform.position);

                    float distanceFactor = Mathf.Clamp01(distance / pullRadius);
                    float currentPullForce = pullForce * distanceFactor;

                    Vector2 pullVelocity = pullDirection * currentPullForce;
                    pullVelocity = Vector2.ClampMagnitude(pullVelocity, maxPullSpeed);

                    bubbleRb.velocity = pullVelocity;
                    bubble.SetPullState(true);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showPullRadius)
        {
            Gizmos.color = isPulling ? Color.yellow : new Color(1, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, pullRadius);
        }
    }
}