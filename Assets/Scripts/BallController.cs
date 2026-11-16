using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] public float bouncePower = 1f;
    [SerializeField] public float bounceAngleRange = 2f;

    [Header("Visuals")]
    [SerializeField] public SpriteRenderer vignetteOverlay;

    [Header("Audio")]
    public AudioSource ballAudioSource;

    [Header("Shoot Audio")]
    public AudioClip ShootSound1;
    public AudioClip ShootSound2;
    public AudioClip ShootSound3;
    [Range(0f, 1f)] public float shootVolume = 1f;

    [Header("Shield Hit Audio")]
    public AudioClip ShieldHit1;
    public AudioClip ShieldHit2;
    public AudioClip ShieldHit3;
    [Range(0f, 1f)] public float shieldHitVolume = 1f;

    [Header("Shield Bounce Audio")]
    public AudioClip ShieldBounce1;
    public AudioClip ShieldBounce2;
    public AudioClip ShieldBounce3;
    [Range(0f, 1f)] public float shieldBounceVolume = 1f;

    [Header("Wall Bounce Audio")]
    public AudioClip WallBounce1;
    public AudioClip WallBounce2;
    public AudioClip WallBounce3;
    public AudioClip WallBounce4;
    [Range(0f, 1f)] public float wallBounceVolume = 1f;

    [Header("Strong Wall Bounce Audio")]
    public AudioClip StrongWallBounce1;
    public AudioClip StrongWallBounce2;
    public AudioClip StrongWallBounce3;
    [Range(0f, 1f)] public float strongWallBounceVolume = 1f;

    private const float StrongImpactSpeed = 12f;

    public Rigidbody2D rb { get; private set; }
    private GameObject lastHitObject;
    private Vector2 previousVelocity;

    private bool isHeld;
    private Transform heldAnchor;
    private ShieldController heldShieldController;
    private Vector2 heldLocalOffset;
    private Vector2 heldReleaseVelocity;
    private float heldTimeRemaining;

    public static BallController Instance { get; private set; }
    public bool IsHeld => isHeld;
    public bool IsHeldOnPlayerOneSide => isHeld && heldAnchor != null && heldAnchor.position.x < 0f;
    public bool IsHeldOnPlayerTwoSide => isHeld && heldAnchor != null && heldAnchor.position.x > 0f;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnDisable()
    {
        if (isHeld)
        {
            isHeld = false;
            heldAnchor = null;
            heldShieldController = null;
        }
    }

    private void Start()
    {
        SetRandomDirection();
    }

    private void FixedUpdate()
    {
        if (isHeld)
        {
            UpdateHold();
            return;
        }

        previousVelocity = rb.linearVelocity;

        if (rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            SetRandomDirection();
            return;
        }

        float currentSpeed = rb.linearVelocity.magnitude;
        if (currentSpeed < minSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }
        else if (currentSpeed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        UpdateVignetteAlpha();
    }

    private void UpdateVignetteAlpha()
    {
        if (vignetteOverlay == null)
        {
            return;
        }

        float speedValue = rb.linearVelocity.magnitude;
        float alpha;

        if (speedValue <= 10f)
        {
            alpha = 0f;
        }
        else
        {
            alpha = Mathf.Clamp01((speedValue - 10f) * 0.05f);
        }

        Color current = vignetteOverlay.color;
        current.a = alpha;
        vignetteOverlay.color = current;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHeld)
        {
            return;
        }

        if (collision.collider == null || collision.collider.isTrigger)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Ball"))
        {
            return;
        }

        if (lastHitObject == collision.gameObject)
        {
            return;
        }

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;

        if (Vector2.Dot(previousVelocity, normal) >= 0f)
        {
            return;
        }

        Vector2 reflected = Vector2.Reflect(previousVelocity.normalized, normal);

        if (collision.gameObject.CompareTag("Wall"))
        {
            float xSign = Mathf.Sign(reflected.x);
            if (xSign == 0f)
            {
                xSign = Mathf.Sign(previousVelocity.x);
            }

            float jitterDegrees;
            if (xSign >= 0f)
            {
                jitterDegrees = Random.Range(0f, bounceAngleRange);
            }
            else
            {
                jitterDegrees = Random.Range(-bounceAngleRange, 0f);
            }

            float angle = Mathf.Atan2(reflected.y, reflected.x) * Mathf.Rad2Deg + jitterDegrees;
            Vector2 jittered = Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            if (xSign != 0f)
            {
                jittered.x = Mathf.Abs(jittered.x) * xSign;
            }

            reflected = jittered;
        }

        if (reflected.sqrMagnitude < 0.01f)
        {
            reflected = Vector2.Reflect(previousVelocity.normalized, Vector2.right);
        }

        rb.position += normal * 0.02f;
        float adjustedSpeed = Mathf.Clamp(previousVelocity.magnitude + bouncePower, minSpeed, maxSpeed);
        Vector2 newVelocity = reflected.normalized * adjustedSpeed;

        if (collision.gameObject.CompareTag("Shield"))
        {
            PlayRandomSound(shieldHitVolume, ShieldHit1, ShieldHit2, ShieldHit3);

            ShieldController shield = collision.collider.GetComponentInParent<ShieldController>();
            if (shield != null)
            {
                BeginHold(shield.transform, shield.RecoilOutDurationSeconds, newVelocity, shield);
                shield.SetAimDirection(newVelocity);
                lastHitObject = collision.gameObject;
                return;
            }
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            if (adjustedSpeed >= StrongImpactSpeed)
            {
                PlayRandomSound(strongWallBounceVolume, StrongWallBounce1, StrongWallBounce2, StrongWallBounce3);
            }
            else
            {
                PlayRandomSound(wallBounceVolume, WallBounce1, WallBounce2, WallBounce3, WallBounce4);
            }
        }

        rb.linearVelocity = newVelocity;
        lastHitObject = collision.gameObject;
    }

    // Plays one randomly chosen clip from the given set through the ball's audio source.
    private void PlayRandomSound(float volume, params AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        ballAudioSource.PlayOneShot(clip, volume);
    }

    // Freezes the ball against the shield so it moves with it, then releases it with the current aim direction.
    private void BeginHold(Transform anchor, float duration, Vector2 releaseVelocity, ShieldController shield)
    {
        isHeld = true;
        heldAnchor = anchor;
        heldShieldController = shield;
        heldLocalOffset = anchor.InverseTransformPoint(rb.position);
        heldReleaseVelocity = releaseVelocity;
        heldTimeRemaining = duration;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void UpdateHold()
    {
        heldTimeRemaining -= Time.fixedDeltaTime;
        rb.MovePosition(heldAnchor.TransformPoint(heldLocalOffset));

        if (heldTimeRemaining <= 0f)
        {
            EndHold();
        }
    }

    private void EndHold()
    {
        isHeld = false;
        heldAnchor = null;

        Vector2 launchVelocity = heldShieldController != null
            ? heldShieldController.GetCurrentAimVelocity(Mathf.Clamp(heldReleaseVelocity.magnitude, minSpeed, maxSpeed))
            : heldReleaseVelocity;

        heldShieldController = null;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = launchVelocity;
        previousVelocity = launchVelocity;
        lastHitObject = null;

        if (launchVelocity.magnitude >= StrongImpactSpeed)
        {
            PlayRandomSound(shootVolume, ShootSound1, ShootSound2, ShootSound3);
        }
        else
        {
            PlayRandomSound(shieldBounceVolume, ShieldBounce1, ShieldBounce2, ShieldBounce3);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (lastHitObject == collision.gameObject)
        {
            lastHitObject = null;
        }
    }

    private Vector2 GetBounceNormal(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            Vector2 normal = Vector2.zero;

            for (int i = 0; i < collision.contactCount; i++)
            {
                normal += collision.GetContact(i).normal;
            }

            if (normal.sqrMagnitude > 0.0001f)
            {
                return normal.normalized;
            }
        }

        Vector2 delta = (Vector2)transform.position - (Vector2)collision.collider.bounds.center;
        return delta != Vector2.zero ? delta.normalized : Vector2.up;
    }

    private void SetRandomDirection()
    {
        int randomDirection = Random.Range(0, 4);
        Vector2 direction;

        switch (randomDirection)
        {
            case 0:
                direction = new Vector2(1f, 1f).normalized;
                break;
            case 1:
                direction = new Vector2(-1f, 1f).normalized;
                break;
            case 2:
                direction = new Vector2(1f, -1f).normalized;
                break;
            default:
                direction = new Vector2(-1f, -1f).normalized;
                break;
        }

        rb.linearVelocity = direction * speed;
    }

    public void SetDirection(float angleDegrees)
    {
        Vector2 direction = Quaternion.Euler(0f, 0f, angleDegrees) * Vector2.right;
        rb.linearVelocity = direction.normalized * Mathf.Clamp(speed, minSpeed, maxSpeed);
    }
}
