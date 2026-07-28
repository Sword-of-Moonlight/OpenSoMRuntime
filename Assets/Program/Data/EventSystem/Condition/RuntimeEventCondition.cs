public class RuntimeEventCondition
{
	/// <summary>condition type controls what type of variable the constant is compared against</summary>
	public virtual RuntimeEventConditionType ConditionType => RuntimeEventConditionType.None;

	/// <summary>comparision mode controls the mode of comparision used</summary>
	public RuntimeEventComparisonType ComparisonMode { get; set; } = RuntimeEventComparisonType.Equals;

	/// <summary>The condition constant is compared using comparison mode against the variable chosen by condition type</summary>
	public ushort ConditionConstant { get; set; } = 0;
	
	public virtual bool Check(RuntimeEventConditionContext context)
		=> true;
}

public enum RuntimeEventConditionType : ushort
{
	None		 = 0,
	ItemQuantity = 1,
	CoinQuantity = 2,
	Strength	 = 3,
	Magic		 = 4,
	Level	     = 5,
	Counter	     = 6
}

public enum RuntimeEventComparisonType : ushort
{
	Equals      = 0,
	NotEquals   = 1,
	GreaterThan = 2,
	LessThan    = 3
}