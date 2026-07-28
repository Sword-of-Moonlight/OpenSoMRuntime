using UnityEngine;

public class RuntimeEventPlayerMagicCondition : RuntimeEventCondition
{
    public override RuntimeEventConditionType ConditionType => RuntimeEventConditionType.Magic;

    public override bool Check(RuntimeEventConditionContext context)
    {
        return ComparisonMode switch
        {
            RuntimeEventComparisonType.Equals      => GameManager.Instance.SessionData.PlayerMagic == ConditionConstant,
            RuntimeEventComparisonType.NotEquals   => GameManager.Instance.SessionData.PlayerMagic != ConditionConstant,
            RuntimeEventComparisonType.GreaterThan => GameManager.Instance.SessionData.PlayerMagic  > ConditionConstant,
            RuntimeEventComparisonType.LessThan    => GameManager.Instance.SessionData.PlayerMagic  < ConditionConstant,
            _ => false,
        };
    }
}
