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

            // 1. Tell the Rack Manager to update game state
            // The Sensor is a child of the Cup, which is a child of the Rack.
            // So we search up the hierarchy.
            CupRackManager rackManager = GetComponentInParent<CupRackManager>();
            
            if (rackManager != null)
            {
                rackManager.OnCupHit();
            }
            else
            {
                Debug.LogError("Could not find CupRackManager in parents!");
            }

            // 2. Reset the Ball
            ball.ResetBall();
        }
    }
}