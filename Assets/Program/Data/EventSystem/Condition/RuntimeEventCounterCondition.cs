using UnityEngine;

public class RuntimeEventCounterCondition : RuntimeEventCondition
{
    public override RuntimeEventConditionType ConditionType => RuntimeEventConditionType.Counter;

    public ushort CounterID { get; set; }

    public override bool Check(RuntimeEventConditionContext context)
    {
        return ComparisonMode switch
        {
            RuntimeEventComparisonType.Equals      => GameManager.Instance.SessionData.Counters[CounterID] == ConditionConstant,
            RuntimeEventComparisonType.NotEquals   => GameManager.Instance.SessionData.Counters[CounterID] != ConditionConstant,
            RuntimeEventComparisonType.GreaterThan => GameManager.Instance.SessionData.Counters[CounterID]  > ConditionConstant,
            RuntimeEventComparisonType.LessThan    => GameManager.Instance.SessionData.Counters[CounterID]  < ConditionConstant,
            _                                      => false,
        };
    }
}
