using UnityEngine;
using System.Collections.Generic;

public class CupRackManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject cupPrefab;
    public float cupSpacingRatio = 1.05f;

    [Header("Debug")]
    [Range(0, 6)]
    public int currentCupCount = 6; // Renamed from testCupCount for clarity

    private List<GameObject> activeCups = new List<GameObject>();
    private Dictionary<int, List<Vector3>> layouts = new Dictionary<int, List<Vector3>>();

    void Start()
    {
        DefineLayouts();
        // Start the game with 6 cups (or whatever is set in Inspector)
        SetupRack(currentCupCount);
    }

    // --- NEW FUNCTION: Called by CupSensor.cs ---
    public void OnCupHit()
    {
        // 1. Decrease Count
        currentCupCount--;

        // 2. Check Game Over
        if (currentCupCount <= 0)
        {
            Debug.Log("GAME OVER! YOU WIN!");
            ClearCups();
            // Optional: You could call a function here to restart the game later
            return;
        }

        // 3. Re-Rack (Re-construct layout)
        // This destroys old cups and spawns new ones in the new formation
        SetupRack(currentCupCount);
    }

    public void SetupRack(int count)
    {
        if (count <= 0 || !layouts.ContainsKey(count))
        {
            ClearCups();
            return;
        }

        ClearCups();
        SpawnCups(count);
        currentCupCount = count; // Ensure internal tracking matches
    }

    private void ClearCups()
    {
        foreach (GameObject cup in activeCups)
        {
            if (cup != null) Destroy(cup);
        }
        activeCups.Clear();
    }

    private void SpawnCups(int count)
    {
        List<Vector3> positions = layouts[count];

        foreach (Vector3 localPos in positions)
        {
            GameObject newCup = Instantiate(cupPrefab, transform);
            newCup.transform.localPosition = localPos;
            // Ensure standard rotation
            newCup.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            activeCups.Add(newCup);
        }
    }

    // This defines the exact patterns from your image (image_9.png)
    // Using a standard coordinate system where Z is "forward/back" and X is "left/right"
    private void DefineLayouts()
    {
        // Determine actual spacing based on the prefab's own size scale
        // Assuming the prefab is roughly 1 unit wide by default. Adjust cupSpacingRatio if needed.
        float s = cupSpacingRatio; 
        // Height of an equilateral triangle row (sqrt(3)/2 * spacing) for tight packing
        float rowHeight = s * 0.866f; 

        layouts.Clear();

        // Layout 1: Single Point
        layouts.Add(1, new List<Vector3>
        {
            new Vector3(0, 0, 0)
        });

        // Layout 2: Two in a line (front to back as per image)
        layouts.Add(2, new List<Vector3>
        {
             new Vector3(0, 0, 0),    // Front
             new Vector3(0, 0, s)     // Back
        });

        // Layout 3: Small Triangle
        layouts.Add(3, new List<Vector3>
        {
            new Vector3(0, 0, 0),           // Front tip
            new Vector3(-s/2, 0, rowHeight),// Back Left
            new Vector3(s/2, 0, rowHeight)  // Back Right
        });

        // Layout 4: Diamond
        layouts.Add(4, new List<Vector3>
        {
            new Vector3(0, 0, 0),               // Front tip
            new Vector3(-s/2, 0, rowHeight),    // Mid Left
            new Vector3(s/2, 0, rowHeight),     // Mid Right
            new Vector3(0, 0, rowHeight * 2)    // Back tip
        });

        // Layout 5: Trapezoid (2 cups in front, 3 in back)
        layouts.Add(5, new List<Vector3>
        {
            // Row 1 (Front two) - Centered
            new Vector3(-s / 2, 0, 0),
            new Vector3(s / 2, 0, 0),

            // Row 2 (Back three) - Centered behind row 1
            new Vector3(-s, 0, rowHeight),
            new Vector3(0, 0, rowHeight),
            new Vector3(s, 0, rowHeight)
        });

        // Layout 6: Standard Triangle (3-2-1 rows)
        layouts.Add(6, new List<Vector3>
        {
            // Row 1 (Front tip)
            new Vector3(0, 0, 0),
            // Row 2 (Middle two)
            new Vector3(-s/2, 0, rowHeight),
            new Vector3(s/2, 0, rowHeight),
            // Row 3 (Back three)
            new Vector3(-s, 0, rowHeight * 2),
            new Vector3(0, 0, rowHeight * 2),
            new Vector3(s, 0, rowHeight * 2)
        });
    }
}