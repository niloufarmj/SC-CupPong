using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard Instance;

    [Header("UI References")]
    public TextMeshProUGUI hitsText;
    public TextMeshProUGUI missesText;

    private int hits = 0;
    private int misses = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddHit()
    {
        hits++;
        UpdateUI();
    }

    public void AddMiss()
    {
        misses++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hitsText != null) hitsText.text = $"HITS: {hits}";
        if (missesText != null) missesText.text = $"MISSES: {misses}";
    }

    // *** NEW FUNCTION ***
    public void ShowWinMessage()
    {
        // 1. Hide the "Misses" line
        if (missesText != null) missesText.gameObject.SetActive(false);

        // 2. Change the "Hits" line to the big Win Message
        if (hitsText != null)
        {
            hitsText.text = "Congratulations!\nYou Win!";
            hitsText.color = Color.yellow;
            hitsText.alignment = TextAlignmentOptions.Center;
            // Optional: Make it bigger if needed
            hitsText.fontSize = 50; 
        }
    }
}