using UnityEngine;
using TMPro;

/// <summary>
/// Manages the in-game Scoreboard UI (World Space Canvas).
/// Implements the Singleton pattern so any script (Ball, Cup) can update the score easily.
/// </summary>
public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard Instance; // Singleton instance

    [Header("UI References")]
    [Tooltip("TextMeshPro component for the Green 'HITS' text.")]
    public TextMeshProUGUI hitsText;
    [Tooltip("TextMeshPro component for the Red 'MISSES' text.")]
    public TextMeshProUGUI missesText;

    // Internal score tracking
    private int hits = 0;
    private int misses = 0;

    void Awake()
    {
        // Singleton Initialization
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Increments the Hit counter and updates the display.
    /// </summary>
    public void AddHit()
    {
        hits++;
        UpdateUI();
    }

    /// <summary>
    /// Increments the Miss counter and updates the display.
    /// </summary>
    public void AddMiss()
    {
        misses++;
        UpdateUI();
    }

    /// <summary>
    /// Refreshes the text components with current values.
    /// </summary>
    private void UpdateUI()
    {
        if (hitsText != null) hitsText.text = $"HITS: {hits}";
        if (missesText != null) missesText.text = $"MISSES: {misses}";
    }

    /// <summary>
    /// Called by CupRackManager when the game is won.
    /// Hides the score details and displays a large victory message.
    /// </summary>
    public void ShowWinMessage()
    {
        // 1. Hide the "Misses" line to clean up the UI
        if (missesText != null) missesText.gameObject.SetActive(false);

        // 2. Hijack the "Hits" line to show the Win Message
        if (hitsText != null)
        {
            hitsText.text = "Congratulations!\nYou Win!";
            hitsText.color = Color.yellow;
            hitsText.alignment = TextAlignmentOptions.Center;
            // Optional: Increase font size via code if desired
            // hitsText.fontSize = 50; 
        }
    }
}