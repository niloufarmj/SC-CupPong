using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

public class ForceSceneLoad : MonoBehaviour
{
    [Header("Debug Materials")]
    public Material wallMaterial;     // Assign a Blue material
    public Material floorMaterial;    // Assign a Grey material
    public Material ceilingMaterial;  // Assign a White material
    public Material tableMaterial;    // Assign a Red material (Critical for your project)
    public Material otherMaterial;    // Assign a Yellow/Green material for generic stuff

    void Start()
    {
        // Subscribe to room events
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);
            MRUK.Instance.RoomUpdatedEvent.AddListener(OnRoomUpdated);
        }
    }

    void OnRoomCreated(MRUKRoom room)
    {
        Debug.Log($"Room created: {room.name} - Adding colliders...");
        AddCollidersToRoom(room);
    }
    
    void OnRoomUpdated(MRUKRoom room)
    {
        Debug.Log($"Room updated: {room.name} - Adding colliders...");
        AddCollidersToRoom(room);
    }

    void AddCollidersToRoom(MRUKRoom room)
    {
        var anchors = room.GetComponentsInChildren<MRUKAnchor>(true);
        Debug.Log($"Processing {anchors.Length} anchors in room");
        
        foreach (var anchor in anchors)
        {
            string label = anchor.Label.ToString().ToUpper();
            
            // Check if it already has a collider
            if (anchor.GetComponent<Collider>() != null) continue;
            
            // --- 1. Add Collider Logic ---
            if (anchor.PlaneRect.HasValue)
            {
                Vector2 size = anchor.PlaneRect.Value.size;
                BoxCollider boxCollider = anchor.gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(size.x, size.y, 0.01f);
                
                // --- 2. Visual Debugging Logic ---
                // Determine which material to use based on the label
                Material matToUse = GetMaterialForLabel(label);

                if (matToUse != null)
                {
                    // Create visual cube
                    GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    visualCube.transform.SetParent(anchor.transform, false);
                    visualCube.transform.localPosition = Vector3.zero;
                    visualCube.transform.localRotation = Quaternion.identity;
                    visualCube.transform.localScale = new Vector3(size.x, size.y, 0.01f);
                    
                    // Cleanup visual cube collider
                    Destroy(visualCube.GetComponent<Collider>());
                    
                    // Apply the specific color
                    MeshRenderer visualRenderer = visualCube.GetComponent<MeshRenderer>();
                    if (visualRenderer != null)
                    {
                        visualRenderer.material = matToUse;
                    }
                }
            }
            else
            {
                // Mesh fallback
                MeshFilter meshFilter = anchor.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    MeshCollider meshCollider = anchor.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                    meshCollider.convex = false;
                }
            }
            
            anchor.gameObject.layer = 0; // Default layer
        }
    }

    // Helper to pick the right color
    Material GetMaterialForLabel(string label)
    {
        if (label.Contains("WALL")) return wallMaterial;
        if (label.Contains("FLOOR")) return floorMaterial;
        if (label.Contains("CEILING")) return ceilingMaterial;
        if (label.Contains("TABLE") || label.Contains("DESK")) return tableMaterial;
        
        return otherMaterial; // Fallback for windows, doors, etc.
    }
}