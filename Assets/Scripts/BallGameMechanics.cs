using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the core physics and lifecycle of the ping pong ball, including:
/// - Resurrecting (Resetting) the ball when it goes out of bounds.
/// - Detecting floor collisions to count as a "Miss".
/// - Triggering audio effects for collisions.
/// </summary>
public class BallGameMechanics : MonoBehaviour
{
    // Stores the initial spawn position/rotation to return to upon reset.
    private Vector3 startPosition;
    private Quaternion startRotation;
    
    private Rigidbody rb;
    private bool isReseting = false; // Prevents multiple reset calls overlapping

    void Start()
    {
        // Cache initial transform data for resetting later
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Resets the ball to its starting position.
    /// </summary>
    /// <param name="isGoal">If true, prevents adding a "Miss" to the scoreboard (used when scoring).</param>
    public void ResetBall(bool isGoal = false)
    {
        if (isReseting) return;
        
        // Handle Score & SFX for Misses
        // If this reset wasn't caused by a Goal (e.g., hit floor/wall), it counts as a Miss.
        if (ScoreBoard.Instance != null)
        {
            if (!isGoal) 
            {
                ScoreBoard.Instance.AddMiss();
                
                // Play Miss SFX
                if (GameAudioManager.Instance != null)
                    GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.missClip, transform.position);
            }
        }

        StartCoroutine(ResetRoutine());
    }

    /// <summary>
    /// Detects collisions with the Floor (Miss) or Table (Bounce SFX).
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // 1. Floor Detection (Miss Condition)
        // Checks for objects named "FLOOR" (from ForceSceneLoad) or generally low height.
        if (collision.gameObject.name.ToUpper().Contains("FLOOR"))
        {
            ResetBall(false); // Trigger reset as a Miss
            return;
        }

        // 2. Table Bounce Audio
        // Play bounce sound if hitting a solid object with sufficient velocity.
        // Threshold (0.5f) prevents spamming sounds when rolling gently.
        if (GameAudioManager.Instance != null && collision.relativeVelocity.magnitude > 0.5f)
        {
            GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.bounceClip, transform.position, 0.8f);
        }
    }

    /// <summary>
    /// Coroutine to smoothly reset the ball's physics state and position.
    /// </summary>
    private IEnumerator ResetRoutine()
    {
        isReseting = true;

        // 1. Kill all momentum instantly
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. Teleport back to hand/start position
        transform.position = startPosition;
        transform.rotation = startRotation;

        // 3. Short delay to prevent accidental immediate re-throws
        yield return new WaitForSeconds(0.2f);

        // 4. Re-enable physics
        rb.isKinematic = false;
        isReseting = false;
    }
}