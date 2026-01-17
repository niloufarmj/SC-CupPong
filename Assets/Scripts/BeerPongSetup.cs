using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BeerPongSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cupRackPrefab;
    public GameObject ballPrefab;

    [Header("Settings")]
    public float tableEdgeOffset = 0.3f;

    // We remove Start() and OnRoomLoaded() automatic logic.
    // Instead, we wait for this function to be called:

    public void InitializeGameOnTable(MRUKAnchor selectedTable)
    {
        Debug.Log($"Initializing game on table: {selectedTable.name}");

        // 1. Calculate Geometry (Reuse logic from before)
        Rect planeRect = selectedTable.PlaneRect.HasValue ? selectedTable.PlaneRect.Value : new Rect(0,0,1,1);
        Vector2 tableSize = planeRect.size;
        
        bool isWide = tableSize.x > tableSize.y;
        float longDimension = isWide ? tableSize.x : tableSize.y;
        float offsetDistance = (longDimension / 2) - tableEdgeOffset;

        Vector3 localPosCup = isWide ? new Vector3(offsetDistance, 0, 0) : new Vector3(0, offsetDistance, 0);
        Vector3 localPosBall = isWide ? new Vector3(-offsetDistance, 0, 0) : new Vector3(0, -offsetDistance, 0);

        // 2. Spawn Items
        SpawnGameObjects(selectedTable, localPosCup, localPosBall);
        
        // 3. Disable the selector so we don't accidentally spawn game 2
        // (Optional) gameObject.SetActive(false); 
    }

    void SpawnGameObjects(MRUKAnchor table, Vector3 cupLocalPos, Vector3 ballLocalPos)
    {
        // --- Spawn Cup Rack ---
        GameObject rack = Instantiate(cupRackPrefab);
        rack.transform.SetParent(table.transform, false);
        
        // FIX: Change Z from 0 to 0.07f (lifts the rack up by ~7cm)
        // Adjust "0.07f" until the cups sit perfectly on the surface.
        rack.transform.localPosition = new Vector3(cupLocalPos.x, cupLocalPos.y, 0.07f);

        // Keep your working rotation
        rack.transform.localRotation = Quaternion.Euler(0, 90, 90);

        // --- Spawn Ball ---
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.SetParent(table.transform, false);
        ball.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, 0.1f);
    }
}