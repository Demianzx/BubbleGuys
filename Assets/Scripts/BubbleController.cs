using System.Collections;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float floatSpeed = 2f;
    [SerializeField] float maxVelocity = 5f;
    [SerializeField] float dampingForce = 0.98f;
    [SerializeField] float influencedDampingForce = 0.9f;

    [Header("Corner Detection Settings")]
    [SerializeField] float slideForce = 8f;              // Fuerza de deslizamiento en esquinas
    [SerializeField] float cornerEscapeForce = 15f;      // Fuerza extra para escapar de esquinas
    [SerializeField] float cornerDetectionAngle = 90f;   // Ángulo para detectar esquinas

    [Header("Collision Settings")]
    [SerializeField] float pushForce = 5f;  
    [SerializeField] float movementThreshold = 0.1f;

    [Header("Size Settings")]
    [SerializeField] float initialSize = 1f;
    [SerializeField] float shrinkAmount = 0.2f;    // Cantidad que se reduce por golpe
    [SerializeField] float minSize = 0.5f;         // Tamaño mínimo antes de explotar
    [SerializeField] float shrinkDuration = 0.3f;  // Duración de la animación de encogimiento

    private float currentSizePercentage = 1f;      // 1 = 100% del tamaño
    private bool isShrinking = false;

    [Header("References")]
    [SerializeField] Transform visualsTransform;

    private Rigidbody2D myRigidbody;
    private CircleCollider2D myCollider;
    private Animator myAnimator;
    private bool isBeingInfluenced = false;
    private bool isBeingPulled = false;
    private LayerMask collisionMask;
    private float currentFloatSpeed;
    private ContactPoint2D[] contacts = new ContactPoint2D[4];
    private int contactCount;
    private bool isInCorner = false;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<CircleCollider2D>();
        SetupComponents();
        currentFloatSpeed = floatSpeed;
    }

    private void Start()
    {
        transform.localScale = Vector3.one * initialSize;
        currentSizePercentage = 1f;
    }

    private void SetupComponents()
    {
        myRigidbody.gravityScale = 0f;
        myRigidbody.drag = 0.5f;
        myRigidbody.angularDrag = 0.5f;
        myRigidbody.mass = 1f;
        myRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        myRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        myRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (visualsTransform != null)
        {
            myAnimator = visualsTransform.GetComponent<Animator>();
        }

        collisionMask = ~LayerMask.GetMask("Background");
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Background"), true);
    }

    void FixedUpdate()
    {
        CheckForCorner();
        if (isInCorner)
        {
            HandleCornerEscape();
        }
        else
        {
            HandleVerticalMovement();
        }

        ApplyDamping();
        LimitVelocity();
        UpdateAnimation();
    }

    private void CheckForCorner()
    {
        contactCount = myCollider.GetContacts(contacts);

        if (contactCount >= 2)
        {
            // Verificar si los puntos de contacto forman una esquina
            for (int i = 0; i < contactCount - 1; i++)
            {
                for (int j = i + 1; j < contactCount; j++)
                {
                    float angle = Vector2.Angle(contacts[i].normal, contacts[j].normal);
                    if (angle <= cornerDetectionAngle)
                    {
                        isInCorner = true;
                        return;
                    }
                }
            }
        }

        isInCorner = false;
    }

    private void HandleCornerEscape()
    {
        // Calcular dirección promedio de escape basada en las normales de contacto
        Vector2 escapeDirection = Vector2.zero;
        for (int i = 0; i < contactCount; i++)
        {
            escapeDirection += contacts[i].normal;
        }
        escapeDirection.Normalize();

        // Añadir componente vertical para ayudar a escapar
        escapeDirection += Vector2.up * 0.5f;
        escapeDirection.Normalize();

        // Aplicar fuerza de escape
        myRigidbody.AddForce(escapeDirection * cornerEscapeForce, ForceMode2D.Force);

        // Aplicar fuerza de deslizamiento
        Vector2 slideDirection = new Vector2(escapeDirection.y, -escapeDirection.x);
        myRigidbody.AddForce(slideDirection * slideForce, ForceMode2D.Force);
    }

    private void HandleVerticalMovement()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, myCollider.radius * 2f, collisionMask);

        if (!hit)
        {
            Vector2 currentVelocity = myRigidbody.velocity;
            float targetYVelocity = isBeingInfluenced || isBeingPulled ?
                                  currentFloatSpeed * 0.5f :
                                  currentFloatSpeed;

            currentVelocity.y = Mathf.Lerp(currentVelocity.y, targetYVelocity, Time.fixedDeltaTime * 2f);
            myRigidbody.velocity = currentVelocity;
        }
    }

    private void ApplyDamping()
    {
        if (!isInCorner)
        {
            Vector2 velocity = myRigidbody.velocity;
            velocity.x *= isBeingInfluenced || isBeingPulled ? influencedDampingForce : dampingForce;
            myRigidbody.velocity = velocity;
        }
    }

    private void LimitVelocity()
    {
        if (myRigidbody.velocity.magnitude > maxVelocity)
        {
            myRigidbody.velocity = Vector2.ClampMagnitude(myRigidbody.velocity, maxVelocity);
        }
    }

    private void UpdateAnimation()
    {
        if (myAnimator != null)
        {
            bool isMovingX = Mathf.Abs(myRigidbody.velocity.x) > movementThreshold;
            bool isMovingY = Mathf.Abs(myRigidbody.velocity.y) > movementThreshold;

            myAnimator.SetBool("isMovingX", isMovingX);
            myAnimator.SetBool("isMovingY", isMovingY);
        }
    }

    public void SetInfluenceState(bool influenced)
    {
        isBeingInfluenced = influenced;
    }

    public void SetPullState(bool pulled)
    {
        isBeingPulled = pulled;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazards"))
        {
            ShrinkBubble();
        }
    }

    private void HandlePlayerCollision(Collision2D collision)
    {
        Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
        pushDirection.y += 0.5f;
        pushDirection.Normalize();

        myRigidbody.velocity = Vector2.zero;
        myRigidbody.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
    }

    private void ShrinkBubble()
    {
        if (isShrinking) return; // Evitar múltiples encogimientos simultáneos

        float newSizePercentage = currentSizePercentage - shrinkAmount;

        // Verificar si la burbuja explotará
        if (newSizePercentage * initialSize < minSize)
        {
            StartBubbleDeathSequence();
            return;
        }

        // Iniciar la animación de encogimiento
        StartCoroutine(ShrinkAnimation(newSizePercentage));
    }

    private IEnumerator ShrinkAnimation(float targetSizePercentage)
    {
        isShrinking = true;
        float startSize = currentSizePercentage;
        float elapsed = 0f;

        // Actualizar el animator si existe
        if (myAnimator != null)
        {
            myAnimator.SetTrigger("takeDamage");
        }

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;

            // Usar una curva de easing para suavizar la animación
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic easing out

            currentSizePercentage = Mathf.Lerp(startSize, targetSizePercentage, easedT);
            float currentSize = initialSize * currentSizePercentage;

            // Actualizar la escala de la burbuja
            transform.localScale = Vector3.one * currentSize;

            // Actualizar el collider
            if (myCollider != null)
            {
                myCollider.radius = currentSize * 0.45f;
            }

            yield return null;
        }

        // Asegurar que llegamos al tamaño exacto
        currentSizePercentage = targetSizePercentage;
        transform.localScale = Vector3.one * (initialSize * currentSizePercentage);

        isShrinking = false;
    }

    private void StartBubbleDeathSequence()
    {
        // Desactivar interacciones físicas
        myRigidbody.simulated = false;
        myCollider.enabled = false;

        // Activar animación de muerte si existe
        if (myAnimator != null)
        {
            myAnimator.SetTrigger("isDying");

            // Si hay una animación de muerte, esperar a que termine
            float animationLength = GetAnimationLength("Death");
            if (animationLength > 0)
            {
                StartCoroutine(WaitForDeathAnimation(animationLength));
                return;
            }
        }

        // Si no hay animación, usar el efecto de escala
        StartCoroutine(DeathScaleEffect());
    }

    private float GetAnimationLength(string clipName)
    {
        if (myAnimator != null)
        {
            AnimationClip[] clips = myAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains(clipName))
                {
                    return clip.length;
                }
            }
        }
        return 0f;
    }

    private IEnumerator WaitForDeathAnimation(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(DeathScaleEffect());
    }

    private IEnumerator DeathScaleEffect()
    {
        float growDuration = 0.3f;
        float shrinkDuration = 0.5f;
        float pauseDuration = 0.2f;

        Vector3 originalScale = transform.localScale;
        Vector3 maxScale = originalScale * 1.5f;
        Vector3 finalScale = Vector3.zero;

        // Fase de crecimiento
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            transform.localScale = Vector3.Lerp(originalScale, maxScale, t);
            yield return null;
        }

        // Pausa breve en el tamaño máximo
        yield return new WaitForSeconds(pauseDuration);

        // Fase de encogimiento
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(maxScale, finalScale, t);
            yield return null;
        }

        // Esperar un momento antes de finalizar
        yield return new WaitForSeconds(0.1f);

        FinishDeath();
    }

    private void FinishDeath()
    {
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        // Notificar la muerte al GameSession
        FindObjectOfType<GameSession>().ProcessPlayerDeath();

        // Esperar un momento antes de destruir el objeto
        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}