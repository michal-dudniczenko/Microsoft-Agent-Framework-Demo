using System.ComponentModel;

namespace LessSimpleCliChatDemo;

internal static class Tools
{
    [Description("Get the weather for a given location.")]
    public static string GetWeather(
        [Description("The location to get the weather for.")] string location)
    {
        var normalizedLocation = location.Trim().ToUpperInvariant();

        string weather = normalizedLocation switch
        {
            "KATOWICE" => "sunny, clear sky and 25°C",
            "WROCLAW" or "WROCŁAW" => "very hot, dry and 42°C",
            "POZNAN" or "POZNAŃ" => "thunderstorms expected, 7°C",
            _ => "cloudy with a high of 15°C."
        };

        return $"The weather in {location} is {weather}";
    }

    [Description("Count how many times a specific character appears in a given string.")]
    public static string CountCharacterOccurrences(
        [Description("The text to analyze.")] string text,
        [Description("The character to count.")] char character)
    {
        if (string.IsNullOrEmpty(text))
            return "The text is empty.";

        int count = text.Count(c =>
            char.ToLowerInvariant(c) == char.ToLowerInvariant(character));

        return $"The character '{character}' appears {count} time(s) in \"{text}\".";
    }

    [Description("Reverse the characters in a given string.")]
    public static string ReverseString(
        [Description("The text to reverse.")] string text)
    {
        if (string.IsNullOrEmpty(text))
            return "The text is empty.";

        char[] chars = text.ToCharArray();
        Array.Reverse(chars);

        return new string(chars);
    }
}
