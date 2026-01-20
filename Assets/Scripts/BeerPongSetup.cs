using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BeerPongSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cupRackPrefab;
    public GameObject ballPrefab;
    public GameObject ballCorralPrefab; // *** NEW SLOT for the invisible walls ***

    [Header("Settings")]
    public float tableEdgeOffset = 0.3f;
    public float surfaceHeightOffset = 0.02f;
    public bool useRaycastPlacement = true;

    public void InitializeGameOnTable(MRUKAnchor selectedTable)
    {
        // 1. Calculate Positions (Same as before)
        Rect planeRect = selectedTable.PlaneRect.HasValue ? selectedTable.PlaneRect.Value : new Rect(0,0,1,1);
        Vector2 tableSize = planeRect.size;
        
        bool isWide = tableSize.x > tableSize.y;
        float longDimension = isWide ? tableSize.x : tableSize.y;
        float offsetDistance = (longDimension / 2) - tableEdgeOffset;

        Vector3 localPosCup = isWide ? new Vector3(offsetDistance, 0, 0) : new Vector3(0, offsetDistance, 0);
        Vector3 localPosBall = isWide ? new Vector3(-offsetDistance, 0, 0) : new Vector3(0, -offsetDistance, 0);

        // 2. Spawn Items
        SpawnGameObjects(selectedTable, localPosCup, localPosBall);
    }

    void SpawnGameObjects(MRUKAnchor table, Vector3 cupLocalPos, Vector3 ballLocalPos)
    {
        // --- Spawn Rack ---
        GameObject rack = Instantiate(cupRackPrefab);
        rack.transform.SetParent(table.transform, false);
        float cupHeight = useRaycastPlacement ? GetSurfaceHeight(table, cupLocalPos) : 0.07f;
        rack.transform.localPosition = new Vector3(cupLocalPos.x, cupLocalPos.y, cupHeight);
        rack.transform.localRotation = Quaternion.Euler(0, 90, 90);

        // --- Spawn Ball ---
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.SetParent(table.transform, false);
        float ballHeight = useRaycastPlacement ? GetSurfaceHeight(table, ballLocalPos) + 0.05f : 0.1f;
        ball.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight);

        // --- SPAWN INVISIBLE CORRAL ---
        if (ballCorralPrefab != null)
        {
            GameObject corral = Instantiate(ballCorralPrefab);
            corral.transform.SetParent(table.transform, false);
            // Place at same position as ball, but slightly lower so walls sit ON table
            corral.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight - 0.02f);
            corral.transform.localRotation = Quaternion.Euler(90, 0, 0);
            // Optional: Destroy the walls after 15 seconds so they don't block the game forever
            // Destroy(corral, 15f); 
        }
    }

    float GetSurfaceHeight(MRUKAnchor table, Vector3 localPos)
    {
        Vector3 worldPos = table.transform.TransformPoint(localPos);
        Vector3 rayStart = worldPos + Vector3.up * 0.5f;
        Ray ray = new Ray(rayStart, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.0f)) return table.transform.InverseTransformPoint(hit.point).z + surfaceHeightOffset;
        return 0.07f;
    }
}