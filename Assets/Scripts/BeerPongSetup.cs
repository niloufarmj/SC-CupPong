using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// The central Game Manager for the Mixed Reality Beer Pong experience.
/// Responsibilities:
/// 1. Calculating table geometry (finding the long axis).
/// 2. Clearing debug visuals (hiding pink walls/floors) to reveal Passthrough.
/// 3. Spawning game elements (Cups, Ball, Scoreboard) at correct positions.
/// 4. Generating invisible boundary walls to handle out-of-bounds logic.
/// </summary>
public class BeerPongSetup : MonoBehaviour
{
    [Header("Game Assets")]
    [Tooltip("The manager prefab that holds the logic for cup formations.")]
    public GameObject cupRackPrefab;
    [Tooltip("The interactive ball prefab.")]
    public GameObject ballPrefab;
    [Tooltip("The invisible container box to stop the ball from rolling away on start.")]
    public GameObject ballCorralPrefab;
    [Tooltip("The world-space UI canvas for score display.")]
    public GameObject scoreBoardPrefab;

    [Header("Placement Settings")]
    [Tooltip("How far from the table edge the player/cups should be positioned (in meters).")]
    public float tableEdgeOffset = 0.3f;
    [Tooltip("Additional height offset to prevent z-fighting or clipping into the table.")]
    public float surfaceHeightOffset = 0.02f;
    [Tooltip("If true, uses Physics Raycast to find exact table height. If false, uses fixed offset.")]
    public bool useRaycastPlacement = true;

    [Header("Boundary Settings")]
    public float wallHeight = 1.0f; 

    /// <summary>
    /// Main entry point. Called by TableSelector when the user confirms a table.
    /// </summary>
    /// <param name="selectedTable">The specific MRUK Anchor the user is looking at.</param>
    public void InitializeGameOnTable(MRUKAnchor selectedTable)
    {
        // 1. VISUAL CLEANUP: Focus Mode
        // Hide all other debug meshes (colored walls/floors) so only the real world + game table exist.
        HideOtherDebugSurfaces(selectedTable);

        // 2. Geometry Calculation
        // Determine the dimensions of the table plane.
        Rect planeRect = selectedTable.PlaneRect.HasValue ? selectedTable.PlaneRect.Value : new Rect(0,0,1,1);
        Vector2 tableSize = planeRect.size;
        
        // Find the "Long Axis" of the table to orient the game correctly.
        bool isWide = tableSize.x > tableSize.y;
        float longDimension = isWide ? tableSize.x : tableSize.y;
        
        // Calculate spawn positions relative to the center (0,0)
        // Cups go to one end, Ball goes to the other.
        float offsetDistance = (longDimension / 2) - tableEdgeOffset;

        Vector3 localPosCup = isWide ? new Vector3(offsetDistance, 0, 0) : new Vector3(0, offsetDistance, 0);
        Vector3 localPosBall = isWide ? new Vector3(-offsetDistance, 0, 0) : new Vector3(0, -offsetDistance, 0);

        // 3. Spawn Core Elements (Cups, Ball, UI)
        SpawnGameObjects(selectedTable, localPosCup, localPosBall);

        // 4. Generate Boundaries
        // Create invisible walls around the table to detect "Misses".
        SpawnBoundaries(selectedTable, tableSize, isWide);
    }

