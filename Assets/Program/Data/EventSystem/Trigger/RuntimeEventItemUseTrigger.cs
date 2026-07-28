public class RuntimeEventItemUseTrigger : RuntimeEventTrigger
{
    public int TriggerItem { get; private set; }

    public override bool CanRun(RuntimeEventTriggerContext context) =>
        TriggerItem == context.UsedItemID;
}
