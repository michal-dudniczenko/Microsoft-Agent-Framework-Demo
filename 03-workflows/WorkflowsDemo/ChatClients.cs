using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo;

internal static class ChatClients
{
    public static IChatClient GetOllamaChatClient()
    {
        return new ChatClient(
            model: OllamaModelName,
            credential: new ApiKeyCredential("anything"),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(OllamaUrl),
                NetworkTimeout = TimeSpan.FromMinutes(10)
            })
            .AsIChatClient()
            .AsBuilder()
            .UseOpenTelemetry(
                sourceName: OpenTelemetrySourceName,
                configure: c => c.EnableSensitiveData = true)
            .Build();
    }

    public static IChatClient GetOpenRouterChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable(OpenRouterApiKeyEnvVarName) ?? OpenRouterApiKey;

        return new ChatClient(
            model: OpenRouterModelName,
            credential: new ApiKeyCredential(apiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(OpenRouterOpenAIUrl),
                NetworkTimeout = TimeSpan.FromMinutes(30)
            })
            .AsIChatClient()
            .AsBuilder()
            .UseOpenTelemetry(
                sourceName: OpenTelemetrySourceName,
                configure: c => c.EnableSensitiveData = true)
            .Build();
    }
}