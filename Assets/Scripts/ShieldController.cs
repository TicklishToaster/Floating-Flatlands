using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [Header("Shield Visuals")]
    [SerializeField] private GameObject shieldFill;
    [SerializeField] private GameObject shieldOutline;
    [SerializeField] private SpriteMask shieldOutlineMask;

    [Header("Animation Targets")]
    [SerializeField] private SpriteRenderer shieldPulse;
    [SerializeField] private SpriteRenderer shieldScan;
    [SerializeField] public SpriteRenderer aimArrow;
    [SerializeField] public SpriteRenderer aimCircle;

    [Header("Aiming")]
    [SerializeField] [Min(0f)] private float aimRotateSpeed = 90f;
    [SerializeField] [Range(0f, 90f)] private float aimMaxAngle = 45f;

    [Header("Animation Frames")]
    public Sprite[] shieldPulseSprites = new Sprite[0];
    public Sprite[] shieldScanSprites = new Sprite[0];

    [Header("Glow")]
    [SerializeField] private Sprite shieldGlow;
    [SerializeField] private Color glowColor = new Color32(255, 255, 255, 255);
    [Min(0f)] public float GlowMaxScale = 1f;

    [Header("Animation Timing")]
    [SerializeField] [Min(0.1f)] private float shieldAnimationFps = 12f;

    private const float GlowLifetimeSeconds = 2f;
    private const float GlowGrowDurationSeconds = 0.25f;
    private const float GlowShrinkStartSeconds = 1.75f;
    private const float ShieldRecoilPixels = 16f;
    private const float ShieldRecoilOutDurationSeconds = 0.5f;
    private const float ShieldRecoilReturnDurationSeconds = 0.25f;
    private const float DefaultPixelsPerUnit = 100f;
    private int lastImpactFrame = -1;
    private GameObject lastImpactBall;
    private Coroutine shieldRecoilCoroutine;
    private Vector3 shieldStartLocalPosition;
    private Transform shieldPulseTransform;
    private Transform shieldScanTransform;
    private int shieldPulseFrameIndex;
    private int shieldScanFrameIndex;
    private float shieldPulseTimer;
    private float shieldScanTimer;
    private bool isAiming;
    private float aimCenterAngle;
    private float aimUpSign;
    private float aimAngleOffset;

    // Duration the ball stays held against the shield while it recoils outward.
    public float RecoilOutDurationSeconds => ShieldRecoilOutDurationSeconds;

    // Initializes references and one-time setup used by runtime effects.
    private void Awake()
    {
        CacheShieldRenderers();
        ApplyInitialAnimationFrames();
        CacheShieldStartPositions();
        RegisterCollisionRelays();
    }

    // Loops shield sprite animations using the configured frame arrays.
    private void Update()
    {
        UpdateAimRotation();

        float frameDuration = 1f / shieldAnimationFps;
        AnimateFrameLoop(shieldPulse, shieldPulseSprites, ref shieldPulseFrameIndex, ref shieldPulseTimer, frameDuration);
        AnimateFrameLoop(shieldScan, shieldScanSprites, ref shieldScanFrameIndex, ref shieldScanTimer, frameDuration);
    }

    // Handles physics-based 2D collisions and forwards the impact point for effect processing.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        ProcessImpact(collision.gameObject, impactPoint);
    }

    // Handles trigger-based 2D collisions and forwards the impact point for effect processing.
    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 impactPoint = other.ClosestPoint(transform.position);
        ProcessImpact(other.gameObject, impactPoint);
    }

    // Accepts direct, parented, or child ball references so nested collider setups still work.
    private bool IsBall(GameObject other)
    {
        return other.GetComponent<BallController>() != null
            || other.GetComponentInParent<BallController>() != null
            || other.GetComponentInChildren<BallController>() != null;
    }

    // Validates impact events, suppresses duplicates in the same frame, then triggers all reactions.
    private void ProcessImpact(GameObject other, Vector2 impactPoint)
    {
        if (!IsBall(other))
        {
            return;
        }

        if (lastImpactFrame == Time.frameCount && lastImpactBall == other)
        {
            return;
        }

        lastImpactFrame = Time.frameCount;
        lastImpactBall = other;

        SpawnGlow(impactPoint);
        UpdateAimVisuals(impactPoint);
        TryAnimateShieldRecoil(impactPoint);
    }

    // Spawns the glow overlay at impact and starts its grow/hold/shrink lifetime animation.
    private void SpawnGlow(Vector2 impactPoint)
    {
        Transform glowParent = shieldOutline.transform;
        GameObject glowObject = new GameObject("Shield Glow FX");
        glowObject.transform.SetParent(glowParent, true);
        glowObject.transform.position = new Vector3(impactPoint.x, impactPoint.y, glowParent.position.z);
        glowObject.transform.localScale = Vector3.zero;

        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = shieldGlow;
        glowRenderer.color = glowColor;
        glowRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        SpriteRenderer sortingRenderer = shieldOutline.GetComponent<SpriteRenderer>();
        glowRenderer.sortingLayerID = sortingRenderer.sortingLayerID;
        glowRenderer.sortingOrder = sortingRenderer.sortingOrder + 1;

        StartCoroutine(AnimateAndDestroyGlow(glowObject.transform));
    }

    // Animates glow scale in/out and destroys the temporary effect object when finished.
    private IEnumerator AnimateAndDestroyGlow(Transform glowTransform)
    {
        float elapsed = 0f;

        while (elapsed < GlowLifetimeSeconds)
        {
            if (glowTransform == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;

            float scale;
            if (elapsed <= GlowGrowDurationSeconds)
            {
                scale = Mathf.Lerp(0f, GlowMaxScale, elapsed / GlowGrowDurationSeconds);
            }
            else if (elapsed < GlowShrinkStartSeconds)
            {
                scale = GlowMaxScale;
            }
            else
            {
                float shrinkProgress = (elapsed - GlowShrinkStartSeconds) / GlowGrowDurationSeconds;
                scale = Mathf.Lerp(GlowMaxScale, 0f, Mathf.Clamp01(shrinkProgress));
            }

            glowTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        if (glowTransform != null)
        {
            Destroy(glowTransform.gameObject);
        }
    }

    // Selects the renderer used for sorting and pixels-per-unit calculations.
    private SpriteRenderer GetReferenceRenderer()
    {
        return shieldFill.GetComponent<SpriteRenderer>();
    }

    // Shows the aim guides at the impact height during recoil.
    private void UpdateAimVisuals(Vector2 impactPoint)
    {
        if (aimArrow != null)
        {
            aimArrow.enabled = true;

            Vector3 arrowPosition = aimArrow.transform.position;
            arrowPosition.y = impactPoint.y;
            aimArrow.transform.position = arrowPosition;
        }

        if (aimCircle != null)
        {
            aimCircle.enabled = true;
            Vector3 circlePosition = aimCircle.transform.position;
            circlePosition.y = impactPoint.y;
            aimCircle.transform.position = circlePosition;
        }
    }

    // Called by the ball with its exact post-bounce velocity so the arrow points where the ball will travel.
    public void SetAimDirection(Vector2 bounceVelocity)
    {
        aimCenterAngle = bounceVelocity.x >= 0f ? 0f : 180f;
        aimUpSign = bounceVelocity.x >= 0f ? 1f : -1f;
        aimAngleOffset = Mathf.DeltaAngle(aimCenterAngle, Vector2.SignedAngle(Vector2.right, bounceVelocity));
        isAiming = true;

        ApplyAimRotation();
    }

    // Rotates the arrow up/down from player input while the ball is held against this shield.
    private void UpdateAimRotation()
    {
        if (!isAiming)
        {
            return;
        }

        float input = ReadAimInput();
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        aimAngleOffset += input * aimUpSign * aimRotateSpeed * Time.deltaTime;
        aimAngleOffset = Mathf.Clamp(aimAngleOffset, -aimMaxAngle, aimMaxAngle);
        ApplyAimRotation();
    }

    // Reads vertical aim input for the side this shield is on; the enemy side is not player controlled in single player.
    private float ReadAimInput()
    {
        bool isPlayerOneShield = aimUpSign > 0f;

        if (isPlayerOneShield)
        {
            return PlayerMovement.ReadVertical();
        }

        return 0f;
    }

    private void ApplyAimRotation()
    {
        float angleDegrees = aimCenterAngle + aimAngleOffset;
        aimArrow.transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    // Returns the live launch direction implied by the current arrow angle.
    public Vector2 GetCurrentAimDirection()
    {
        float angleDegrees = aimCenterAngle + aimAngleOffset;
        return Quaternion.Euler(0f, 0f, angleDegrees) * Vector2.right;
    }

    // Returns the launch velocity using the current arrow angle and the supplied speed.
    public Vector2 GetCurrentAimVelocity(float magnitude)
    {
        Vector2 direction = GetCurrentAimDirection();
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right * (aimUpSign >= 0f ? 1f : -1f);
        }

        return direction.normalized * magnitude;
    }

    // Starts shield recoil when enabled, resetting active recoil so repeated hits remain stable.
    private void TryAnimateShieldRecoil(Vector2 impactPoint)
    {
        if (shieldRecoilCoroutine != null)
        {
            StopCoroutine(shieldRecoilCoroutine);
        }

        ResetShieldVisualPositions();

        Vector2 recoilDirection = GetShieldRecoilDirection(impactPoint);
        float recoilWorldDistance = GetRecoilWorldDistance();
        shieldRecoilCoroutine = StartCoroutine(AnimateShieldRecoil(recoilDirection, recoilWorldDistance));
    }

    // Determines whether recoil should move along local right or local left based on impact side.
    private Vector2 GetShieldRecoilDirection(Vector2 impactPoint)
    {
        Vector2 toCenter = (Vector2)transform.position - impactPoint;
        if (toCenter.sqrMagnitude < 0.0001f)
        {
            toCenter = transform.right;
        }

        Vector2 right = transform.right;
        float directionSign = Vector2.Dot(toCenter.normalized, right) >= 0f ? 1f : -1f;
        return (right * directionSign).normalized;
    }

    // Converts configured recoil pixels into world units using the active shield sprite PPU.
    private float GetRecoilWorldDistance()
    {
        SpriteRenderer renderer = GetReferenceRenderer();
        float pixelsPerUnit = Mathf.Max(1f, renderer.sprite.pixelsPerUnit);
        return ShieldRecoilPixels / pixelsPerUnit;
    }

    // Animates fill/outline/pulse/scan recoil with out, hold, and return phases.
    private IEnumerator AnimateShieldRecoil(Vector2 recoilDirection, float recoilWorldDistance)
    {
        Vector3 recoilWorldOffset = (Vector3)(recoilDirection * recoilWorldDistance);
        Vector3 localOffset = transform.parent != null
            ? transform.parent.InverseTransformVector(recoilWorldOffset)
            : recoilWorldOffset;
        Vector3 shieldPeak = shieldStartLocalPosition + localOffset;

        if (aimArrow != null)
        {
            aimArrow.enabled = true;
        }

        if (aimCircle != null)
        {
            aimCircle.enabled = true;
        }

        float elapsed = 0f;
        while (elapsed < ShieldRecoilOutDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ShieldRecoilOutDurationSeconds);
            transform.localPosition = Vector3.Lerp(shieldStartLocalPosition, shieldPeak, t);
            yield return null;
        }

        transform.localPosition = shieldPeak;

        isAiming = false;

        if (aimArrow != null)
        {
            aimArrow.enabled = false;
        }

        if (aimCircle != null)
        {
            aimCircle.enabled = false;
        }

        elapsed = 0f;
        while (elapsed < ShieldRecoilReturnDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ShieldRecoilReturnDurationSeconds);
            transform.localPosition = Vector3.Lerp(shieldPeak, shieldStartLocalPosition, t);
            yield return null;
        }

        ResetShieldVisualPositions();
        shieldRecoilCoroutine = null;
    }

    // Caches initial local positions so recoil can always restore exact original placement.
    private void CacheShieldStartPositions()
    {
        shieldStartLocalPosition = transform.localPosition;
    }

    // Caches SpriteRenderer components used for frame-based shield animation.
    private void CacheShieldRenderers()
    {
        shieldPulseTransform = shieldPulse.transform;
        shieldScanTransform = shieldScan.transform;
    }

    // Applies first frame immediately so targets show a valid sprite before the first loop tick.
    private void ApplyInitialAnimationFrames()
    {
        if (shieldPulse != null && shieldPulseSprites.Length > 0)
        {
            shieldPulse.sprite = shieldPulseSprites[0];
        }

        shieldPulseFrameIndex = 0;

        if (shieldScan != null && shieldScanSprites.Length > 0)
        {
            shieldScan.sprite = shieldScanSprites[0];
        }

        shieldScanFrameIndex = 0;
    }

    // Advances a looping frame sequence and updates the target renderer sprite.
    private void AnimateFrameLoop(SpriteRenderer targetRenderer, Sprite[] frames, ref int frameIndex, ref float frameTimer, float frameDuration)
    {
        frameTimer += Time.deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        Sprite currentFrame = frames[frameIndex];
        targetRenderer.sprite = currentFrame;
    }

    // Restores outline and fill visuals to their cached local positions.
    private void ResetShieldVisualPositions()
    {
        transform.localPosition = shieldStartLocalPosition;
    }

    // Adds relay components to child colliders so nested colliders report impacts to this controller.
    private void RegisterCollisionRelays()
    {
        RegisterCollisionRelaysFor(GetComponentsInChildren<Collider2D>(true));
        RegisterCollisionRelaysFor(GetComponentsInParent<Collider2D>(true));
    }

    private void RegisterCollisionRelaysFor(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            ShieldCollisionRelay relay = colliders[i].GetComponent<ShieldCollisionRelay>();
            if (relay == null)
            {
                relay = colliders[i].gameObject.AddComponent<ShieldCollisionRelay>();
            }

            relay.Initialize(this);
        }
    }

    // Receives forwarded collision events from child relay components.
    public void RelayCollision(Collision2D collision)
    {
        Vector2 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        ProcessImpact(collision.gameObject, impactPoint);
    }

    // Receives forwarded trigger events from child relay components.
    public void RelayTrigger(Collider2D other)
    {
        ProcessImpact(other.gameObject, other.ClosestPoint(transform.position));
    }
}

public class ShieldCollisionRelay : MonoBehaviour
{
    private ShieldController controller;

    // Assigns the target ShieldController that should handle forwarded collider callbacks.
    public void Initialize(ShieldController shieldController)
    {
        controller = shieldController;
    }

    // Forwards collision callbacks from this collider object to the owning shield controller.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        controller.RelayCollision(collision);
    }

    // Forwards trigger callbacks from this collider object to the owning shield controller.
    private void OnTriggerEnter2D(Collider2D other)
    {
        controller.RelayTrigger(other);
    }
}
