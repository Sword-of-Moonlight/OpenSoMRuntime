using UnityEngine;

public static class Logger
{
    public static string Colourize(this string str, uint colour)
    {
        #if UNITY_EDITOR    // We only want fancy colour inside the editor, or it floods our logs with crap
        return $"<color=#{colour:X6}>{str}</color>";
        #else
        return str;
        #endif
    }

    public static void Info(string message) =>
        Debug.Log($"{"[".Colourize(0x202020)}{"INFO".Colourize(0x8080F0)}{"]: ".Colourize(0x202020)}{message}");
       
    public static void Warn(string message) =>
        Debug.Log($"{"[".Colourize(0x202020)}{"WARN".Colourize(0xF0F080)}{"]: ".Colourize(0x202020)}{message}");

    public static void Error(string message) =>
        Debug.Log($"{"[".Colourize(0x202020)}{"OOPS".Colourize(0xF08080)}{"]: ".Colourize(0x202020)}{message}");

    public static void Critical(string message) =>
        Debug.Log($"{"[".Colourize(0x202020)}{"CRIT".Colourize(0xF08080)}{"]: ".Colourize(0x202020)}{message.Colourize(0XF0F080)}");

    public static void Custom(string header, uint headerColour, string message) =>
        Debug.Log($"{"[".Colourize(0x202020)}{header.Colourize(headerColour)}{"]: ".Colourize(0x202020)}{message}");
}
