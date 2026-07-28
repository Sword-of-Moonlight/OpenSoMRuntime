using UnityEngine;

public class RuntimeEventPlayerLevelCondition : RuntimeEventCondition
{
    public override RuntimeEventConditionType ConditionType => RuntimeEventConditionType.Level;

    public override bool Check(RuntimeEventConditionContext context)
    {
        return ComparisonMode switch
        {
            RuntimeEventComparisonType.Equals      => GameManager.Instance.SessionData.PlayerLevel == ConditionConstant,
            RuntimeEventComparisonType.NotEquals   => GameManager.Instance.SessionData.PlayerLevel != ConditionConstant,
            RuntimeEventComparisonType.GreaterThan => GameManager.Instance.SessionData.PlayerLevel  > ConditionConstant,
            RuntimeEventComparisonType.LessThan    => GameManager.Instance.SessionData.PlayerLevel  < ConditionConstant,
            _ => false,
        };
    }
}
