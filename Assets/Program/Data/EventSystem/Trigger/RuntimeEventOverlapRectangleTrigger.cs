using UnityEngine;

public class RuntimeEventOverlapRectangleTrigger : RuntimeEventTrigger
{
    public float TriggerSizeX { get; private set; }
    public float TriggerSizeZ { get; private set; }
    public float TriggerSizeY { get; private set; }

    public override bool CanRun(RuntimeEventTriggerContext context)
    {
        Vector3 aabbMin = context.EventPosition - new Vector3(TriggerSizeX, TriggerSizeY, TriggerSizeZ);
        Vector3 aabbMax = context.EventPosition - new Vector3(TriggerSizeX, TriggerSizeY, TriggerSizeZ);

        return
            context.PlayerPosition.x > aabbMin.x && context.PlayerPosition.x < aabbMax.x &&
            context.PlayerPosition.y > aabbMin.y && context.PlayerPosition.y < aabbMax.y &&
            context.PlayerPosition.z > aabbMin.z && context.PlayerPosition.z < aabbMax.z;
    }
}
