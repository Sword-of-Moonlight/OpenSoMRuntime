using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

public class HUDDebugPerf : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] ProfilerRecorder frameTimeRecorder;

    void OnEnable()
    {
        frameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
    }

    void OnDisable()
    {
        frameTimeRecorder.Dispose();
    }

    void Update()
    {
        float frameTimeMS = frameTimeRecorder.LastValue / 1000000f;
        float fps         = 1000f / frameTimeMS;

        textField.SetText($"FPS: {fps}\nFrame Time: {frameTimeMS}\nMemory (Sys, MB): {(Profiler.GetTotalAllocatedMemoryLong()/1024)/1024}\nMemory (Managed, MB): {(Profiler.GetMonoUsedSizeLong() / 1024) / 1024}/{(Profiler.GetMonoHeapSizeLong() / 1024) / 1024}");
    }
}
