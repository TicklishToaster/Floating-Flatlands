using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    private const string PortraitObjectName = "DialoguePortrait";
    private const string PortraitSortingLayer = "Screen Layer";
    private const int PortraitSortingOrder = 1;
    private const int PortraitFrameCount = 4;
    private const int TextSoundIntervalCharacters = 2;
    private const float DialogueScaleDuration = 0.25f;
    private const float TextAdvanceFlashDuration = 0.1f;
    private const float PunctuationPauseDuration = 0.25f;
    private const float PortraitFramesPerSecond = 12f;
    private const float PortraitIdleDelayMinSeconds = 4f;
    private const float PortraitIdleDelayMaxSeconds = 8f;

    private static readonly Vector3 DialoguePortraitLocalPosition = new Vector3(-3.52f, 0f, 0f);
    private static readonly Vector3 HiddenScale = Vector3.zero;
    private static readonly Vector3 VisibleScale = new Vector3(2f, 2f, 1f);

    [Header("References")]
    public SpriteRenderer textBackground;
    public SpriteRenderer textArrow;
    public SpriteRenderer textArrowHighlight;
    public List<Sprite> speakerPortrait1;
    public List<Sprite> speakerPortrait2;
    public TMP_Text speakerTextbox;

    [Header("Audio")]
    public AudioClip textSound;
    [Range(0f, 1f)] public float textSoundVolume = 1f;
    public AudioClip textAdvance;
    [Range(0f, 1f)] public float textAdvanceVolume = 1f;
    public AudioClip windowOpen;
    [Range(0f, 1f)] public float windowOpenVolume = 1f;
    public AudioClip windowClose;
    [Range(0f, 1f)] public float windowCloseVolume = 1f;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 30f;

    private Coroutine typewriterCoroutine;
    private Coroutine portraitAnimationCoroutine;
    private Coroutine dialogueScaleCoroutine;
    private Coroutine textArrowHighlightCoroutine;
    private SpriteRenderer spawnedPortrait;
    private List<Sprite> activePortraitFrames;

    public bool IsTypewriterActive => typewriterCoroutine != null;

    public void ShowDialogue(Sprite portraitSprite = null)
    {
        AudioSource.PlayClipAtPoint(windowOpen, transform.position, windowOpenVolume);

        StopDialogueAnimationCoroutines();
        StopScaleCoroutine();
        ClearTextbox();

        if (spawnedPortrait != null)
        {
            Destroy(spawnedPortrait.gameObject);
        }

        Sprite spriteToUse = portraitSprite ?? speakerPortrait1[0];
        activePortraitFrames = GetPortraitFrames(spriteToUse);

        GameObject portraitObject = new GameObject(PortraitObjectName);
        portraitObject.transform.SetParent(textBackground.transform, false);
        portraitObject.transform.localPosition = DialoguePortraitLocalPosition;
        portraitObject.transform.localScale = HiddenScale;

        spawnedPortrait = portraitObject.AddComponent<SpriteRenderer>();
        spawnedPortrait.sprite = activePortraitFrames[0];
        spawnedPortrait.enabled = true;
        spawnedPortrait.sortingLayerName = PortraitSortingLayer;
        spawnedPortrait.sortingOrder = PortraitSortingOrder;

        textBackground.enabled = true;
        textArrow.enabled = true;

        textBackground.transform.localScale = HiddenScale;
        textArrow.transform.localScale = HiddenScale;

        dialogueScaleCoroutine = StartCoroutine(ScaleDialogueElementsRoutine(HiddenScale, VisibleScale, DialogueScaleDuration));
    }

    public void LoadDialogue(string dialogueText, Sprite portraitSprite = null)
    {
        if (!string.IsNullOrEmpty(speakerTextbox.text))
        {
            TriggerAdvanceFeedback();
        }

        if (portraitSprite != null)
        {
            activePortraitFrames = GetPortraitFrames(portraitSprite);
            spawnedPortrait.sprite = activePortraitFrames[0];
        }

        StopDialogueAnimationCoroutines();
        ClearTextbox();
        speakerTextbox.text = dialogueText;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        portraitAnimationCoroutine = StartCoroutine(AnimatePortraitRoutine());
    }

    public void ForceInstantText()
    {
        TriggerAdvanceFeedback();

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        speakerTextbox.ForceMeshUpdate();
        speakerTextbox.maxVisibleCharacters = speakerTextbox.textInfo.characterCount;
    }

    private void TriggerAdvanceFeedback()
    {
        AudioSource.PlayClipAtPoint(textAdvance, transform.position, textAdvanceVolume);

        if (textArrowHighlightCoroutine != null)
        {
            StopCoroutine(textArrowHighlightCoroutine);
        }

        textArrowHighlightCoroutine = StartCoroutine(FlashTextArrowHighlightRoutine());
    }

    private IEnumerator FlashTextArrowHighlightRoutine()
    {
        textArrowHighlight.enabled = true;
        yield return new WaitForSeconds(TextAdvanceFlashDuration);
        textArrowHighlight.enabled = false;
        textArrowHighlightCoroutine = null;
    }

    public void HideDialogue()
    {
        AudioSource.PlayClipAtPoint(windowClose, transform.position, windowCloseVolume);

        StopDialogueAnimationCoroutines();
        StopScaleCoroutine();
        ClearTextbox();
        dialogueScaleCoroutine = StartCoroutine(HideDialogueRoutine());
    }

    private IEnumerator TypewriterRoutine()
    {
        speakerTextbox.ForceMeshUpdate();
        int totalCharacters = speakerTextbox.textInfo.characterCount;

        if (totalCharacters <= 0)
        {
            typewriterCoroutine = null;
            yield break;
        }

        float delay = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
        WaitForSeconds characterDelayWait = delay > 0f ? new WaitForSeconds(delay) : null;
        WaitForSeconds punctuationPauseWait = new WaitForSeconds(PunctuationPauseDuration);

        for (int i = 1; i <= totalCharacters; i++)
        {
            speakerTextbox.maxVisibleCharacters = i;

            if (i % TextSoundIntervalCharacters == 0)
            {
                AudioSource.PlayClipAtPoint(textSound, transform.position, textSoundVolume);
            }

            yield return characterDelayWait ?? null;

            char revealedCharacter = speakerTextbox.textInfo.characterInfo[i - 1].character;
            if (revealedCharacter == '.' || revealedCharacter == ',' || revealedCharacter == '!' || revealedCharacter == '?')
            {
                yield return punctuationPauseWait;
            }
        }

        typewriterCoroutine = null;
    }

    private IEnumerator AnimatePortraitRoutine()
    {
        WaitForSeconds frameDelayWait = new WaitForSeconds(1f / PortraitFramesPerSecond);

        while (true)
        {
            for (int i = 0; i < PortraitFrameCount; i++)
            {
                spawnedPortrait.sprite = activePortraitFrames[i];
                yield return frameDelayWait;
            }

            spawnedPortrait.sprite = activePortraitFrames[0];

            float delaySeconds = Random.Range(PortraitIdleDelayMinSeconds, PortraitIdleDelayMaxSeconds);
            yield return new WaitForSeconds(delaySeconds);
        }
    }

    private IEnumerator HideDialogueRoutine()
    {
        yield return ScaleDialogueElementsRoutine(textBackground.transform.localScale, HiddenScale, DialogueScaleDuration);
        ClearTextbox();

        if (spawnedPortrait != null)
        {
            Destroy(spawnedPortrait.gameObject);
            spawnedPortrait = null;
        }

        dialogueScaleCoroutine = null;
    }

    private List<Sprite> GetPortraitFrames(Sprite spriteToUse)
    {
        if (speakerPortrait2.Contains(spriteToUse))
        {
            return speakerPortrait2;
        }

        return speakerPortrait1;
    }

    private void StopDialogueAnimationCoroutines()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (portraitAnimationCoroutine != null)
        {
            StopCoroutine(portraitAnimationCoroutine);
            portraitAnimationCoroutine = null;
        }
    }

    private void StopScaleCoroutine()
    {
        if (dialogueScaleCoroutine != null)
        {
            StopCoroutine(dialogueScaleCoroutine);
            dialogueScaleCoroutine = null;
        }
    }

    private void ClearTextbox()
    {
        speakerTextbox.text = string.Empty;
        speakerTextbox.maxVisibleCharacters = 0;
    }

    private IEnumerator ScaleDialogueElementsRoutine(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 scale = Vector3.Lerp(fromScale, toScale, t);

            textBackground.transform.localScale = scale;
            textArrow.transform.localScale = scale;
            if (spawnedPortrait != null)
            {
                spawnedPortrait.transform.localScale = scale;
            }

            yield return null;
        }

        textBackground.transform.localScale = toScale;
        textArrow.transform.localScale = toScale;

        if (spawnedPortrait != null)
        {
            spawnedPortrait.transform.localScale = toScale;
        }

        dialogueScaleCoroutine = null;
    }
}
