using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;
using TMPro;


public class AudioDebugTool : DebugTool
{
    [SerializeField] TextMeshProUGUI debugField;

    List<FileInfo> wavFiles, midFiles, rmiFiles, sndFiles;
    List<FileInfo> combine = new List<FileInfo>();

    public int currentFileIndex = 0;
    public FileInfo currentFile = null;

    AudioResource audioResource;

    protected override void Awake()
    {

        base.Awake();

        // Load Files...
        wavFiles = ResourceManager.Enumerate("*.wav");
        combine.AddRange(wavFiles);

        midFiles = ResourceManager.Enumerate("*.mid");
        combine.AddRange(midFiles);

        rmiFiles = ResourceManager.Enumerate("*.rmi");
        combine.AddRange(rmiFiles);

        sndFiles = ResourceManager.Enumerate("*.snd");    
        combine.AddRange(sndFiles);

        SetDebugText();
    }

    public void NextFile()
    {
        currentFileIndex++;

        if (currentFileIndex > combine.Count)
            currentFileIndex = 0;

        SetDebugText();
    }

    public void PrevFile()
    {
        currentFileIndex--;

        if (currentFileIndex < 0)
            currentFileIndex = combine.Count - 1;

        SetDebugText();
    }

    public void PlayFile()
    {
        currentFile = combine[currentFileIndex];

        // Stop music playback
        MusicManager.Instance.Stop();

        if (!currentFile.Extension.Equals(".snd", StringComparison.InvariantCultureIgnoreCase))
            MusicManager.Instance.Play(currentFile.Name, true);
        else
        {
            if (audioResource != null)
                audioResource.Free();

            ulong resourceName = ResourceManager.Load<AudioResource>(currentFile.FullName);
            audioResource = ResourceManager.Get<AudioResource>(resourceName);

            SoundManager.Instance.Play2D(audioResource.Get());
        }

        SetDebugText();
    }

    void SetDebugText() =>
        debugField.SetText($"Counts:\n  WAV = {wavFiles.Count}\n  MID = {midFiles.Count}\n  RMI = {rmiFiles.Count}\n  SND = {sndFiles.Count}\n\nCurrent File = {currentFile?.Name}\nFile No. {currentFileIndex:D4}/{combine.Count:D4}");
}
