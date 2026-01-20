using UnityEngine;

/// <summary>
/// A centralized audio system for the game.
/// Implements the Singleton pattern to allow easy access from any other script.
/// Handles:
/// 1. Looping background music on startup.
/// 2. Providing a helper method to spawn 3D sound effects at specific locations.
/// </summary>
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance; // Singleton instance

    [Header("Audio Clips")]
    [Tooltip("Ambient music that loops in the background.")]
    public AudioClip backgroundMusic;
    [Tooltip("Played when the table is selected and the game begins.")]
    public AudioClip gameStartClip;
    [Tooltip("Played when the ball hits the table surface.")]
    public AudioClip bounceClip; 
    [Tooltip("Played when the ball hits the floor or goes out of bounds.")]
    public AudioClip missClip;
    [Tooltip("Played when the ball lands inside a cup (Water Splash).")]
    public AudioClip scoreClip;  
    [Tooltip("Played when the final cup is removed.")]
    public AudioClip winClip;

    private AudioSource bgmSource;

    void Awake()
    {
        // Singleton Initialization
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Initialize and Play Background Music
        if (backgroundMusic != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true; // Ensure it never stops
            bgmSource.volume = 0.3f; // Keep background music subtle so SFX can be heard
            bgmSource.Play();
        }
    }

    /// <summary>
    /// Spawns a temporary AudioSource to play a sound clip at a specific world position.
    /// Use this for spatial 3D sounds (e.g., a bounce happening on the left side of the table).
    /// </summary>
    /// <param name="clip">The audio file to play.</param>
    /// <param name="position">World coordinates where the sound originates.</param>
    /// <param name="volume">Volume scale (0.0 to 1.0).</param>
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip != null)
        {
            // PlayClipAtPoint creates a "One Shot" AudioSource that automatically destroys itself after playing.
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}