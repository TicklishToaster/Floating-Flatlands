using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float speed = 6f;
    private float minY = -3.4f;
    private float maxY = 3.4f;

    void Update()
    {
        bool shouldFreezeThisPaddle = BallController.Instance != null &&
            ((transform.position.x < 0f && BallController.Instance.IsHeldOnPlayerOneSide) ||
             (transform.position.x > 0f && BallController.Instance.IsHeldOnPlayerTwoSide));

        if (shouldFreezeThisPaddle)
        {
            return;
        }

        float pMovement = ReadVertical();
        Vector2 playerPosition = transform.position;
        playerPosition.y = Mathf.Clamp(playerPosition.y + pMovement * speed * Time.deltaTime, minY, maxY);
        transform.position = playerPosition;
    }

    public static float ReadVertical()
    {
        return Mathf.Clamp(ReadKeyboardAxis(Key.W, Key.S) + ReadKeyboardAxis(Key.UpArrow, Key.DownArrow), -1f, 1f);
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
}
