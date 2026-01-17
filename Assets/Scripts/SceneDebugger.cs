using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class SceneDebugger : MonoBehaviour
{
    void Start()
    {
        // Wait a bit for MRUK to initialize
        Invoke("CheckSceneData", 2f);
    }

    void CheckSceneData()
    {
        Debug.Log("=== SCENE DEBUG START ===");
        
        // Check if MRUK is initialized
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance is NULL! MRUK not initialized.");
            return;
        }
        
        Debug.Log("MRUK Instance found: " + MRUK.Instance.name);
        
        // Check for rooms
        var rooms = MRUK.Instance.Rooms;
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("NO ROOMS LOADED! Check your Space Setup in headset.");
            return;
        }
        
        Debug.Log($"Found {rooms.Count} room(s)");
        
        // Check each room
        foreach (var room in rooms)
        {
            Debug.Log($"--- Room: {room.name} ---");
            
            // Check for anchor objects (walls, tables, etc)
            var anchors = room.GetComponentsInChildren<MRUKAnchor>();
            Debug.Log($"  Anchors found: {anchors.Length}");
            
            foreach (var anchor in anchors)
            {
                Debug.Log($"    - {anchor.name} | Label: {anchor.Label} | Has Collider: {anchor.GetComponent<Collider>() != null}");
                
                // Check specifically for tables
                if (anchor.Label.ToString().Contains("TABLE"))
                {
                    Debug.Log($"      *** TABLE FOUND! Position: {anchor.transform.position}");
                    
                    // Check collider
                    var collider = anchor.GetComponent<Collider>();
                    if (collider == null)
                    {
                        Debug.LogError("      !!! TABLE HAS NO COLLIDER - This is the problem!");
                    }
                    else
                    {
                        Debug.Log($"      Collider: {collider.GetType().Name} | Enabled: {collider.enabled}");
                    }
                }
            }
        }
        
        // List all objects with colliders in scene
        Debug.Log("=== ALL COLLIDERS IN SCENE ===");
        Collider[] allColliders = FindObjectsOfType<Collider>();
        Debug.Log($"Total colliders found: {allColliders.Length}");
        foreach (var col in allColliders)
        {
            Debug.Log($"  - {col.gameObject.name} ({col.GetType().Name}) | Layer: {LayerMask.LayerToName(col.gameObject.layer)}");
        }
        
        Debug.Log("=== SCENE DEBUG END ===");
    }
}