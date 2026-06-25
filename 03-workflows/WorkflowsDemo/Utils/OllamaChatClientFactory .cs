using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Utils;

internal static class OllamaChatClientFactory
{
    public static IChatClient Create()
    {
        return new ChatClient(
            model: OllamaModelName,
            credential: new ApiKeyCredential("api-key"),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(OllamaOpenAIUrl),
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