    /// <summary>
    /// Hides the colored debug meshes (created by ForceSceneLoad) for all room anchors except the active table.
    /// This creates a clean MR effect where virtual objects sit in the real world.
    /// </summary>
    void HideOtherDebugSurfaces(MRUKAnchor selectedTable)
    {
        if (MRUK.Instance == null) return;
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        foreach (var anchor in room.Anchors)
        {
            // Skip the active game table (we want to keep it visible or at least its collider active)
            if (anchor == selectedTable) continue;

            // Disable MeshRenderers on all other anchors (Walls, Ceiling, Floor, other Tables)
            foreach (var renderer in anchor.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }
    }

    /// <summary>
    /// Instantiates all interactive game objects at their calculated positions.
    /// </summary>
    void SpawnGameObjects(MRUKAnchor table, Vector3 cupLocalPos, Vector3 ballLocalPos)
    {
        // --- 1. Cup Rack ---
        GameObject rack = Instantiate(cupRackPrefab);
        rack.transform.SetParent(table.transform, false);
        
        // Calculate height (Raycast or Fixed)
        float cupHeight = useRaycastPlacement ? GetSurfaceHeight(table, cupLocalPos) : 0.07f;
        
        // Apply position (Z-offset added to prevent clipping)
        rack.transform.localPosition = new Vector3(cupLocalPos.x, cupLocalPos.y, cupHeight + 0.05f);
        
        // Rotate (0, 90, 90) is often needed to align upright prefabs with the Table's flat coordinate system.
        rack.transform.localRotation = Quaternion.Euler(0, 90, 90); 

        // --- 2. The Ball ---
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.SetParent(table.transform, false);
        float ballHeight = useRaycastPlacement ? GetSurfaceHeight(table, ballLocalPos) + 0.05f : 0.1f;
        ball.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight);
        
        // --- 3. Stability Corral ---
        // An invisible box to keep the ball from rolling away at start.
        if (ballCorralPrefab != null)
        {
            GameObject corral = Instantiate(ballCorralPrefab);
            corral.transform.SetParent(table.transform, false);
            // Place slightly lower so the walls intersect the table
            corral.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight - 0.02f);
            corral.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        // --- 4. Scoreboard UI ---
        if (scoreBoardPrefab != null)
        {
            GameObject board = Instantiate(scoreBoardPrefab);
            board.transform.SetParent(table.transform, false);
            
            // Calculate a position BEHIND the cups
            Vector3 direction = (cupLocalPos - ballLocalPos).normalized;
            Vector3 boardPos = cupLocalPos + (direction * 0.25f) + new Vector3(0, 0, 0.3f); // 30cm up
            
            board.transform.localPosition = boardPos;
            
            // Make the UI face the player (Ball Spawn Point)
            Vector3 lookTarget = table.transform.TransformPoint(ballLocalPos);
            board.transform.LookAt(lookTarget);
            board.transform.Rotate(0, 180, 0); // Corrects text mirroring if needed
        }
    }

    /// <summary>
    /// Procedurally generates 3 invisible walls around the table edges (Far, Left, Right).
    /// The player's side is left open.
    /// </summary>
    void SpawnBoundaries(MRUKAnchor table, Vector2 size, bool isWide)
    {
        // Parent object to keep hierarchy clean
        GameObject boundaryParent = new GameObject("InvisibleBoundaries");
        boundaryParent.transform.SetParent(table.transform, false);
        boundaryParent.transform.localPosition = Vector3.zero;
        boundaryParent.transform.localRotation = Quaternion.identity;

        // Logic: Spawn walls based on which axis is the "Long" one.
        if (isWide)
        {
            // Far Wall (+X)
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            // Left Wall (+Y)
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            // Right Wall (-Y)
            CreateOneWall(boundaryParent, new Vector3(0, -size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
        }
        else 
        {
            // Far Wall (+Y)
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            // Left Wall (+X)
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            // Right Wall (-X)
            CreateOneWall(boundaryParent, new Vector3(-size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
        }
    }

    /// <summary>
    /// Helper to create a single invisible trigger wall.
    /// </summary>
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
        bc.isTrigger = true; // Crucial for detecting the ball passing through

        // Add the logic script that resets the ball
        wall.AddComponent<BoundaryWall>();
    }

    /// <summary>
    /// Raycasts down to find the exact surface height of the table at a specific point.
    /// Useful for uneven scanned meshes.
    /// </summary>
    float GetSurfaceHeight(MRUKAnchor table, Vector3 localPos)
    {
        Vector3 worldPos = table.transform.TransformPoint(localPos);
        Vector3 rayStart = worldPos + Vector3.up * 0.5f; // Start 0.5m above
        Ray ray = new Ray(rayStart, Vector3.down);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 1.0f)) 
        {
            // Convert world hit point back to local Z height
            return table.transform.InverseTransformPoint(hit.point).z + surfaceHeightOffset;
        }
        return 0.07f; // Fallback height
    }
}