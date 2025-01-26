using UnityEngine;

public class BubbleInfluenceField : MonoBehaviour
{
    [Header("Field Settings")]
    [SerializeField] float influenceRadius = 3f;
    [SerializeField] float influenceStrength = 2f;
    [SerializeField] LayerMask bubbleLayer;

    [Header("Debug")]
    [SerializeField] bool showDebugGizmos = false;

    private Rigidbody2D playerRigidbody;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Find all bubbles in range
        Collider2D[] nearbyBubbles = Physics2D.OverlapCircleAll(transform.position, influenceRadius, bubbleLayer);

        foreach (Collider2D bubbleCollider in nearbyBubbles)
        {
            Rigidbody2D bubbleRb = bubbleCollider.GetComponent<Rigidbody2D>();
            if (bubbleRb != null)
            {
                // Calculate distance factor (stronger influence when closer)
                float distance = Vector2.Distance(transform.position, bubbleCollider.transform.position);
                float influenceFactor = 1 - (distance / influenceRadius);

                // Calculate the influence direction based on player's movement
                Vector2 playerVelocity = playerRigidbody.velocity;
                if (playerVelocity.magnitude < 0.1f)
                {
                    continue; // Skip if player is not moving
                }

                // Apply influence force
                Vector2 influenceForce = playerVelocity.normalized * influenceStrength * influenceFactor;
                bubbleRb.AddForce(influenceForce, ForceMode2D.Force);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, influenceRadius);
        }
    }
}