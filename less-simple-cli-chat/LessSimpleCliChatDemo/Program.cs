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

const string modelName = "granite4.1:3b";
const string apiKey = "YOUR-API-KEY";
const string apiUrl = "http://172.23.176.1:11434/v1";

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
        ChatOptions = new ChatOptions()
        {
            Instructions = "You are a friendly and helpful pirate. Answer like a pirate. Keep your answers brief.",
            Tools = [
                AIFunctionFactory.Create(Tools.GetWeather),
                AIFunctionFactory.Create(Tools.CountCharacterOccurrences),
                AIFunctionFactory.Create(Tools.ReverseString),
            ]
        }
    })
    .AsBuilder()
    .Use(
        runFunc: Middleware.LoggingMiddleware,
        runStreamingFunc: Middleware.LoggingStreamingMiddleware)
    .Use(
        runFunc: Middleware.ApiKeyGuardMiddleware,
        runStreamingFunc: Middleware.ApiKeyGuardStreamingMiddleware)
    .Build();

AgentSession session = await agent.CreateSessionAsync();

Console.Write("\n=======================================================\n\n");

while (true)
{
    Console.Write("PROMPT:\t\t");

    var prompt = string.Empty;
    while (string.IsNullOrWhiteSpace(prompt))
    {
        prompt = Console.ReadLine();
    }

    Console.Write("RESPONSE:\t");

    await foreach (var update in agent.RunStreamingAsync(prompt, session))
    {
        Console.Write(update);
    }

    Console.Write("\n\n=======================================================\n\n");
}
