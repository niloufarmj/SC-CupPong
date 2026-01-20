using UnityEngine;

/// <summary>
/// Attached to the internal trigger collider ("Water Sensor") of the cup.
/// Detects when the ball enters the cup and handles the scoring sequence.
/// </summary>
public class CupSensor : MonoBehaviour
{
    // Prevents the sensor from triggering multiple times for the same ball (e.g., if it bounces inside)
    private bool hasScored = false;

    /// <summary>
    /// Triggered when the ball passes through the "liquid" surface.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 1. Validation Checks
        if (hasScored) return;

        // Verify collision is with the Ball (by looking for its main script)
        BallGameMechanics ball = other.GetComponent<BallGameMechanics>();

        if (ball != null)
        {
            hasScored = true;
            Debug.Log("GOAL! Ball landed in cup.");

            // 2. Audio Feedback (Splash Sound)
            if (GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.scoreClip, transform.position);

            // 3. Update Scoreboard (CRITICAL ORDER)
            // We must add the hit BEFORE calling the Rack Manager.
            // If the Rack Manager detects a win (0 cups left), it changes the UI to "You Win".
            // If we updated the score AFTER that, we would overwrite "You Win" with "Hits: 7".
            if (ScoreBoard.Instance != null)
            {
                 ScoreBoard.Instance.AddHit();
            }

            // 4. Notify Game Logic (Rack Manager)
            // This handles decreasing cup count, re-racking, or triggering Game Over.
            CupRackManager rackManager = GetComponentInParent<CupRackManager>();
            
            if (rackManager != null)
            {
                rackManager.OnCupHit();
            }
            else
            {
                Debug.LogError("Could not find CupRackManager in parents!");
            }

            // 5. Reset the Ball
            // We pass 'true' to indicate this was a GOAL, preventing the ball script from counting a 'Miss'.
            // Note: If the game ends here, the ball might be destroyed by the RackManager immediately after.
            ball.ResetBall(true);
        }
    }
}