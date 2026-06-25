using System.Text.Json;

namespace WorkflowsDemo.Utils;

internal static class ObjectExtensions
{
    private const string OutputDirectoryName = "model-responses";
    private static readonly JsonSerializerOptions JsonSerializerPrettyPrint = new() { WriteIndented = true };

    public static void SaveModelResponse(this object? response, string fileName, bool insertSeparator = false)
    {
        var textToWrite = JsonSerializer.Serialize(response, JsonSerializerPrettyPrint);

        if (insertSeparator)
            textToWrite += "\n\n======================================================\n\n";

        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), OutputDirectoryName, $"{fileName}.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        File.AppendAllText(outputPath, textToWrite);
    }
}
