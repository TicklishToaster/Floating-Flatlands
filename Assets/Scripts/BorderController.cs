using UnityEngine;

public class BorderController : MonoBehaviour
{
    [Header("Reserved Visual References")]
    [SerializeField] private SpriteMask outlineMask;
    [SerializeField] private SpriteRenderer outlineRenderer;

    [Header("Scorch Marks")]
    [SerializeField] public Sprite[] scorchSprites;
    [SerializeField] public Sprite[] scorchSpritesAlpha;
    [SerializeField] private float baseScorchFadeDelay = 1f;
    [SerializeField] private float baseScorchFadeDuration = 2f;
    [SerializeField] private float alphaScorchFadeDelay = 1f;
    [SerializeField] private float alphaScorchFadeDuration = 2f;

    private int lastImpactFrame = -1;
    private GameObject lastImpactBall;

    private void Awake()
    {
        RegisterCollisionRelays();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        ProcessImpact(collision.gameObject, impactPoint);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 impactPoint = other.ClosestPoint(transform.position);
        ProcessImpact(other.gameObject, impactPoint);
    }

    private bool IsBall(GameObject other)
    {
        return other.GetComponent<BallController>() != null
            || other.GetComponentInParent<BallController>() != null
            || other.GetComponentInChildren<BallController>() != null;
    }

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

        TrySpawnScorchMark(impactPoint);
    }

    private float GetScorchScaleMultiplier(GameObject other)
    {
        BallController ballController = other.GetComponent<BallController>()
            ?? other.GetComponentInParent<BallController>()
            ?? other.GetComponentInChildren<BallController>();

        Rigidbody2D ballRb = ballController.GetComponent<Rigidbody2D>();
        float speed = ballRb != null ? ballRb.linearVelocity.magnitude : 0f;

        if (speed < 12f)
        {
            return 1f;
        }

        if (speed < 14f)
        {
            return 1.1f;
        }

        if (speed < 16f)
        {
            return 1.2f;
        }

        if (speed < 18f)
        {
            return 1.3f;
        }

        if (speed < 20f)
        {
            return 1.4f;
        }

        return 1.5f;
    }

    private void TrySpawnScorchMark(Vector2 impactPoint)
    {
        int spriteIndex = Random.Range(0, scorchSprites.Length);
        float randomScale = Random.Range(0.8f, 1.4f);
        float scaleMultiplier = GetScorchScaleMultiplier(lastImpactBall);
        randomScale *= scaleMultiplier;

        GameObject scorchObject = new GameObject("ScorchMark");
        scorchObject.transform.position = impactPoint;
        scorchObject.transform.SetParent(transform, false);

        SpriteRenderer baseRenderer = scorchObject.AddComponent<SpriteRenderer>();
        baseRenderer.sprite = scorchSprites[spriteIndex];
        baseRenderer.sortingLayerName = "SFX Border 1";
        baseRenderer.sortingOrder = 1;
        baseRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        baseRenderer.color = new Color(1f, 1f, 1f, 0.75f);

        scorchObject.transform.localScale = Vector3.one * randomScale;

        ScorchFadeEffect baseScorchFade = scorchObject.AddComponent<ScorchFadeEffect>();
        baseScorchFade.Initialize(0.75f, baseScorchFadeDelay, baseScorchFadeDuration);

        GameObject alphaObject = new GameObject("ScorchAlpha");
        alphaObject.transform.position = impactPoint;
        alphaObject.transform.SetParent(transform, false);

        SpriteRenderer alphaRenderer = alphaObject.AddComponent<SpriteRenderer>();
        alphaRenderer.sprite = scorchSpritesAlpha[spriteIndex];
        alphaRenderer.sortingLayerName = "SFX Border 2";
        alphaRenderer.sortingOrder = 2;
        alphaRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        alphaRenderer.color = new Color(1f, 1f, 1f, 0.75f);

        alphaObject.transform.localScale = Vector3.one * randomScale * 0.4f;

        ScorchFadeEffect alphaScorchFade = alphaObject.AddComponent<ScorchFadeEffect>();
        alphaScorchFade.InitializeColorTransition(0.75f, alphaScorchFadeDelay, alphaScorchFadeDuration,
            new Color(1f, 0.478431f, 0.102f),
            new Color(0.105882f, 0.105882f, 0.105882f));
    }

    private void RegisterCollisionRelays()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            BorderCollisionRelay relay = colliders[i].GetComponent<BorderCollisionRelay>();
            if (relay == null)
            {
                relay = colliders[i].gameObject.AddComponent<BorderCollisionRelay>();
            }

            relay.Initialize(this);
        }
    }

    public void RelayCollision(Collision2D collision)
    {
        Vector2 impactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        ProcessImpact(collision.gameObject, impactPoint);
    }

    public void RelayTrigger(Collider2D other)
    {
        ProcessImpact(other.gameObject, other.ClosestPoint(transform.position));
    }
}

public class BorderCollisionRelay : MonoBehaviour
{
    private BorderController controller;

    public void Initialize(BorderController borderController)
    {
        controller = borderController;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        controller.RelayCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        controller.RelayTrigger(other);
    }
}

public class ScorchFadeEffect : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private float timer;
    private float startAlpha;
    private float fadeDelay;
    private float fadeDuration;
    private Color startColor = Color.white;
    private Color endColor = Color.white;
    private bool useColorTransition;

    public void Initialize(float initialAlpha, float delay, float duration)
    {
        startAlpha = Mathf.Clamp01(initialAlpha);
        fadeDelay = delay;
        fadeDuration = duration;
        useColorTransition = false;

        if (renderers == null)
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            Color color = renderers[i].color;
            color.a = startAlpha;
            renderers[i].color = color;
        }
    }

    public void InitializeColorTransition(float initialAlpha, float delay, float duration, Color colorStart, Color colorEnd)
    {
        Initialize(initialAlpha, delay, duration);
        startColor = colorStart;
        endColor = colorEnd;
        useColorTransition = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            renderers[i].color = new Color(startColor.r, startColor.g, startColor.b, startAlpha);
        }
    }

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float currentAlpha = startAlpha;
        Color currentColor = Color.white;

        if (useColorTransition && timer < fadeDelay)
        {
            float colorProgress = Mathf.Clamp01(timer / fadeDelay);
            currentColor = Color.Lerp(startColor, endColor, colorProgress);
        }
        else if (useColorTransition)
        {
            currentColor = endColor;
        }
        else
        {
            currentColor = renderers != null && renderers.Length > 0 ? renderers[0].color : Color.white;
            currentColor.a = startAlpha;
        }

        if (timer >= fadeDelay)
        {
            float fadeProgress = Mathf.Clamp01((timer - fadeDelay) / fadeDuration);
            currentAlpha = Mathf.Lerp(startAlpha, 0f, fadeProgress);
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            Color color = useColorTransition ? currentColor : renderers[i].color;
            color.a = currentAlpha;
            renderers[i].color = color;
        }

        if (timer >= fadeDelay + fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}
