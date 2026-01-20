using UnityEngine;
using System.Collections;

public class BallGameMechanics : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;
    private bool isReseting = false;

    void Start()
    {
        // Remember where we spawned so we can return here later
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    // Called by boundaries or floor to reset the ball
    // Modified ResetBall to accept a "Goal" flag
    public void ResetBall(bool isGoal = false)
    {
        if (isReseting) return;
        
        // *** LOGIC UPDATE ***
        if (ScoreBoard.Instance != null)
        {
            if (!isGoal) 
            {
                // If it wasn't a goal, it must be a miss (floor/wall hit)
                ScoreBoard.Instance.AddMiss();
            }
        }

        StartCoroutine(ResetRoutine());
    }

    // Floor/Wall Detection (MISS)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.ToUpper().Contains("FLOOR"))
        {
            // Call reset with isGoal = false
            ResetBall(false);
        }
    }

    private IEnumerator ResetRoutine()
    {
        isReseting = true;

        // 1. Stop Physics instantly
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. Teleport Home
        transform.position = startPosition;
        transform.rotation = startRotation;

        // 3. Small delay to prevent accidental immediate throws
        yield return new WaitForSeconds(0.2f);

        // 4. Unfreeze (Ready to play)
        rb.isKinematic = false;
        isReseting = false;
    }
}