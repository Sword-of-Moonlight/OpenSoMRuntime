using System;

/// <summary>
/// Runtime Event Condition stores condition data for an event to run.
/// </summary>
public readonly struct RuntimeEventCondition
{
	/// <summary>Default Trigger Definition</summary>
	public static readonly RuntimeEventCondition Default = new RuntimeEventCondition(RuntimeEventConditionType.None, RuntimeEventComparisonType.Equals, 0, 0);

	/// <summary>condition type controls what type of variable the constant is compared against</summary>
	public readonly RuntimeEventConditionType type;

	/// <summary>comparision mode controls the mode of comparision used</summary>
	public readonly RuntimeEventComparisonType comparison;

	/// <summary>A constant to check against</summary>
	public readonly ushort constant;

	/// <summary>The ID of an entity, item or counter</summary>
	public readonly ushort id;

	/// <summary>
	/// Default Constructor.<br/>
	/// Constructs from raw data
	/// </summary>
	public RuntimeEventCondition(RuntimeEventConditionType type, RuntimeEventComparisonType comparison, ushort constant, ushort id)
    {
		this.type = type;
		this.comparison = comparison;
		this.constant = constant;
		this.id = id;
    }

	/// <summary>
	/// Evaluates the condition to see if it passes. If the condition is met, true is returned - false otherwise.
	/// </summary>
	public bool Evaluate()
    {
		switch (type)
        {
			// Return true when there is no comparison
			case RuntimeEventConditionType.None:
				return true;

			// Return based on if the item quanity passes a comparision check
			case RuntimeEventConditionType.ItemQuantity:
				throw new NotImplementedException();
			
			// Return based on if the gold quanity passes a comparision check
			case RuntimeEventConditionType.CoinQuantity:
				throw new NotImplementedException();

			// Return based on if the player strength stat passes a comparision check
			case RuntimeEventConditionType.Strength:
				throw new NotImplementedException();

			// Return based on if the player magic stat passes a comparision check
			case RuntimeEventConditionType.Magic:
				throw new NotImplementedException();

			// Return based on if the player level passes a comparision check
			case RuntimeEventConditionType.Level:
				throw new NotImplementedException();

			// Return based on if the counter passes a comparision check
			case RuntimeEventConditionType.Counter:
				return Compare(GameManager.Instance.SessionData.Counters[id], constant);
        }

		// Anything else will throw an exception
		throw new ArgumentException($"Unknown Runtime Event Condition Type! {{ type = {type} }}");
    }

	/// <summary>
	/// Runs the comparision against two values, according to the comparison type
	/// </summary>
	bool Compare(int a, int b) =>
		comparison switch
        {
			RuntimeEventComparisonType.Equals      => (a == b),
			RuntimeEventComparisonType.NotEquals   => (a != b),
			RuntimeEventComparisonType.GreaterThan => (a >  b),
			RuntimeEventComparisonType.LessThan    => (a <  b),
			_ => throw new ArgumentException($"Unknown Runtime Event Comparison Type! {{ type = {comparison} }}")
        };
}