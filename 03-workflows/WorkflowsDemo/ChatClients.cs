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
            model: ollamaModelName,
            credential: new ApiKeyCredential("anything"),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(ollamaUrl)
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
        return new ChatClient(
            model: openRouterModelName,
            credential: new ApiKeyCredential(openRouterApiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(openRouterUrl)
            })
            .AsIChatClient()
            .AsBuilder()
            .UseOpenTelemetry(
                sourceName: OpenTelemetrySourceName,
                configure: c => c.EnableSensitiveData = true)
            .Build();
    }
}