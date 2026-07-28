using UnityEngine;

public class RuntimeEventOverlapCircleTrigger : RuntimeEventTrigger
{
    public float TriggerRadius { get; private set; }

    public override bool CanRun(RuntimeEventTriggerContext context) =>
        Vector3.Distance(context.EventPosition, context.PlayerPosition) < TriggerRadius;
}
