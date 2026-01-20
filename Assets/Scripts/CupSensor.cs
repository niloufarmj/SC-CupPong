using UnityEngine;

public class CupSensor : MonoBehaviour
{
    private bool hasScored = false;

    void OnTriggerEnter(Collider other)
    {
        // prevent double counting
        if (hasScored) return;

        // Check if the object is the Ball
        // We look for the "BallGameMechanics" script we created earlier
        BallGameMechanics ball = other.GetComponent<BallGameMechanics>();

        if (ball != null)
        {
            hasScored = true;
            Debug.Log("GOAL! Ball landed in cup.");

            if (GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.scoreClip, transform.position);

            // 1. UPDATE SCORE FIRST (Fixes the bug)
            // We count the hit BEFORE checking if the game is over.
            if (ScoreBoard.Instance != null)
            {
                 ScoreBoard.Instance.AddHit();
            }

            // 2. Then Tell the Rack Manager (which might trigger Game Over)
            CupRackManager rackManager = GetComponentInParent<CupRackManager>();
            
            if (rackManager != null)
            {
                rackManager.OnCupHit();
            }
            else
            {
                Debug.LogError("Could not find CupRackManager in parents!");
            }

            // 3. Reset the Ball (Pass TRUE so it doesn't count as a miss)
            // Note: If the game ends, the ball might be destroyed by RackManager, 
            // but calling this here is safe as it will just be ignored if destroyed.
            ball.ResetBall(true);
        }
    }
}