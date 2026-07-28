using UnityEngine;

public static class StringExtensions
{
    /// <summary>
    /// Returns a trimmed string without any invalid characters
    /// </summary>
    public static string Sanitise(this string str)
    {
        // Get the first null terminator position
        int nullPosition = str.IndexOf('\0');

        // If the null terminator is the first character, we will just return an empty string...
        if (nullPosition ==  0)
            return string.Empty;

        // If there is no null terminator, the entire string must be valid...
        if (nullPosition == -1)
            return str;

        return str[..(nullPosition-1)];
    } 
}
