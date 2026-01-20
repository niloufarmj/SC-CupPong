using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip gameStartClip;
    public AudioClip bounceClip; // ball-hit-table
    public AudioClip missClip;
    public AudioClip scoreClip;  // water-splash
    public AudioClip winClip;

    private AudioSource bgmSource;

    void Awake()
    {
        // Singleton Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Play Background Music (Looping)
        if (backgroundMusic != null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.volume = 0.3f; // Keep background music subtle
            bgmSource.Play();
        }
    }

    // Helper function to play 3D sounds
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip != null)
        {
            // PlayClipAtPoint creates a temporary AudioSource that destroys itself
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}