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
    public float wallHeight = 1.0f; 

    public void InitializeGameOnTable(MRUKAnchor selectedTable)
    {
        // 1. VISUAL CLEANUP: Hide all other room meshes (pink walls, etc.)
        // We do this BEFORE spawning game items so we don't accidentally hide the game itself.
        HideOtherDebugSurfaces(selectedTable);

        // 2. Calculate Geometry
        Rect planeRect = selectedTable.PlaneRect.HasValue ? selectedTable.PlaneRect.Value : new Rect(0,0,1,1);
        Vector2 tableSize = planeRect.size;
        
        bool isWide = tableSize.x > tableSize.y;
        float longDimension = isWide ? tableSize.x : tableSize.y;
        float offsetDistance = (longDimension / 2) - tableEdgeOffset;

        Vector3 localPosCup = isWide ? new Vector3(offsetDistance, 0, 0) : new Vector3(0, offsetDistance, 0);
        Vector3 localPosBall = isWide ? new Vector3(-offsetDistance, 0, 0) : new Vector3(0, -offsetDistance, 0);

        if (GameAudioManager.Instance != null)
            GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.gameStartClip, selectedTable.transform.position);

        // 3. Spawn Game Items
        SpawnGameObjects(selectedTable, localPosCup, localPosBall);

        // 4. Spawn Invisible Boundaries
        SpawnBoundaries(selectedTable, tableSize, isWide);
    }

    // *** NEW FUNCTION: Visual Cleanup ***
    void HideOtherDebugSurfaces(MRUKAnchor selectedTable)
    {
        if (MRUK.Instance == null) return;
        var room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        foreach (var anchor in room.Anchors)
        {
            // Skip the active game table (keep it visible as the "Stage")
            if (anchor == selectedTable) continue;

            // Find and disable the mesh renderers (the colored debug cubes)
            // We use GetComponentsInChildren because ForceSceneLoad adds the visual as a child
            foreach (var renderer in anchor.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = false;
            }
        }
    }

    void SpawnGameObjects(MRUKAnchor table, Vector3 cupLocalPos, Vector3 ballLocalPos)
    {
        // --- Rack ---
        GameObject rack = Instantiate(cupRackPrefab);
        rack.transform.SetParent(table.transform, false);
        float cupHeight = useRaycastPlacement ? GetSurfaceHeight(table, cupLocalPos) : 0.07f;
        rack.transform.localPosition = new Vector3(cupLocalPos.x, cupLocalPos.y, cupHeight + 0.05f);
        rack.transform.localRotation = Quaternion.Euler(0, 90, 90); 

        // --- Ball ---
        GameObject ball = Instantiate(ballPrefab);
        ball.transform.SetParent(table.transform, false);
        float ballHeight = useRaycastPlacement ? GetSurfaceHeight(table, ballLocalPos) + 0.05f : 0.1f;
        ball.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight);
        
        // --- Corral ---
        if (ballCorralPrefab != null)
        {
            GameObject corral = Instantiate(ballCorralPrefab);
            corral.transform.SetParent(table.transform, false);
            corral.transform.localPosition = new Vector3(ballLocalPos.x, ballLocalPos.y, ballHeight - 0.02f);
            corral.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        // --- Scoreboard ---
        if (scoreBoardPrefab != null)
        {
            GameObject board = Instantiate(scoreBoardPrefab);
            board.transform.SetParent(table.transform, false);
            
            Vector3 direction = (cupLocalPos - ballLocalPos).normalized;
            Vector3 boardPos = cupLocalPos + (direction * 0.25f) + new Vector3(0, 0, 0.3f);
            
            board.transform.localPosition = boardPos;
            
            Vector3 lookTarget = table.transform.TransformPoint(ballLocalPos);
            board.transform.LookAt(lookTarget);
            board.transform.Rotate(0, 180, 0); 
        }
    }

    void SpawnBoundaries(MRUKAnchor table, Vector2 size, bool isWide)
    {
        GameObject boundaryParent = new GameObject("InvisibleBoundaries");
        boundaryParent.transform.SetParent(table.transform, false);
        boundaryParent.transform.localPosition = Vector3.zero;
        boundaryParent.transform.localRotation = Quaternion.identity;

        if (isWide)
        {
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            CreateOneWall(boundaryParent, new Vector3(0, -size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
        }
        else 
        {
            CreateOneWall(boundaryParent, new Vector3(0, size.y/2, wallHeight/2), new Vector3(size.x, 0.1f, wallHeight));
            CreateOneWall(boundaryParent, new Vector3(size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
            CreateOneWall(boundaryParent, new Vector3(-size.x/2, 0, wallHeight/2), new Vector3(0.1f, size.y, wallHeight));
        }
    }

    void CreateOneWall(GameObject parent, Vector3 localPos, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(parent.transform, false);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = localScale;
        Destroy(wall.GetComponent<MeshRenderer>());
        BoxCollider bc = wall.GetComponent<BoxCollider>();
        bc.isTrigger = true;
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