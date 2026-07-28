using UnityEngine;

[CreateAssetMenu(fileName = "SoMLevelCurve", menuName = "Sword of Moonlight/Level Curve")]
public class SoMLevelCurve : ScriptableObject
{
    [field: SerializeField] public AnimationCurve experienceCurve { get; private set; }
    [field: SerializeField] public AnimationCurve hpCurve { get; private set; }
    [field: SerializeField] public AnimationCurve mpCurve { get; private set; } = new AnimationCurve();
    [field: SerializeField] public AnimationCurve strengthCurve { get; private set; } = new AnimationCurve();
    [field: SerializeField] public AnimationCurve magicCurve { get; private set; } = new AnimationCurve();

    public void Initialize()
    {
        // Get the filename for the current lvt file
        string lvtFile = $"{ResourceManager.ResourceRoot}\\PARAM\\{GameManager.Instance.ProjectData.playerLevelCurveType}.LVT";

        // Initialize Curves
        experienceCurve = new AnimationCurve();
        hpCurve = new AnimationCurve();
        mpCurve = new AnimationCurve();
        strengthCurve = new AnimationCurve();
        magicCurve = new AnimationCurve();

        using FileInputStream fis = new FileInputStream(lvtFile);

        // We must convert relative to absolute for these...
        int hpAccumulator  = GameManager.Instance.ProjectData.playerConfigNormal.startHP;
        int mpAccumulator  = GameManager.Instance.ProjectData.playerConfigNormal.startMP;
        int strAccumulator = GameManager.Instance.ProjectData.playerConfigNormal.startStrength;
        int magAccumulator = GameManager.Instance.ProjectData.playerConfigNormal.startMagic;
        
        // Read all 99 curve entries (1 for each level)
        for (int i = 0; i < 99; ++i)
        {
            // Read data...
            uint experienceToLevel = fis.ReadU32();
            hpAccumulator  += fis.ReadU8();
            mpAccumulator  += fis.ReadU8();
            strAccumulator += fis.ReadU8();
            magAccumulator += fis.ReadU8();

            // Set data in graphs
            experienceCurve.AddKey(new Keyframe { time = (1 + i), value = experienceToLevel, weightedMode = WeightedMode.None });
            hpCurve.AddKey(new Keyframe { time = (1 + i), value = Mathf.Min(999, hpAccumulator), weightedMode = WeightedMode.None });
            mpCurve.AddKey(new Keyframe { time = (1 + i), value = Mathf.Min(999, mpAccumulator), weightedMode = WeightedMode.None });
            strengthCurve.AddKey(new Keyframe { time = (1 + i), value = Mathf.Min(999, strAccumulator), weightedMode = WeightedMode.None });
            magicCurve.AddKey(new Keyframe { time = (1 + i), value = Mathf.Min(999, magAccumulator), weightedMode = WeightedMode.None });
        }
    }
}
