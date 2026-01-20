using UnityEngine;

public class BoundaryWall : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // If the ball hits this invisible wall...
        BallGameMechanics ball = other.GetComponent<BallGameMechanics>();
        if (ball != null)
        {
            ball.ResetBall();
        }
    }
}