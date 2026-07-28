using UnityEngine;

public class RuntimeEventPlayerStrengthCondition : RuntimeEventCondition
{
    public override RuntimeEventConditionType ConditionType => RuntimeEventConditionType.Strength;

    public override bool Check(RuntimeEventConditionContext context)
    {
        return ComparisonMode switch
        {
            RuntimeEventComparisonType.Equals      => GameManager.Instance.SessionData.PlayerStrength == ConditionConstant,
            RuntimeEventComparisonType.NotEquals   => GameManager.Instance.SessionData.PlayerStrength != ConditionConstant,
            RuntimeEventComparisonType.GreaterThan => GameManager.Instance.SessionData.PlayerStrength  > ConditionConstant,
            RuntimeEventComparisonType.LessThan    => GameManager.Instance.SessionData.PlayerStrength  < ConditionConstant,
            _ => false,
        };
    }
}
