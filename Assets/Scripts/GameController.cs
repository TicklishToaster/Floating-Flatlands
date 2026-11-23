using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject ballPrefab;
    private GameObject ball;
    private bool playerWonLastGame;

    [Header("Health")]
    public int player1HP;
    public int player2HP;

    [Header("Round References")]
    public BorderController borderController;
    public EventController eventController;
    public BoxCollider2D ScoreZone1;
    public BoxCollider2D ScoreZone2;

    [Header("Shoot Audio")]
    public AudioClip ShootSound1;
    public AudioClip ShootSound2;
    public AudioClip ShootSound3;
    [Range(0f, 1f)] public float shootVolume = 1f;

    [Header("Score Audio")]
    public AudioClip ScoreSound;
    [Range(0f, 1f)] public float ScoreVolume = 1f;

    // Spawns the ball at the origin, shows the launch panel for 1 second, then launches the ball.
    // shootAtPlayer1: true aims at player 1 (180 degrees), false aims at player 2 (0 degrees).
    public void SpawnBall(bool shootAtPlayer1)
    {
        StartCoroutine(SpawnBallRoutine(shootAtPlayer1));
    }

    private IEnumerator SpawnBallRoutine(bool shootAtPlayer1)
    {
        float launchAngle = (shootAtPlayer1 ? 180f : 0f) + Random.Range(-60f, 60f);

        ball = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
        BallController ballController = ball.GetComponent<BallController>();
        ballController.PrepareForLaunch();

        borderController.OpenLaunchPanel();
        yield return ScaleChildrenIn(ball.transform, 1f);

        PlayRandomSound(ballController.ballAudioSource, shootVolume, ShootSound1, ShootSound2, ShootSound3);
        ballController.Launch(launchAngle);
        borderController.CloseLaunchPanel();
    }

    // Called when the ball enters a score zone: removes the ball, applies the HP loss, then serves a new ball.
    // The new ball is shot at the player who did not lose HP.
    public void EndRound(bool scoreZone1Triggered)
    {
        if (scoreZone1Triggered)
        {
            player1HP--;
        }
        else
        {
            player2HP--;
        }

        AudioSource.PlayClipAtPoint(ScoreSound, Vector3.zero, ScoreVolume);

        bool gameEnded = player1HP <= 0 || player2HP <= 0;
        if (gameEnded)
        {
            playerWonLastGame = player2HP <= 0;
            eventController.TriggerEvent_02_SecondGame();
        }

        StartCoroutine(EndRoundRoutine(scoreZone1Triggered, gameEnded));
    }

    public bool DidPlayerWinLastGame()
    {
        return playerWonLastGame;
    }

    private IEnumerator EndRoundRoutine(bool scoreZone1Triggered, bool gameEnded)
    {
        yield return ScaleChildrenOut(ball.transform, 0.5f);

        Destroy(ball);

        if (gameEnded)
        {
            yield break;
        }

        SpawnBall(!scoreZone1Triggered);
    }

    // Sets the X/Y scale of every child to 0, then grows them back to their original scale over the duration.
    private IEnumerator ScaleChildrenIn(Transform parent, float duration)
    {
        int count = parent.childCount;
        Transform[] children = new Transform[count];
        Vector3[] targetScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Transform child = parent.GetChild(i);
            children[i] = child;
            targetScales[i] = child.localScale;
            child.localScale = new Vector3(0f, 0f, targetScales[i].z);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < count; i++)
            {
                if (children[i] == null)
                {
                    continue;
                }

                Vector3 target = targetScales[i];
                children[i].localScale = new Vector3(target.x * t, target.y * t, target.z);
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            if (children[i] != null)
            {
                children[i].localScale = targetScales[i];
            }
        }
    }

    // Shrinks the X/Y scale of every child to 0 over the duration.
    private IEnumerator ScaleChildrenOut(Transform parent, float duration)
    {
        int count = parent.childCount;
        Transform[] children = new Transform[count];
        Vector3[] startScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Transform child = parent.GetChild(i);
            children[i] = child;
            startScales[i] = child.localScale;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < count; i++)
            {
                if (children[i] == null)
                {
                    continue;
                }

                Vector3 start = startScales[i];
                float factor = 1f - t;
                children[i].localScale = new Vector3(start.x * factor, start.y * factor, start.z);
            }

            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            if (children[i] != null)
            {
                Vector3 start = startScales[i];
                children[i].localScale = new Vector3(0f, 0f, start.z);
            }
        }
    }

    // Plays one random clip from the provided set using PlayOneShot.
    private void PlayRandomSound(AudioSource audioSource, float volume, params AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }
}
