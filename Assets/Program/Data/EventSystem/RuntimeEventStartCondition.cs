using UnityEngine;

public struct RuntimeEventStartCondition
{
    public ushort conditionType;
    public ushort targetID;         // conditionType = 1: 0x00->0xF9 = Item ID, 0xFF = None. conditionType == 6: 0x0000->0x03FF = counter ID
    public ushort comparisonValue;  // The value to compare to
    public ushort comparisonType;   // How to compare the value
}
