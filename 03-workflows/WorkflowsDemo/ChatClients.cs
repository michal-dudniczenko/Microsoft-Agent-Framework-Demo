using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo;

internal static class ChatClients
{
    public static IChatClient GetOllamaChatClient()
    {
        return GetChatClient(
            modelName: OllamaModelName,
            endpointUrl: OllamaOpenAIUrl
        );
    }

    public static IChatClient GetOpenRouterChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable(OpenRouterApiKeyEnvVarName) ?? OpenRouterApiKey;

        return GetChatClient(
            modelName: OpenRouterModelName,
            endpointUrl: OpenRouterOpenAIUrl,
            apiKey: apiKey
        );
    }

    private static IChatClient GetChatClient(string modelName, string endpointUrl, string apiKey = "api-key")
    {
        return new ChatClient(
            model: modelName,
            credential: new ApiKeyCredential(apiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(endpointUrl),
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
