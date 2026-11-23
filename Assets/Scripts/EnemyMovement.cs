using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] public float speedOnOwnSide = 5f;
    [SerializeField] public float speedOnFarSide = 1f;
    public SpriteRenderer shipSprite;
    private float maxY = 3.4f;
    private float minY = -3.4f;

    public void SetMovementValues(float ownSideSpeed, float upperYLimit, float lowerYLimit)
    {
        speedOnOwnSide = ownSideSpeed;
        maxY = upperYLimit;
        minY = lowerYLimit;
    }

    private void Update()
    {
        BallController ballController = BallController.Instance;
        if (ballController == null || ballController.IsHeldOnPlayerTwoSide)
        {
            return;
        }

        Transform ballTransform = ballController.transform;
        float verticalDelta = ballTransform.position.y - transform.position.y;
        if (Mathf.Approximately(verticalDelta, 0f))
        {
            return;
        }

        float moveSign = Mathf.Sign(verticalDelta);
        float speed = ballTransform.position.x > 0f ? speedOnOwnSide : speedOnFarSide;
        MovePaddle(moveSign * speed * Time.deltaTime);
    }

    private void MovePaddle(float deltaY)
    {
        Vector3 playerPosition = transform.position;
        playerPosition.y = Mathf.Clamp(playerPosition.y + deltaY, minY, maxY);
        transform.position = playerPosition;
    }
}
