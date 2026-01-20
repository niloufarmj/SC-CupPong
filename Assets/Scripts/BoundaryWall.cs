using UnityEngine;

/// <summary>
/// Attached to the invisible boundary walls generated around the table.
/// Acts as a safety net to detect when the ball flies out of bounds or misses the table completely.
/// </summary>
public class BoundaryWall : MonoBehaviour
{
    /// <summary>
    /// Triggered when any physical object passes through this invisible wall.
    /// </summary>
    /// <param name="other">The collider that entered the trigger zone.</param>
    void OnTriggerEnter(Collider other)
    {
        // Attempt to find the Ball mechanics script on the object that hit the wall
        BallGameMechanics ball = other.GetComponent<BallGameMechanics>();

        // If it is indeed the ball...
        if (ball != null)
        {
            // Reset the ball to the hand position.
            // Note: ResetBall() defaults to isGoal=false, so this counts as a "Miss".
            ball.ResetBall();
        }
    }
}