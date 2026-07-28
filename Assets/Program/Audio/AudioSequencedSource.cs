using MeltySynth;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSequencedSource : MonoBehaviour
{
    [Header("References (Internal)")]
    [SerializeField] AudioSource unitySource;

    [Header("Configuration")]
    [SerializeField] string sf2Name = "gs.sf2";
    [SerializeField] string smfName = "GameData_NeverQuest\\DATA\\SOUND\\BGM\\mountain.mid";

    // Data
    Synthesizer meltySynth;
    MidiFileSequencer meltySequencer;
    MidiFile meltyMidi;

    [SerializeField] bool pause;

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Awake()
    {
        // Ensure the clip is null..
        unitySource.clip = null;

        //
        // (Temporary) Load SoundFont, Set up synthesizer
        //
        string sf2Path = Path.Combine(Path.GetFullPath(Application.streamingAssetsPath), "SoundFont", sf2Name);

        Debug.Log(File.Exists(sf2Path));

        SynthesizerSettings synthSettings = new SynthesizerSettings(AudioSettings.outputSampleRate)
        {
            SampleRate       = AudioSettings.outputSampleRate,
            MaximumPolyphony = 64
        };

        meltySynth = new Synthesizer(sf2Path, synthSettings);

        //
        // (Temporary) Load Midi and play
        //
        string smfPath = Path.Combine(Path.GetFullPath(Application.streamingAssetsPath), smfName);

        Debug.Log(File.Exists(smfPath));

        meltyMidi = new MidiFile(smfPath);
        meltySequencer = new MidiFileSequencer(meltySynth);

        if (unitySource.playOnAwake)
            meltySequencer.Play(meltyMidi, unitySource.loop);
    }


    void Update()
    {
        if (pause)
        {
            meltySequencer.TogglePause();
            pause = false;
        }
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (meltySequencer != null)
            meltySequencer.RenderInterleaved(data);
    }
}
