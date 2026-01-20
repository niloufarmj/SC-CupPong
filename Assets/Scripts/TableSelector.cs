using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Handles the selection of the game table using Hand Tracking.
/// 1. Casts a ray from the hand's pointer pose.
/// 2. Highlights valid tables/desks when hovered.
/// 3. Detects a "Pinch" gesture to confirm selection and start the game.
/// </summary>
public class TableSelector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the main game manager script.")]
    public BeerPongSetup gameManager; 
    [Tooltip("The OVRHand component (usually Right Hand) to use for pointing.")]
    public OVRHand handTracker;      
    [Tooltip("Material to apply when hovering over a valid table.")]
    public Material hoverMaterial;   

    [Header("Settings")]
    public float rayDistance = 3.0f;
    public LayerMask layerMask = ~0; // Default to 'Everything'

    // State tracking
    private MRUKAnchor currentHoveredTable;
    private Material originalMaterial;
    private MeshRenderer currentRenderer;
    private bool wasPinching = false; // To detect pinch 'down' event

    void Update()
    {
        // Ensure hand tracker is assigned
        if (handTracker == null) return;

        // 1. Determine Ray Origin
        // Use PointerPose if available (stable wrist/pointer direction), otherwise fallback to transform.
        Transform rayOrigin = handTracker.PointerPose != null ? handTracker.PointerPose : transform;
        
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        // 2. Cast Ray
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            // Try to get the MRUK Anchor component from the object we hit
            MRUKAnchor anchor = hit.collider.GetComponentInParent<MRUKAnchor>();

            // 3. Validate Hit
            // We only care about objects labeled TABLE or DESK
            if (anchor != null && (anchor.Label.ToString().Contains("TABLE") || anchor.Label.ToString().Contains("DESK")))
            {
                HandleHover(anchor);
                CheckForPinch(anchor); // Check if user wants to select this table
            }
            else
            {
                ClearHover(); // Looked away from table
            }
        }
        else
        {
            ClearHover(); // Looked at nothing
        }
    }

    /// <summary>
    /// Checks for the Index Finger Pinch gesture to trigger selection.
    /// </summary>
    void CheckForPinch(MRUKAnchor anchor)
    {
        // Get current pinch state from OVRHand
        bool isPinching = handTracker.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // Detect "Pinch Down" (Edge detection: was not pinching -> is now pinching)
        if (isPinching && !wasPinching)
        {
            SelectTable(anchor);
        }

        // Update state for next frame
        wasPinching = isPinching;
    }

    /// <summary>
    /// Logic for highlighting the table when looked at.
    /// </summary>
    void HandleHover(MRUKAnchor anchor)
    {
        // If already hovering this table, do nothing
        if (currentHoveredTable == anchor) return;
        
        // Clean up previous hover if any
        ClearHover();
        
        // Apply new hover
        currentHoveredTable = anchor;
        
        if (hoverMaterial != null)
        {
            currentRenderer = anchor.GetComponentInChildren<MeshRenderer>();
            if (currentRenderer != null)
            {
                // Cache original material to restore later
                originalMaterial = currentRenderer.material;
                // Apply hover effect
                currentRenderer.material = hoverMaterial;
            }
        }
    }

    /// <summary>
    /// Restores the original material of the table when the user looks away.
    /// </summary>
    void ClearHover()
    {
        if (currentHoveredTable != null)
        {
            if (currentRenderer != null && originalMaterial != null)
                currentRenderer.material = originalMaterial;
        }
        
        // Reset state
        currentHoveredTable = null;
        currentRenderer = null;
        originalMaterial = null;
    }

    /// <summary>
    /// Confirms the selection and hands control over to the GameManager.
    /// </summary>
    void SelectTable(MRUKAnchor anchor)
    {
        Debug.Log("Table Selected via Pinch!");
        ClearHover(); // Clean up visuals before starting
        
        // Start the game logic on this specific anchor
        gameManager.InitializeGameOnTable(anchor);
        
        // Disable this selector script so we don't keep selecting tables during gameplay
        this.enabled = false; 
    }
}