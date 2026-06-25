using LessSimpleCliChatDemo;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.ClientModel;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

#region  OpenTelemetrySetup

const string OpenTelemetrySourceName = "Agents.Demo";
const string OpenTelemetryServiceName = "AgentsDemo";
const string OpenTelemetryEndpoint = "http://localhost:4317";

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

const string modelName = "MODEL-NAME";
const string apiKey = "API-KEY";
const string apiUrl = "API-URL";

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

const string AgentName = "MyAgent";

AIAgent agent = new ChatClientAgent(
    chatClient: chatClient,
    options: new ChatClientAgentOptions()
    {
        Name = AgentName,
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

#region DevUI Setup

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();
builder.Services.AddDevUI();

builder.AddAIAgent(AgentName, (_, _) => agent);

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services
        .GetRequiredService<IServer>()
        .Features
        .Get<IServerAddressesFeature>()
        ?.Addresses;

    var address = addresses?.FirstOrDefault();

    Console.WriteLine("\n=================================================================\n");
    Console.WriteLine($"DevUI running at: {address}/devui");
});

app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();

_ = app.RunAsync();

#endregion

Console.Write("\n=======================================================\n\n");

while (true)
{
    var prompt = ReadPrompt();

    if (prompt is null || IsCommand(prompt, "/exit"))
    {
        await ExitAsync();
        return;
    }

    if (IsCommand(prompt, "/clear"))
    {
        await ClearSessionAsync();
        continue;
    }

    await RunPromptAsync(prompt);

    Console.Write("\n\n=======================================================\n\n");
}

string? ReadPrompt()
{
    while (true)
    {
        Console.Write("PROMPT:\t\t");

        var input = Console.ReadLine();

        // Handles Ctrl+Z / redirected stdin ending.
        if (input is null)
        {
            return null;
        }

        input = input.Trim();

        if (!string.IsNullOrWhiteSpace(input))
        {
            return input;
        }
    }
}

static bool IsCommand(string prompt, string command)
{
    return prompt.Equals(command, StringComparison.OrdinalIgnoreCase);
}

async Task ExitAsync()
{
    Console.WriteLine("Saving session and exiting...");
    await SaveSessionAsync(agent, session, sessionFilePath);
}

async Task ClearSessionAsync()
{
    try
    {
        if (File.Exists(sessionFilePath))
        {
            File.Delete(sessionFilePath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not delete session file: {ex.Message}");
    }

    session = await agent.CreateSessionAsync();

    Console.WriteLine("Session cleared. New session created.");
    Console.Write("\n=======================================================\n\n");
}

async Task RunPromptAsync(string prompt)
{
    var approvalRequests = await StreamAndCollectApprovalsAsync(
        agent.RunStreamingAsync(prompt, session));

    while (approvalRequests.Count > 0)
    {
        var approvalResponses = CreateApprovalResponseMessages(approvalRequests);

        approvalRequests = await StreamAndCollectApprovalsAsync(
            agent.RunStreamingAsync(approvalResponses, session));
    }
}

async Task<List<ToolApprovalRequestContent>> StreamAndCollectApprovalsAsync(
    IAsyncEnumerable<AgentResponseUpdate> stream)
{
    var hasStartedResponse = false;
    var approvalRequests = new List<ToolApprovalRequestContent>();

    await foreach (var update in stream)
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            if (!hasStartedResponse)
            {
                Console.Write("RESPONSE:\t");
                hasStartedResponse = true;
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            }

            Console.Write(update.Text);
        }

        approvalRequests.AddRange(
            update.Contents.OfType<ToolApprovalRequestContent>());
    }
    Console.ResetColor();

    return approvalRequests;
}

List<ChatMessage> CreateApprovalResponseMessages(
    IEnumerable<ToolApprovalRequestContent> approvalRequests)
{
    var messages = new List<ChatMessage>();

    foreach (var request in approvalRequests)
    {
        var approved = AskForApproval(request);

        messages.Add(new ChatMessage(
            ChatRole.User,
            [request.CreateResponse(approved)]));
    }

    return messages;
}

bool AskForApproval(ToolApprovalRequestContent request)
{
    var toolCall = (FunctionCallContent)request.ToolCall;

    Console.WriteLine();
    Console.WriteLine($"Approval needed: '{toolCall.Name}{FormatArguments(toolCall)}'");

    while (true)
    {
        Console.Write("Approve? (y/n): ");

        var input = Console.ReadLine()?.Trim();

        Console.WriteLine();

        if (input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true ||
            input?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (input is null ||
            input.Equals("n", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Console.WriteLine("Please enter 'y' or 'n'.");
    }
}

static string FormatArguments(FunctionCallContent toolCall)
{
    if (toolCall.Arguments is null || !toolCall.Arguments.Any())
    {
        return string.Empty;
    }

    var args = toolCall.Arguments.Select(arg => $"{arg.Key}: {arg.Value}");

    return $"({string.Join(", ", args)})";
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
