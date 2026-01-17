using UnityEngine;

public class AnchorCreator : MonoBehaviour
{
    public GameObject anchorPrefab; // Assign your "TableAnchor" prefab here
    public LayerMask layerMask = ~0; // Default to hit everything

    void Update()
    {
        // Check for Right Index Trigger press (works for Controller or Hand Pinch)
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            // Create a ray starting from this object (the hand/controller) pointing forward
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Perform the physics Raycast
            if (Physics.Raycast(ray, out hit, 10.0f, layerMask))
            {
                // Instantiate the marker at the hit point
                GameObject newAnchor = Instantiate(anchorPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                
                // CRITICAL: Parent it to the hit object (the real world plane)
                // This ensures that if the headset corrects its tracking, the anchor moves with the table.
                newAnchor.transform.parent = hit.transform;
            }
        }
    }
}