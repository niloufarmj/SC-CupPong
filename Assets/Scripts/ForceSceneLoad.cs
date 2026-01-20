using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Automatically processes the Room Geometry provided by Meta MR Utility Kit (MRUK).
/// Responsibilities:
/// 1. Listening for Room Loaded events.
/// 2. Iterating through room anchors (Walls, Floor, Ceiling, Tables).
/// 3. Adding Physics Colliders to these surfaces so balls can bounce off them.
/// 4. (Optional) Spawning colored debug meshes to visualize what the headset "sees".
/// </summary>
public class ForceSceneLoad : MonoBehaviour
{
    [Header("Debug Visualization")]
    [Tooltip("Material for Wall anchors (usually Blue)")]
    public Material wallMaterial;     
    [Tooltip("Material for Floor anchors (usually Grey)")]
    public Material floorMaterial;    
    [Tooltip("Material for Ceiling anchors (usually White)")]
    public Material ceilingMaterial;  
    [Tooltip("Material for Table/Desk anchors (usually Red) - Critical for gameplay")]
    public Material tableMaterial;    
    [Tooltip("Fallback material for other objects like Windows/Doors")]
    public Material otherMaterial;    

    void Start()
    {
        // Subscribe to MRUK events to know when the room geometry is ready.
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);
            MRUK.Instance.RoomUpdatedEvent.AddListener(OnRoomUpdated);
        }
    }

    /// <summary>
    /// Triggered when a new room is detected and loaded.
    /// </summary>
    void OnRoomCreated(MRUKRoom room)
    {
        Debug.Log($"Room created: {room.name} - Generating colliders...");
        AddCollidersToRoom(room);
    }
    
    /// <summary>
    /// Triggered if the room data changes (e.g., user rescans).
    /// </summary>
    void OnRoomUpdated(MRUKRoom room)
    {
        Debug.Log($"Room updated: {room.name} - Refreshing colliders...");
        AddCollidersToRoom(room);
    }

    /// <summary>
    /// The core logic loop. Iterates over all anchors in the room and adds necessary components.
    /// </summary>
    void AddCollidersToRoom(MRUKRoom room)
    {
        // Get all anchors (surfaces) associated with this room (include children).
        var anchors = room.GetComponentsInChildren<MRUKAnchor>(true);
        Debug.Log($"Processing {anchors.Length} anchors in room");
        
        foreach (var anchor in anchors)
        {
            string label = anchor.Label.ToString().ToUpper();
            
            // Optimization: If it already has a collider, skip it to prevent duplicates.
            if (anchor.GetComponent<Collider>() != null) continue;
            
            // --- Strategy A: Planar Surfaces (Walls, Tables, Floors) ---
            if (anchor.PlaneRect.HasValue)
            {
                Vector2 size = anchor.PlaneRect.Value.size;
                
                // 1. Add Physics Collider
                // We use a BoxCollider with a thin Z-depth (0.01f) to act as a solid surface.
                BoxCollider boxCollider = anchor.gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(size.x, size.y, 0.01f);
                
                // 2. Add Visual Debug Cube (Optional but recommended for testing)
                Material matToUse = GetMaterialForLabel(label);

                if (matToUse != null)
                {
                    // Create a primitive cube to represent the surface visually
                    GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visualCube.transform.SetParent(anchor.transform, false);
                    visualCube.transform.localPosition = Vector3.zero;
                    visualCube.transform.localRotation = Quaternion.identity;
                    visualCube.transform.localScale = new Vector3(size.x, size.y, 0.01f);
                    
                    // Cleanup: Remove the collider from the visual mesh (the parent anchor already has one)
                    Destroy(visualCube.GetComponent<Collider>());
                    
                    // Apply the semantic color (Red for tables, etc.)
                    MeshRenderer visualRenderer = visualCube.GetComponent<MeshRenderer>();
                    if (visualRenderer != null)
                    {
                        visualRenderer.material = matToUse;
                    }
                }
            }
            // --- Strategy B: Complex Meshes (Fallback) ---
            else
            {
                // If the anchor isn't a simple plane, try to use its mesh data if available.
                MeshFilter meshFilter = anchor.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCollider = anchor.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                    meshCollider.convex = false; // Non-convex allows complex shapes (concave)
                }
            }
            
            // Ensure the object is on the Default layer so Raycasts can hit it.
            anchor.gameObject.layer = 0; 
        }
    }

    /// <summary>
    /// Helper to select the correct debug material based on the Semantic Label.
    /// </summary>
    Material GetMaterialForLabel(string label)
    {
        if (label.Contains("WALL")) return wallMaterial;
        if (label.Contains("FLOOR")) return floorMaterial;
        if (label.Contains("CEILING")) return ceilingMaterial;
        if (label.Contains("TABLE") || label.Contains("DESK")) return tableMaterial;
        
        return otherMaterial; // Default for windows, doors, etc.
    }
}