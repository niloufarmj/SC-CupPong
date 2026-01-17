using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TableSelector : MonoBehaviour
{
    [Header("References")]
    public BeerPongSetup gameManager; 
    public OVRHand handTracker;      // Drag your "OVRHandPrefab" (Right) here
    public Material hoverMaterial;   

    [Header("Settings")]
    public float rayDistance = 3.0f;
    public LayerMask layerMask = ~0;

    private MRUKAnchor currentHoveredTable;
    private Material originalMaterial;
    private MeshRenderer currentRenderer;
    
    // Track pinch state to prevent "double clicking"
    private bool wasPinching = false;

    void Update()
    {
        // Safety check
        if (handTracker == null) return;

        // 1. Raycast from the Hand's "PointerPose" (Standard for hand rays)
        // If PointerPose is null, fallback to the object's transform
        Transform rayOrigin = handTracker.PointerPose != null ? handTracker.PointerPose : transform;
        
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            MRUKAnchor anchor = hit.collider.GetComponentInParent<MRUKAnchor>();

            if (anchor != null && (anchor.Label.ToString().Contains("TABLE") || anchor.Label.ToString().Contains("DESK")))
            {
                HandleHover(anchor);
                CheckForPinch(anchor); // Check for Hand Pinch
            }
            else
            {
                ClearHover();
            }
        }
        else
        {
            ClearHover();
        }
    }

    void CheckForPinch(MRUKAnchor anchor)
    {
        // Check if Index finger is pinching
        bool isPinching = handTracker.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // Detect the moment the pinch starts (GetDown)
        if (isPinching && !wasPinching)
        {
            SelectTable(anchor);
        }

        // Update state for next frame
        wasPinching = isPinching;
    }

    void HandleHover(MRUKAnchor anchor)
    {
        if (currentHoveredTable == anchor) return;
        ClearHover();
        currentHoveredTable = anchor;
        
        if (hoverMaterial != null)
        {
            currentRenderer = anchor.GetComponentInChildren<MeshRenderer>();
            if (currentRenderer != null)
            {
                originalMaterial = currentRenderer.material;
                currentRenderer.material = hoverMaterial;
            }
        }
    }

    void ClearHover()
    {
        if (currentHoveredTable != null)
        {
            if (currentRenderer != null && originalMaterial != null)
                currentRenderer.material = originalMaterial;
        }
        currentHoveredTable = null;
        currentRenderer = null;
        originalMaterial = null;
    }

    void SelectTable(MRUKAnchor anchor)
    {
        Debug.Log("Table Selected via Pinch!");
        ClearHover();
        gameManager.InitializeGameOnTable(anchor);
        this.enabled = false; 
    }
}