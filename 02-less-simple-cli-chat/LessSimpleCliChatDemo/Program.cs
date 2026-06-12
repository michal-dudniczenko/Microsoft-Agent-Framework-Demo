using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using LessSimpleCliChatDemo;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#region  OpenTelemetrySetup

const string OpenTelemetrySourceName = "MyApplication";
const string OpenTelemetryServiceName = "MyAgentService";
const string OpenTelemetryEndpoint = "http://localhost:18889";

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(OpenTelemetryServiceName);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource(OpenTelemetrySourceName)
    .AddOtlpExporter(opt => opt.Endpoint = new Uri(OpenTelemetryEndpoint))
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddMeter(OpenTelemetrySourceName)
    .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
    {
        exporterOptions.Endpoint = new Uri(OpenTelemetryEndpoint);
        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000; // 5s
    })
    .Build();

#endregion

const string modelName = "granite4.1:3b";
const string apiKey = "YOUR-API-KEY";
const string apiUrl = "http://172.23.176.1:11434/v1";

IChatClient chatClient =
    new ChatClient(
        model: modelName,
        credential: new ApiKeyCredential(apiKey),
        options: new OpenAIClientOptions
        {
            Endpoint = new Uri(apiUrl)
        })
    .AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(
        sourceName: OpenTelemetrySourceName,
        configure: c => c.EnableSensitiveData = true)
    .Build();

AIAgent agent = new ChatClientAgent(
    chatClient: chatClient,
    options: new ChatClientAgentOptions()
    {
        Name = "MyAgent",
        ChatHistoryProvider = new InMemoryChatHistoryProvider(),
        AIContextProviders = [],
        ChatOptions = new ChatOptions()
        {
            Instructions = "You are a friendly and helpful pirate. Answer like a pirate. Keep your answers brief.",
            Tools = [
                AIFunctionFactory.Create(Tools.GetWeather),
                new ApprovalRequiredAIFunction(AIFunctionFactory.Create(Tools.CountCharacterOccurrences)),
                AIFunctionFactory.Create(Tools.ReverseString),
            ],
        },
    })
    .AsBuilder()
    .Use(
        runFunc: Middleware.LoggingMiddleware,
        runStreamingFunc: Middleware.LoggingStreamingMiddleware)
    .Use(
        runFunc: Middleware.ApiKeyGuardMiddleware,
        runStreamingFunc: Middleware.ApiKeyGuardStreamingMiddleware)
    .Build();

#region SessionManagement

string sessionFilePath = Path.Combine(Directory.GetCurrentDirectory(), "session.json");
AgentSession session;

if (File.Exists(sessionFilePath))
{
    try
    {
        Console.WriteLine("Restoring session from session.json...");
        var jsonString = await File.ReadAllTextAsync(sessionFilePath);
        using var doc = JsonDocument.Parse(jsonString);
        session = await agent.DeserializeSessionAsync(doc.RootElement.Clone());
        Console.WriteLine("Session restored successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to restore session: {ex.Message}. Creating a new session...");
        session = await agent.CreateSessionAsync();
    }
}
else
{
    session = await agent.CreateSessionAsync();
}

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nSaving session and exiting...");
    SaveSessionAsync(agent, session, sessionFilePath).GetAwaiter().GetResult();
    Environment.Exit(0);
};

#endregion

Console.Write("\n=======================================================\n\n");

while (true)
{
    Console.Write("PROMPT:\t\t");

    var prompt = string.Empty;
    while (string.IsNullOrWhiteSpace(prompt))
    {
        prompt = Console.ReadLine();
    }

    prompt = prompt.Trim();

    if (prompt.Equals("/clear", StringComparison.OrdinalIgnoreCase))
    {
        if (File.Exists(sessionFilePath))
        {
            try
            {
                File.Delete(sessionFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete session file: {ex.Message}");
            }
        }
        session = await agent.CreateSessionAsync();
        Console.WriteLine("Session cleared. New session created.");
        Console.Write("\n=======================================================\n\n");
        continue;
    }

    if (prompt.Equals("/exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Saving session and exiting...");
        await SaveSessionAsync(agent, session, sessionFilePath);
        return;
    }

    Console.Write("RESPONSE:\t");

    bool needsApproval = true;

    // First call - use a string prompt
    object currentInput = prompt;

    while (needsApproval)
    {
        needsApproval = false;
        var approvalRequests = new List<ToolApprovalRequestContent>();

        // Stream the current run
        IAsyncEnumerable<AgentResponseUpdate> stream = currentInput is string s
            ? agent.RunStreamingAsync(s, session)
            : agent.RunStreamingAsync((ChatMessage)currentInput, session);

        await foreach (var update in stream)
        {
            Console.Write(update.Text);

            // Collect any approval requests in this update
            approvalRequests.AddRange(
                update.Contents.OfType<ToolApprovalRequestContent>());
        }

        // After stream ends - handle any approval requests
        if (approvalRequests.Count > 0)
        {
            needsApproval = true;

            // For simplicity: handle one at a time
            // In practice, batch all of them into a single ChatMessage
            var request = approvalRequests[0];

            var toolCall = (FunctionCallContent)request.ToolCall;

            var argsDisplay = toolCall.Arguments?.Any() == true
                ? $"({string.Join(", ", toolCall.Arguments)})"
                : "";

            Console.WriteLine($"\nApproval needed: '{toolCall.Name}{argsDisplay}");
            Console.Write("Approve? (y/n): ");
            bool approved = Console.ReadLine()?.Trim().ToLower() == "y";

            var approvalMessage = new ChatMessage(
                ChatRole.User,
                [request.CreateResponse(approved)]);

            currentInput = approvalMessage;
        }
    }

    Console.Write("\n\n=======================================================\n\n");
}

static async Task SaveSessionAsync(AIAgent agent, AgentSession session, string filePath)
{
    try
    {
        var jsonElement = await agent.SerializeSessionAsync(session);
        var jsonString = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, jsonString);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error serializing session: {ex.Message}");
    }
}

