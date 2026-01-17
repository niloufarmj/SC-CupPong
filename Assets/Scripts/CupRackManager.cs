using UnityEngine;
using System.Collections.Generic;

public class CupRackManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Drag your Red Cup Prefab here")]
    public GameObject cupPrefab;

    [Tooltip("Distance between the center of two adjacent cups relative to cup scale (usually slightly larger than 1.0)")]
    public float cupSpacingRatio = 1.05f;

    [Header("Debug")]
    [Range(0, 6)]
    [Tooltip("Change this slider in Play Mode to test different racks")]
    public int testCupCount = 6;

    // Internal storage for active cups
    private List<GameObject> activeCups = new List<GameObject>();
    // Dictionary to store layout positions: Key = cup count, Value = list of positions
    private Dictionary<int, List<Vector3>> layouts = new Dictionary<int, List<Vector3>>();
    // To track current state
    private int currentLoadedCount = -1;

    void Start()
    {
        // 1. Define layouts on start based on spacing
        DefineLayouts();

        // 2. Initial setup (optional, for testing)
        SetupRack(testCupCount);
    }

    // Update loop just for easy debugging in the Editor
    void Update()
    {
        // If you change the slider in the inspector while playing, update the rack
        if (currentLoadedCount != testCupCount)
        {
            SetupRack(testCupCount);
        }
    }

    /// <summary>
    /// Main function to call to generate a specific rack formation.
    /// </summary>
    /// <param name="count">Number of cups (1, 2, 3, 4, or 6)</param>
    public void SetupRack(int count)
    {
        // Handle invalid or 0 inputs
        if (count <= 0 || !layouts.ContainsKey(count))
        {
            ClearCups();
            currentLoadedCount = 0;
            return;
        }

        ClearCups();
        SpawnCups(count);
        currentLoadedCount = count;
        // Update debug slider to match reality
        testCupCount = count;
    }

    private void ClearCups()
    {
        foreach (GameObject cup in activeCups)
        {
            Destroy(cup);
        }
        activeCups.Clear();
    }

    private void SpawnCups(int count)
    {
        // Retrieve the predefined positions list for this count
        List<Vector3> positions = layouts[count];

        foreach (Vector3 localPos in positions)
        {
            // Instantiate cup as a child of this rack object
            GameObject newCup = Instantiate(cupPrefab, transform);
            // Set its local position based on the layout definition
            newCup.transform.localPosition = localPos;
            // Ensure rotation is standard
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