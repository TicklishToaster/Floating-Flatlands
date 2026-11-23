using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventController : MonoBehaviour
{

    [Header("GameReferences")]
    public GameController gameController;
    public BorderController borderController;
    public BallController ballController;
    public UIController uiController;
    public DialogueController dialogueController;

    [Header("Scene Objects")]
    public GameObject playerObject;
    public GameObject enemyObject;
    public Sprite goliathShipSprite;
    public SpriteRenderer fadeoutOverlay;
    private bool hasStartedSecondGame;
    private bool hasStartedThirdGame;

    private const float DialogueShowDuration = 0.5f;

    #region Event 1 Dialogue Strings
    private string dialogueString_Event1_01 = "Oh! There you are! I thought you weren't coming. \nTook your time choosing a starter ship, eh?";
    private string dialogueString_Event1_02 = "I see you've chosen the Artemis. It's a common choice for newcomers, agile and beginner-friendly. \nI prefer using the Goliath, personally.";
    private string dialogueString_Event1_03 = "Not to worry. Since you're just starting this gig, you'll have plenty of opportunities to try out different ships later if this one doesn't suit your style.";
    private string dialogueString_Event1_04 = "Did they give you an introduction? \nWell, I'm supposed to give you one regardless.";
    private string dialogueString_Event1_05 = "*Ahem*";
    private string dialogueString_Event1_06 = "\"Welcome to space, Rookie! You are now stepping into the galactic sports industry as a player of Plasma Ball(TM)!\"";
    private string dialogueString_Event1_07 = "\"As part of your introduction package, one of our professional instructors (that's me) will be giving you a headstart by teaching you everything you'll need to make it big!\"";
    private string dialogueString_Event1_08 = "... and then there's like 5 more paragraphs of superfluous details and motivational statements.";
    private string dialogueString_Event1_09 = "Look, I'm sure you know what you signed up for. \nPlasma Ball is the most famous sport in the galaxy!";
    private string dialogueString_Event1_10 = "Plus, there's potential to earn a lot of credits from this job, as you'll receive prize money from winning matches in the leagues. Not to mention the commissions from the betting racket.";
    private string dialogueString_Event1_11 = "It's arguably the galaxy's most egregious source of gambling. \nNot that I'm speaking from personal experience or anything...";
    private string dialogueString_Event1_12 = "Anyway. This remote drone will be your first opponent. \nIt's a private match, so don't worry about your performance.";
    private string dialogueString_Event1_13 = "The projectile speed is displayed on that panel that just opened. \nThe shields in the corner of your HUD display your \"HP\". \nThe drone is handicapped for your first match.";
    private string dialogueString_Event1_14 = "Also, please keep in mind your shield's redirect function. When the plasma ball hits your shield, you'll have a split second to adjust the ricochet angle. Use that to your advantage!";
    private string dialogueString_Event1_15 = "To win, just keep bouncing the ball until your opponent fails to bounce them back.";
    private string dialogueString_Event1_16 = "You may begin the match now. I'll be back when you're done.";
    #endregion
    
    #region Event 2 Dialogue Strings
    private string dialogueString_Event2_01 = "Congratulations, you’re a natural born bot killer.";
    private string dialogueString_Event2_02 = "Don’t worry, practice makes perfect.";
    private string dialogueString_Event2_03 = "That’s enough introduction, now for an actual game!\nHold on a sec, I’ll be right with you.";
    private string dialogueString_Event2_04 = "Behold, the Goliath! \nYea yea I know, it's slow.";
    private string dialogueString_Event2_05 = "But just look at the size of this shield! It's no wonder it was used for planetary defense.";
    private string dialogueString_Event2_06 = "This is a ship design that was mass produced during the Hollow War, repurposed into a games vessel.";
    private string dialogueString_Event2_07 = "Please ignore the giant cannon on top. \nIt can no longer shoot, and it's expensive to remove.";
    private string dialogueString_Event2_08 = "Once you take the spotlight in live matches, you'll earn enough credits to try out some of the fancier ships. \nLike the Basilisk, the Magus, or even the Hierophant.";
    private string dialogueString_Event2_09 = "But for now, it’s you and me! Speed vs bulk.";
    private string dialogueString_Event2_10 = "I’m going to increase the bounce power of the ball to mimic a real match.";
    private string dialogueString_Event2_11 = "You ready? Starting in 3… 2… 1…";
    #endregion

    #region Event 3 Dialogue Strings
    private string dialogueString_Event3_01 = "Wowie, that was quite a match!";
    private string dialogueString_Event3_02 = "With that, your introduction is complete.";
    private string dialogueString_Event3_03 = "The company will inform you of the date of your first booked match soon. If you win that, you'll have a chance at the qualifiers for the league.";
    private string dialogueString_Event3_04 = "... \n... \n...";
    private string dialogueString_Event3_05 = "Look... \nWhat I'm about to say is off the record.";
    private string dialogueString_Event3_06 = "Just a piece of advice from a washed up has-been.";
    private string dialogueString_Event3_07 = "Remember that you are nameless newcomer, a dark horse. \nMany will be hesitant to give you a chance.";
    private string dialogueString_Event3_08 = "The only way to get their attention is with a streak of victories.";
    private string dialogueString_Event3_09 = "To become a rising star, to make a name for yourself, you must EARN a name from the cheering crowds.";
    private string dialogueString_Event3_10 = "Otherwise, you'll just end up like me... \nWarning future newcomers of your own failures.";
    private string dialogueString_Event3_11 = "Good luck out there, rookie.";
    #endregion

    private void Awake()
    {
        StartCoroutine(Event_01_FirstGame());
    }

    public void TriggerEvent_02_SecondGame()
    {
        if (!hasStartedSecondGame)
        {
            hasStartedSecondGame = true;
            StartCoroutine(Event_02_SecondGame());
            return;
        }

        if (hasStartedThirdGame)
        {
            return;
        }

        hasStartedThirdGame = true;
        StartCoroutine(Event_03_Aftermath());
    }

    private IEnumerator Event_01_FirstGame()
    {
        StartCoroutine(moveObject(playerObject, new Vector3(-10.5f, 0.0f, 0.0f), new Vector3(-7.5f, 0.0f, 0.0f), 2.0f));
        yield return new WaitForSeconds(2.0f);
        dialogueController.ShowDialogue();
        yield return new WaitForSeconds(DialogueShowDuration);
        dialogueController.LoadDialogue(dialogueString_Event1_01);
        yield return WaitForSpacePress();

        dialogueController.LoadDialogue(dialogueString_Event1_02);             
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_03);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_04);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_05);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_06);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_07);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_08);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_09);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_10);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_11);
        yield return WaitForSpacePress();

        dialogueController.LoadDialogue(dialogueString_Event1_12);
        uiController.SetupHP();
        StartCoroutine(moveObject(enemyObject, new Vector3(10.5f, 0.0f, 0.0f), new Vector3(7.5f, 0.0f, 0.0f), 2.0f));
        yield return new WaitForSeconds(2.0f);
        yield return WaitForSpacePress();

        borderController.OpenSpeedPanel();
        dialogueController.LoadDialogue(dialogueString_Event1_13);
        yield return WaitForSpacePress();

        dialogueController.LoadDialogue(dialogueString_Event1_14);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_15);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event1_16);
        yield return WaitForSpacePress();

        dialogueController.HideDialogue();
        yield return new WaitForSeconds(2.0f);
        gameController.SpawnBall(Random.value < 0.5f);
    }

    private IEnumerator Event_02_SecondGame()
    {
        // Move enemy to starting position for the second game
        yield return new WaitForSeconds(1.0f);
        Vector3 enemyStartPosition = enemyObject.transform.position;
        Vector3 enemyEndPosition = enemyStartPosition + new Vector3(7.5f, 0.0f, 0.0f);
        yield return StartCoroutine(moveObject(enemyObject, enemyStartPosition, enemyEndPosition, 2.0f));
        yield return new WaitForSeconds(1.0f);

        // Reset enemy position and setup for the second game
        enemyObject.transform.position = new Vector3(10.5f, 0.0f, 0.0f);
        EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>();
        enemyMovement.shipSprite.sprite = goliathShipSprite;
        Transform enemyShield = FindChildByExactName(enemyObject.transform, "EnemyShield");
        Vector3 enemyShieldScale = enemyShield.localScale;
        enemyShield.localScale = new Vector3(enemyShieldScale.x, enemyShieldScale.y * 2f, enemyShieldScale.z);
        Transform shieldAimArrow = FindChildByExactName(enemyShield, "Shield (Aim Arrow)");
        Vector3 shieldAimArrowScale = shieldAimArrow.localScale;
        shieldAimArrow.localScale = new Vector3(shieldAimArrowScale.x, shieldAimArrowScale.y * 0.5f, shieldAimArrowScale.z);
        Transform enemyShipSprite = FindChildByExactName(enemyObject.transform, "Enemy Ship");
        Vector3 enemyShipSpritePosition = enemyShipSprite.localPosition;
        enemyShipSprite.localPosition = new Vector3(0.7f, enemyShipSpritePosition.y, enemyShipSpritePosition.z);
        Transform shipExhaust = FindChildByExactName(enemyShipSprite, "Ship Exhaust");
        Vector3 shipExhaustPosition = shipExhaust.localPosition;
        shipExhaust.localPosition = new Vector3(-0.25f, shipExhaustPosition.y, shipExhaustPosition.z);
        enemyMovement.SetMovementValues(4f, 2.4f, -2.4f);
  
        dialogueController.ShowDialogue();
        yield return new WaitForSeconds(DialogueShowDuration);
        if (gameController.DidPlayerWinLastGame()) {            
            dialogueController.LoadDialogue(dialogueString_Event2_01);        }
        else {            
            dialogueController.LoadDialogue(dialogueString_Event2_02);        }
        yield return WaitForSpacePress();
        
        dialogueController.LoadDialogue(dialogueString_Event2_03);
        yield return WaitForSpacePress();

        StartCoroutine(moveObject(enemyObject, new Vector3(10.5f, 0.0f, 0.0f), new Vector3(7.5f, 0.0f, 0.0f), 1.0f));
        dialogueController.LoadDialogue(dialogueString_Event2_04);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_05);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_06);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_07);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_08);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_09);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event2_10);
        yield return WaitForSpacePress();

        gameController.player1HP = 3;
        gameController.player2HP = 3;
        ballController.bouncePower = 2.0f;
        dialogueController.LoadDialogue(dialogueString_Event2_11);
        yield return WaitForSpacePress();

        dialogueController.HideDialogue();
        yield return new WaitForSeconds(1.0f);
        gameController.SpawnBall(Random.value < 0.5f);
    }

    private IEnumerator Event_03_Aftermath()
    {
        yield return new WaitForSeconds(1.0f);
        dialogueController.ShowDialogue();
        yield return new WaitForSeconds(DialogueShowDuration);
                
        dialogueController.LoadDialogue(dialogueString_Event3_01);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_02);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_03);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_04);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_05);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_06);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_07);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_08);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_09);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_10);
        yield return WaitForSpacePress();
        dialogueController.LoadDialogue(dialogueString_Event3_11);
        yield return WaitForSpacePress();

        dialogueController.HideDialogue();
        yield return new WaitForSeconds(1.0f);

        fadeoutOverlay.enabled = true;
        yield return StartCoroutine(FadeOverlayToFullAlpha(4.0f));
        yield return new WaitForSeconds(2.0f);

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    private IEnumerator WaitForSpacePress()
    {
        Keyboard keyboard = Keyboard.current;
        while (keyboard == null)
        {
            keyboard = Keyboard.current;
            yield return null;
        }

        while (keyboard.spaceKey.isPressed)
        {
            yield return null;
        }

        while (true)
        {
            while (!keyboard.spaceKey.isPressed)
            {
                yield return null;
            }

            if (dialogueController != null && dialogueController.IsTypewriterActive)
            {
                dialogueController.ForceInstantText();

                while (keyboard.spaceKey.isPressed)
                {
                    yield return null;
                }

                continue;
            }

            while (keyboard.spaceKey.isPressed)
            {
                yield return null;
            }

            yield break;
        }
    }

    public IEnumerator moveObject(GameObject targetObject, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        targetObject.transform.position = startPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            targetObject.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        targetObject.transform.position = endPosition;
    }

    private Transform FindChildByExactName(Transform root, string childName)
    {
        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private IEnumerator FadeOverlayToFullAlpha(float duration)
    {
        Color32 overlayColor = fadeoutOverlay.color;
        overlayColor.a = 0;
        fadeoutOverlay.color = overlayColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            overlayColor.a = (byte)Mathf.RoundToInt(Mathf.Lerp(0f, 255f, t));
            fadeoutOverlay.color = overlayColor;

            yield return null;
        }

        overlayColor.a = 255;
        fadeoutOverlay.color = overlayColor;
    }
}
