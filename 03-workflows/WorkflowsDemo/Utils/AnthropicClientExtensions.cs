using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Utils;

internal static class AnthropicClientExtensions
{
    public static AIAgent AsClaudeSonnetAgent(
        this IAnthropicClient client,
        string modelName,
        bool thinkingEnabled,
        Effort effort,
        string agentId,
        string? systemPrompt = null,
        IList<AITool>? tools = null,
        int maxOutputTokens = 10000)
    {
        return client.AsAIAgent(
            options: new ChatClientAgentOptions()
            {
                Id = agentId,
                ChatOptions = new ChatOptions()
                {
                    Instructions = systemPrompt,
                    Tools = tools,
                    RawRepresentationFactory = (_) => new MessageCreateParams()
                    {
                        Model = modelName,
                        MaxTokens = maxOutputTokens,
                        Messages = [],
                        Thinking = thinkingEnabled
                            ? new ThinkingConfigParam(new ThinkingConfigEnabled(budgetTokens: maxOutputTokens / 2))
                            : new ThinkingConfigParam(new ThinkingConfigDisabled()),
                        OutputConfig = new OutputConfig()
                        {
                            Effort = effort
                        }
                    }
                }
            },
            clientFactory: (client) => client
                .AsBuilder()
                .UseOpenTelemetry(
                    sourceName: OpenTelemetrySourceName,
                    configure: c => c.EnableSensitiveData = true)
                .Build()
        );
    }

    public static AIAgent AsClaudeHaikuAgent(
        this IAnthropicClient client,
        string modelName,
        bool thinkingEnabled,
        string agentId,
        string? systemPrompt = null,
        IList<AITool>? tools = null,
        int maxOutputTokens = 10000)
    {
        return client.AsAIAgent(
            options: new ChatClientAgentOptions()
            {
                Id = agentId,
                ChatOptions = new ChatOptions()
                {
                    Instructions = systemPrompt,
                    Tools = tools,
                    RawRepresentationFactory = (_) => new MessageCreateParams()
                    {
                        Model = modelName,
                        MaxTokens = maxOutputTokens,
                        Messages = [],
                        Thinking = thinkingEnabled
                            ? new ThinkingConfigParam(new ThinkingConfigEnabled(budgetTokens: maxOutputTokens / 2))
                            : new ThinkingConfigParam(new ThinkingConfigDisabled())
                    }
                }
            },
            clientFactory: (client) => client
                .AsBuilder()
                .UseOpenTelemetry(
                    sourceName: OpenTelemetrySourceName,
                    configure: c => c.EnableSensitiveData = true)
                .Build()
        );
    }
}
