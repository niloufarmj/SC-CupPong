using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the logic for the Beer Pong cup rack.
/// Responsibilities:
/// 1. Defining and spawning cup formations (Triangle, Diamond, etc.) based on the number of remaining cups.
/// 2. Handling the "Cup Hit" event: updating the count, checking for a win, and triggering the Re-Rack.
/// 3. Communicating with the Scoreboard and Audio Manager when the game ends.
/// </summary>
public class CupRackManager : MonoBehaviour
{
    [Header("Assets")]
    [Tooltip("The prefab for the single Red Cup to spawn.")]
    public GameObject cupPrefab;

    [Header("Configuration")]
    [Tooltip("Multiplier for distance between cups. 1.0 = Touching rims. Higher values = More space.")]
    public float cupSpacingRatio = 1.05f;

    [Header("Game State")]
    [Range(0, 6)]
    [Tooltip("Current number of cups remaining on the table.")]
    public int currentCupCount = 6; 

    // Internal list to keep track of instantiated cup objects so we can destroy them during a re-rack.
    private List<GameObject> activeCups = new List<GameObject>();
    
    // Dictionary storing the vector positions for every possible cup count (1 to 6).
    private Dictionary<int, List<Vector3>> layouts = new Dictionary<int, List<Vector3>>();

    void Start()
    {
        // 1. Calculate all possible formation geometry
        DefineLayouts();
        
        // 2. Spawn the initial rack (usually 6 cups)
        SetupRack(currentCupCount);
    }

    /// <summary>
    /// Called by CupSensor.cs when a ball lands in a cup.
    /// Handles the entire sequence of scoring, re-racking, and winning.
    /// </summary>
    public void OnCupHit()
    {
        // 1. Update Game State
        currentCupCount--;

        // 2. Check Victory Condition
        if (currentCupCount <= 0)
        {
            Debug.Log("GAME OVER! YOU WIN!");
            
            // Play Win Fanfare
            if (GameAudioManager.Instance != null)
                GameAudioManager.Instance.PlaySound(GameAudioManager.Instance.winClip, transform.position);
            
            // A. Remove all cups visually
            ClearCups();

            // B. Update UI to show "You Win!"
            if (ScoreBoard.Instance != null)
            {
                ScoreBoard.Instance.ShowWinMessage();
            }

            // C. Remove the Ball to finalize the game state (prevent further interaction)
            BallGameMechanics ball = FindObjectOfType<BallGameMechanics>();
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }

            return; // Stop here (do not try to spawn a rack of 0 cups)
        }

        // 3. Re-Rack (Standard Gameplay)
        // If the game isn't over, destroy the old rack and spawn the new formation.
        SetupRack(currentCupCount);
    }

    /// <summary>
    /// Spawns the cups in the correct formation for the specific count.
    /// </summary>
    /// <param name="count">The number of cups to spawn.</param>
    public void SetupRack(int count)
    {
        // Validation check
        if (count <= 0 || !layouts.ContainsKey(count))
        {
            ClearCups();
            return;
        }

        // Clean up previous cups before spawning new ones
        ClearCups();
        
        SpawnCups(count);
        currentCupCount = count; // Ensure inspector variable stays synced
    }

    /// <summary>
    /// Destroys all currently active cup objects in the scene.
    /// </summary>
    private void ClearCups()
    {
        foreach (GameObject cup in activeCups)
        {
            if (cup != null) Destroy(cup);
        }
        activeCups.Clear();
    }

    /// <summary>
    /// Instantiates cups at the predefined local positions for the given count.
    /// </summary>
    private void SpawnCups(int count)
    {
        List<Vector3> positions = layouts[count];

        foreach (Vector3 localPos in positions)
        {
            GameObject newCup = Instantiate(cupPrefab, transform);
            newCup.transform.localPosition = localPos;
            
            // Rotate (-90, 0, 0) is standard for this specific 3D model to stand upright.
            newCup.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            
            activeCups.Add(newCup);
        }
    }

    /// <summary>
    /// Hardcodes the geometric positions for each rack state (1 to 6 cups).
    /// Uses equilateral triangle math (rowHeight = spacing * 0.866).
    /// </summary>
    private void DefineLayouts()
    {
        // 1. Calculate dimensions
        // Get the actual size of the cup (MeshRenderer bounds) to apply spacing correctly.
        // If no renderer found, assume 8cm diameter.
        float cupDiameter = 0.08f; 
        if (cupPrefab.GetComponent<Renderer>() != null)
             cupDiameter = cupPrefab.GetComponent<Renderer>().bounds.size.x;
        else if (cupPrefab.GetComponentInChildren<Renderer>() != null)
             cupDiameter = cupPrefab.GetComponentInChildren<Renderer>().bounds.size.x;

        float s = cupDiameter * cupSpacingRatio; // Final Spacing Distance
        float rowHeight = s * 0.866f; // Height of an equilateral triangle row

        layouts.Clear();

        // Layout 1: Single Cup
        layouts.Add(1, new List<Vector3>{ new Vector3(0, 0, 0) });

        // Layout 2: Vertical Pair
        layouts.Add(2, new List<Vector3>{ new Vector3(0, 0, 0), new Vector3(0, 0, s) });

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
            new Vector3(0, 0, 0),               // Row 1 (1)
            new Vector3(-s/2, 0, rowHeight),    // Row 2 (2)
            new Vector3(s/2, 0, rowHeight),
            new Vector3(-s, 0, rowHeight * 2),  // Row 3 (3)
            new Vector3(0, 0, rowHeight * 2),
            new Vector3(s, 0, rowHeight * 2)
        });
    }
}