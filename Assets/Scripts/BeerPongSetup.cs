using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BeerPongSetup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cupRackPrefab;
    public GameObject ballPrefab;
    public GameObject ballCorralPrefab;
    public GameObject scoreBoardPrefab;

    [Header("Settings")]
    public float tableEdgeOffset = 0.3f;
    public float surfaceHeightOffset = 0.02f;
    public bool useRaycastPlacement = true;

    [Header("Boundary Settings")]
    public float wallHeight = 1.0f; // 1 meter tall invisible walls

    public void InitializeGameOnTable(MRUKAnchor selectedTable)
    {
        // 1. Get Table Data
        Rect planeRect = selectedTable.PlaneRect.HasValue ? selectedTable.PlaneRect.Value : new Rect(0,0,1,1);
        Vector2 tableSize = planeRect.size;
        
        // Determine orientation
        bool isWide = tableSize.x > tableSize.y;
        float longDimension = isWide ? tableSize.x : tableSize.y;
        float shortDimension = isWide ? tableSize.y : tableSize.x;
        float offsetDistance = (longDimension / 2) - tableEdgeOffset;

        // Calculate positions
        Vector3 localPosCup = isWide ? new Vector3(offsetDistance, 0, 0) : new Vector3(0, offsetDistance, 0);
        Vector3 localPosBall = isWide ? new Vector3(-offsetDistance, 0, 0) : new Vector3(0, -offsetDistance, 0);

        // 2. Spawn Game Items
        SpawnGameObjects(selectedTable, localPosCup, localPosBall);

        // 3. Spawn Invisible Boundaries (The 3 Walls)
        SpawnBoundaries(selectedTable, tableSize, isWide);
    }

    void SpawnGameObjects(MRUKAnchor table, Vector3 cupLocalPos, Vector3 ballLocalPos)
    {
        // --- Rack ---
        GameObject rack = Instantiate(cupRackPrefab);
        rack.transform.SetParent(table.transform, false);
        float cupHeight = useRaycastPlacement ? GetSurfaceHeight(table, cupLocalPos) : 0.07f;
        rack.transform.localPosition = new Vector3(cupLocalPos.x, cupLocalPos.y, cupHeight + 0.05f);
        rack.transform.localRotation = Quaternion.Euler(0, 90, 90); // Adjust manually if needed via Inspector later

        // --- Ball ---
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.SetParent(table.transform, false);
        float ballHeight = useRaycastPlacement ? GetSurfaceHeight(table, ballLocalPos) + 0.05f : 0.1f;
        ball.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight);
        
        // --- Corral (Startup Stability) ---
        if (ballCorralPrefab != null)
        {
            GameObject corral = Instantiate(ballCorralPrefab);
            corral.transform.SetParent(table.transform, false);
            corral.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight - 0.02f);
            corral.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        // 4. *** SPAWN SCOREBOARD ***
        if (scoreBoardPrefab != null)
        {
            GameObject board = Instantiate(scoreBoardPrefab);
            board.transform.SetParent(table.transform, false);
            
            // MATH: Place it 20cm BEHIND the cups and 30cm UP
            // Since cups are at 'cupLocalPos', we extend further along the long axis.
            
            // Calculate direction from Ball to Cup (Gameplay direction)
            Vector3 direction = (cupLocalPos - ballLocalPos).normalized;
            
            // Position = CupPos + (Direction * 0.25m) + Up * 0.3m
            Vector3 boardPos = cupLocalPos + (direction * 0.25f) + new Vector3(0, 0, 0.3f);
            
            board.transform.localPosition = boardPos;
            
            // ROTATION: It needs to face the player (Ball position)
            // LookAt makes Z point to target, but UI text looks down Z-negative usually.
            // Simplest VR Text trick: Look at the opposite direction of the player
            // OR simpler: Just match the Rack's rotation but make it stand up vertically
            
            // If Rack is rotated (0, 90, 90), Board should effectively face the ball.
            // Let's force it to look at the ball spawn point
            Vector3 lookTarget = table.transform.TransformPoint(ballLocalPos);
            board.transform.LookAt(lookTarget);
            // LookAt often flips UI backwards, if so, rotate 180 on Y:
            board.transform.Rotate(0, 180, 0); 
        }
    
    }

    void SpawnBoundaries(MRUKAnchor table, Vector2 size, bool isWide)
    {
        // Create a parent for cleanliness
        GameObject boundaryParent = new GameObject("InvisibleBoundaries");
        boundaryParent.transform.SetParent(table.transform, false);
        boundaryParent.transform.localPosition = Vector3.zero;
        boundaryParent.transform.localRotation = Quaternion.identity;

        // We need 3 walls. 
        // Logic: Player is always at negative axis (-offsetDistance). 
        // So we need walls at: Positive Axis (Far), Positive Cross-Axis (Side), Negative Cross-Axis (Side).

        if (isWide) // X is the long axis (Player at -X, Enemy at +X)
        {
            // 1. Far Wall (+X edge)
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            // 2. Left Wall (+Y edge)
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            // 3. Right Wall (-Y edge)
            CreateOneWall(boundaryParent, new Vector3(0, -size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
        }
        else // Y is the long axis (Player at -Y, Enemy at +Y)
        {
            // 1. Far Wall (+Y edge)
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            // 2. Left Wall (+X edge)
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            // 3. Right Wall (-X edge)
            CreateOneWall(boundaryParent, new Vector3(-size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
        }
    }

    void CreateOneWall(GameObject parent, Vector3 localPos, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(parent.transform, false);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = localScale;

        // Make Invisible
        Destroy(wall.GetComponent<MeshRenderer>());

        // Make Trigger
        BoxCollider bc = wall.GetComponent<BoxCollider>();
        bc.isTrigger = true;

        // Add Logic
        wall.AddComponent<BoundaryWall>();
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