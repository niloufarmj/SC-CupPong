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
    public void ResetBall()
    {
        if (isReseting) return; // Prevent double resets
        StartCoroutine(ResetRoutine());
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

    // Floor Detection: If we hit anything labeled "FLOOR" or simply go too low
    void OnCollisionEnter(Collision collision)
    {
        // Check for Floor by name (from ForceSceneLoad) or just raw height
        if (collision.gameObject.name.ToUpper().Contains("FLOOR"))
        {
            ResetBall();
        }
    }
}