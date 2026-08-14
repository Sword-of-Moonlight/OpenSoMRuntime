using UnityEngine;
using System.IO;
using MeltySynth;


[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("References (Internal)")]
    [SerializeField] AudioSource unitySource;

    [Header("Debugging")]
    [SerializeField, ReadOnly] MusicMode musicMode;

    // Data
    AudioResource currentAudioResource;

    SynthesizerSettings synthesizerSettings;
    Synthesizer synthesizer;
    MidiFileSequencer midiSequencer;

    /// <summary>Singleton Instance</summary>
    public static MusicManager Instance { get; private set; }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Awake()
    {
        // Singleton Implementation
        if (Instance != null)
            throw new DuplicateSingletonException();

        Instance = this;

        // Setup synthesizer and sequencer
        AudioConfiguration unityAudioConfiguration = AudioSettings.GetConfiguration();

        synthesizerSettings = new SynthesizerSettings
        {
            SampleRate            = unityAudioConfiguration.sampleRate,
            MaximumPolyphony      = 64,
            BlockSize             = unityAudioConfiguration.dspBufferSize,
            EnableReverb          = true,
            EnableChorus          = false
        };

        synthesizer = new Synthesizer(new SoundFont(Path.Combine(Path.GetFullPath(Application.streamingAssetsPath), "SoundFont", "gm.sf2")), synthesizerSettings);

        midiSequencer = new MidiFileSequencer(synthesizer);
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void OnDestroy()
    {
        if (currentAudioResource != null)
            currentAudioResource.Free();

        Instance = null;
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Used to render sequenced audio into the unity DSP chain
    /// </summary>
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (musicMode == MusicMode.Sequenced)
            midiSequencer.RenderInterleaved(data);          
    }

    /// <summary>
    /// Plays a music track
    /// </summary>
    public void Play(string musicFileName, bool loop)
    {
        // Get path to the music file...
        if (!ResourceManager.Find($"DATA\\SOUND\\BGM\\{musicFileName}", out string musicFilePath))
            return;
            
        switch (Path.GetExtension(musicFilePath).ToUpperInvariant())
        {
            case ".WAV" or "WAVE":
            case ".SND":
                StartWaveformAudio(musicFilePath, loop);
                break;

            case ".MID" or "MIDI":
            case ".SMF":
                StartSequencedAudio(musicFilePath, loop);
                break;

            default:
                Logger.Error($"Unsupported audio format extension: '{Path.GetExtension(musicFilePath)}'");
                break;
        }
    }

    /// <summary>
    /// Stops a music track
    /// </summary>
    public void Stop()
    {
        // Unity Source Clean Up
        if (unitySource.isPlaying)
            unitySource.Stop();

        unitySource.clip = null;

        // PCM Source Clean up
        if (currentAudioResource != null)
        {
            currentAudioResource.Free();
            currentAudioResource = null;
        }

        // Sequenced Source Cleanup
        if (midiSequencer.MidiFile != null)
            midiSequencer.Stop();

        musicMode = MusicMode.None;
    }

    /// <summary>
    /// Pauses a music track
    /// </summary>
    public void Pause()
    {
        switch (musicMode)
        {
            case MusicMode.Sequenced:
                midiSequencer.TogglePause();
                break;

            case MusicMode.Waveform:
                if (unitySource.clip != null)
                {
                    if (unitySource.isPlaying)
                        unitySource.Pause();
                    else
                        unitySource.UnPause();
                }         
                break;
        }
    }

    /// <summary>
    /// Start waveform (pcm) audio playback
    /// </summary>
    void StartWaveformAudio(string filename, bool loop)
    {
        // Stop previous music
        Stop();

        // Load the resource async style
        ResourceManager.LoadAsync<AudioResource>(filename, null, (resourceName) => OnWaveformLoadComplete(resourceName, loop));
    }

    /// <summary>
    /// Resource Manager Callback.<br/>
    /// Called when PCM audio has finished loading (WAV, SND)
    /// </summary>
    void OnWaveformLoadComplete(ulong resourceName, bool loop)
    {
        // Get the audio resource
        currentAudioResource = ResourceManager.Get<AudioResource>(resourceName);

        // Start playback...
        unitySource.clip = currentAudioResource.Get();
        unitySource.loop = loop;
        unitySource.Play();

        musicMode = MusicMode.Waveform;
    }

    /// <summary>
    /// Start sequenced (midi) audio playback
    /// </summary>
    void StartSequencedAudio(string filename, bool loop)
    {
        // Stop Previous Music
        Stop();

        // Load and play midi
        midiSequencer.Play(new MidiFile(filename), loop);
        unitySource.Play();

        musicMode = MusicMode.Sequenced;
    }
}

public enum MusicMode
{
    None      = 0,
    Sequenced = 1,
    Waveform  = 2
}
