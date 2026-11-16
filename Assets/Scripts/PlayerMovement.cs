using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Movimientos raquetas
    // Crea variables
    [SerializeField] float speed = 7f;
    [SerializeField] bool isPlayerOne = true;

    // N�mero de jugadores
    public int players;

    // Carga el n�mero de jugadores por PlayerPrefs
    void Start()
    {       
        players = PlayerPrefs.GetInt("Players");       
    }

    // Carga funci�n de movimiento de raquetas seg�n est� jugando uno o dos personas
    void Update()
    {
        bool shouldFreezeThisPaddle = BallController.Instance != null &&
            ((isPlayerOne && BallController.Instance.IsHeldOnPlayerOneSide) ||
             (!isPlayerOne && BallController.Instance.IsHeldOnPlayerTwoSide));

        if (shouldFreezeThisPaddle)
        {
            return;
        }

        if (players==2)
        {
            TwoPlayers();
        }
        else
        {
            OnePlayer();
        }    
    }

    // Al ser 2 jugadores, toma los Inputs desde dos Axis distintos
    private void TwoPlayers()
    {
        float pMovement = isPlayerOne ? ReadPlayerOneVertical() : ReadPlayerTwoVertical();
           Vector2 playerPosition = transform.position;
           playerPosition.y = Mathf.Clamp(playerPosition.y + pMovement * speed * Time.deltaTime, -3.9f, 3.9f);
           transform.position = playerPosition;  
    }

    // Al ser 1 jugador, �nicamente toma el Input por Axis de un jugador
    private void OnePlayer()
    {
        float pMovement = ReadSinglePlayerVertical();
        Vector2 playerPosition = transform.position;
        playerPosition.y = Mathf.Clamp(playerPosition.y + pMovement * speed * Time.deltaTime, -3.9f, 3.9f);
        transform.position = playerPosition;  
    }

    public static float ReadSinglePlayerVertical()
    {
        float value = ReadKeyboardAxis(Key.W, Key.S) + ReadKeyboardAxis(Key.UpArrow, Key.DownArrow);
        return Mathf.Clamp(value + ReadGamepadVertical(), -1f, 1f);
    }

    public static float ReadPlayerOneVertical()
    {
        return Mathf.Clamp(ReadKeyboardAxis(Key.W, Key.S) + ReadGamepadVertical(), -1f, 1f);
    }

    public static float ReadPlayerTwoVertical()
    {
        return ReadKeyboardAxis(Key.UpArrow, Key.DownArrow);
    }

    private static float ReadKeyboardAxis(Key upKey, Key downKey)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        bool upPressed = keyboard[upKey].isPressed;
        bool downPressed = keyboard[downKey].isPressed;

        if (upPressed == downPressed)
        {
            return 0f;
        }

        return upPressed ? 1f : -1f;
    }

    private static float ReadGamepadVertical()
    {
        Gamepad gamepad = Gamepad.current;
        return gamepad == null ? 0f : gamepad.leftStick.ReadValue().y;
    }
}
