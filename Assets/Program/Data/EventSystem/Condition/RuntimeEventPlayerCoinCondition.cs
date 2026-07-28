using UnityEngine;

public class RuntimeEventPlayerCoinCondition : RuntimeEventCondition
{
    public override RuntimeEventConditionType ConditionType => RuntimeEventConditionType.CoinQuantity;

    public override bool Check(RuntimeEventConditionContext context)
    {
        return ComparisonMode switch
        {
            RuntimeEventComparisonType.Equals      => GameManager.Instance.SessionData.PlayerCoin == ConditionConstant,
            RuntimeEventComparisonType.NotEquals   => GameManager.Instance.SessionData.PlayerCoin != ConditionConstant,
            RuntimeEventComparisonType.GreaterThan => GameManager.Instance.SessionData.PlayerCoin  > ConditionConstant,
            RuntimeEventComparisonType.LessThan    => GameManager.Instance.SessionData.PlayerCoin  < ConditionConstant,
            _ => false,
        };
    }
}
