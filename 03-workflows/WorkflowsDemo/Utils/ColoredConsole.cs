namespace WorkflowsDemo.Utils;

internal static class ColoredConsole
{
    public static void WriteLine(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ResetColor();
    }

    public static void Write(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
}
