using UnityEngine;

public class BubbleHunterBullet : MonoBehaviour
{
    [SerializeField] float bulletSpeed = 20f;

    private Rigidbody2D myRigidbody;
    private Vector2 direction;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;

        // Rotate bullet sprite to face movement direction
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void Start()
    {
        // Apply velocity in the set direction
        myRigidbody.velocity = direction * bulletSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
    }
}