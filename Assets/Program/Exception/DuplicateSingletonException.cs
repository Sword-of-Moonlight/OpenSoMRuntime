using System;

/// <summary>
/// Raise this exception when you want to make sure that more than one instance of a singleton does not exist.
/// </summary>
public class DuplicateSingletonException : Exception
{
    // Using reflection here. Probably shouldn't since it might brick IL2CPP in the future.
    public override string Message => $"Cannot have more than once instance of '{TargetSite.DeclaringType}'!";
}
