using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    // All objects and components used by the controller
    [SerializeField] GameObject ball;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] TMP_Text player1ScoreText;
    [SerializeField] TMP_Text player2ScoreText;
    [SerializeField] TMP_Text textIntro;
    [SerializeField] Animation animText;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip clip;
    [SerializeField] AudioClip complet;

    // Player 1 score
    public int player1Score;

    // Player 2 score
    public int player2Score;

    // Whether the game is currently in play
    bool isPlaying = false;

    // Number of players
    public int players;

    void Start()
    {
        // Load the "players" value saved in PlayerPrefs to know whether it is one or two players
        players = PlayerPrefs.GetInt("Players");

        // If it is a single-player game, disable the second-player script and enable the AI-controlled second player
        if (players == 1)
        {
            if (player2 == null)
            {
                Debug.LogWarning("[GameController] Player2 reference is missing.");
                return;
            }

            PlayerMovement playerMovement = player2.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            Player2COM player2Com = player2.GetComponent<Player2COM>();
            if (player2Com != null)
            {
                player2Com.enabled = true;
            }
            else
            {
                Debug.LogWarning("[GameController] Player2COM component is missing on Player2.");
            }
        }
    }

    // If the space key is pressed and the game has not started yet, start it
    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame && !isPlaying)
        {
            StartCoroutine("clearText");
            isPlaying = true;
        }

        // Pressing Escape returns to the start menu
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene("Menu");
        }
    }

    // Checks the players' scores and ends the game when one reaches 10 points and displays the result
    void CheckScore()
    {
        if (player1Score == 10)
        {
            player1ScoreText.color = Color.yellow;
            player1ScoreText.text = "\n Player 1 Wins!!";
            player2ScoreText.text = "\n Player 2 Loses!";
            StartCoroutine("resetGame");
        }

        if (player2Score == 10)
        {
            player2ScoreText.color = Color.yellow;
            player1ScoreText.text = "\n Player 1 Loses!";
            player2ScoreText.text = "\n Player 2 Wins!!";
            StartCoroutine("resetGame");
        }
    }

    // Resets the game by returning values to their original state and returns to the menu
    IEnumerator resetGame()
    {
        source.PlayOneShot(complet);
        yield return new WaitForSeconds(1f);

        BallController ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetDirection(0f);
        }

        ball.transform.position = new Vector2(0, 0);
        ball.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Menu");
    }

    // Adds 1 point to player 1 and shows it on screen. Resets the ball to serve from the player gate.
    public void Player1Scored()
    {
        player1Score++;
        player1ScoreText.text = player1Score.ToString();
        Invoke("ResetBall1", 1f);
        CheckScore();
    }

    // Adds 1 point to player 2 and shows it on screen. Resets the ball to serve from the player gate.
    public void Player2Scored()
    {
        player2Score++;
        player2ScoreText.text = player2Score.ToString();
        Invoke("ResetBall2", 1f);
        CheckScore();
    }

    // Serves from player 1's side
    public void ResetBall1()
    {
        ball.transform.position = new Vector3(5, 0, 0);
        ball.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);

        BallController ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetDirection(180f);
        }
    }

    // Serves from player 2's side
    public void ResetBall2()
    {
        ball.transform.position = new Vector3(-5, 0, 0);
        ball.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);

        BallController ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetDirection(0f);
        }
    }

    // Clears the text and starts the match
    IEnumerator clearText()
    {
        animText.Play();

        BallController ballController = ball.GetComponent<BallController>();
        if (ballController != null)
        {
            ballController.SetDirection(45f);
        }

        source.PlayOneShot(clip);
        yield return new WaitForSeconds(.5f);
        textIntro.text = " ";
    }
}
