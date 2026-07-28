using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource globalSource2D;

    // Singleton Implementation
    public static SoundManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Singleton Initialization
        if (Instance != null)
            throw new DuplicateSingletonException();

        Instance = this;
    }

    public void Play2D(AudioClip clip)
    {
        if (clip != null)
            globalSource2D.PlayOneShot(clip);
    }
}
