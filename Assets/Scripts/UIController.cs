using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public BorderController borderController;
    
    [Header("Player HP")]
    public List<Sprite> playerHPFrames = new List<Sprite>();
    public SpriteRenderer playerHPBackground;

    [Header("Enemy HP")]
    public List<Sprite> enemyHPFrames = new List<Sprite>();
    public SpriteRenderer enemyHPBackground;

    [Header("Ball Info")]
    public TMP_Text speedTextbox;

    private SpriteRenderer playerHPCurrentFrame;
    private SpriteRenderer enemyHPCurrentFrame;
    private int lastKnownSpeed;
    private GameController gameController;

    private void Awake()
    {
        gameController = FindAnyObjectByType<GameController>();
    }

    private void Update()
    {
        float speed = BallController.Instance != null ? BallController.Instance.rb.linearVelocity.magnitude : 0f;
        int currentSpeed = Mathf.RoundToInt(speed);

        if (currentSpeed > 0)
        {
            lastKnownSpeed = currentSpeed;
        }

        int displaySpeed = currentSpeed == 0 ? lastKnownSpeed : currentSpeed;
        if (speedTextbox != null)
        {
            speedTextbox.text = displaySpeed.ToString();
        }

        UpdateHPFrames();
    }

    public void FadeIn(params SpriteRenderer[] spriteRenderers)
    {
        StartCoroutine(FadeToAlpha(1f, spriteRenderers));
    }

    public void FadeOut(params SpriteRenderer[] spriteRenderers)
    {
        StartCoroutine(FadeToAlpha(0f, spriteRenderers));
    }

    public void SetupHP()
    {
        playerHPCurrentFrame = SpawnFrameAsChild(playerHPFrames, playerHPBackground, "PlayerHPFrame");
        enemyHPCurrentFrame = SpawnFrameAsChild(enemyHPFrames, enemyHPBackground, "EnemyHPFrame");

        FadeIn(playerHPBackground, playerHPCurrentFrame, enemyHPBackground, enemyHPCurrentFrame);
    }

    private SpriteRenderer SpawnFrameAsChild(List<Sprite> frames, SpriteRenderer background, string childName)
    {
        Transform existingChild = background.transform.Find(childName);
        if (existingChild != null)
        {
            Destroy(existingChild.gameObject);
        }

        GameObject frameObject = new GameObject(childName);
        frameObject.transform.SetParent(background.transform, false);
        frameObject.transform.localPosition = Vector3.zero;
        frameObject.transform.localRotation = Quaternion.identity;
        frameObject.transform.localScale = Vector3.one;

        SpriteRenderer frameRenderer = frameObject.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = frames[0];
        frameRenderer.sortingLayerID = background.sortingLayerID;
        frameRenderer.sortingOrder = background.sortingOrder + 1;
        Color frameColor = frameRenderer.color;
        frameColor.a = 0f;
        frameRenderer.color = frameColor;

        return frameRenderer;
    }

    private IEnumerator FadeToAlpha(float targetAlpha, params SpriteRenderer[] spriteRenderers)
    {
        const float fadeDuration = 1f;
        float elapsed = 0f;

        List<float> startAlphas = new List<float>();

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer renderer = spriteRenderers[i];
            startAlphas.Add(renderer.color.a);
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                SpriteRenderer renderer = spriteRenderers[i];
                Color color = renderer.color;
                color.a = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                renderer.color = color;
            }

            yield return null;
        }
    }

    private void UpdateHPFrames()
    {
        if (gameController == null)
        {
            gameController = FindAnyObjectByType<GameController>();
        }

        if (gameController == null)
        {
            return;
        }

        ApplyHPFrame(playerHPCurrentFrame, playerHPFrames, gameController.player1HP);
        ApplyHPFrame(enemyHPCurrentFrame, enemyHPFrames, gameController.player2HP);
    }

    private void ApplyHPFrame(SpriteRenderer targetRenderer, List<Sprite> frames, int hp)
    {
        if (targetRenderer == null || frames == null || frames.Count < 3)
        {
            return;
        }

        if (hp >= 3)
        {
            targetRenderer.sprite = frames[0];
            SetSpriteAlpha(targetRenderer, 1f);
            return;
        }

        if (hp == 2)
        {
            targetRenderer.sprite = frames[1];
            SetSpriteAlpha(targetRenderer, 1f);
            return;
        }

        if (hp == 1)
        {
            targetRenderer.sprite = frames[2];
            SetSpriteAlpha(targetRenderer, 1f);
            return;
        }

        targetRenderer.sprite = frames[0];
        SetSpriteAlpha(targetRenderer, 0f);
    }

    private void SetSpriteAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
