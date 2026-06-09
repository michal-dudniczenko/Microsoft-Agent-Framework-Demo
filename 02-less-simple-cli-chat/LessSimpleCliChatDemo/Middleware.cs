using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace LessSimpleCliChatDemo;

internal static class Middleware
{
    public static async Task<AgentResponse> LoggingMiddleware(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        var prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "(no user message)";

        var response = await innerAgent.RunAsync(messages, session, options, cancellationToken);

        var responseText = string.Join(" ", response.Messages
            .Where(m => m.Role == ChatRole.Assistant)
            .Select(m => m.Text));

        var entry = $"""
        [{DateTimeOffset.Now:O}]
        PROMPT: {prompt}
        RESPONSE: {responseText}
        """ + "\n\n";

        await File.AppendAllTextAsync("agent-log.txt", entry, cancellationToken);

        return response;
    }

    public static async IAsyncEnumerable<AgentResponseUpdate> LoggingStreamingMiddleware(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "(no user message)";

        var responseBuilder = new StringBuilder();

        await foreach (var update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
        {
            // Capture assistant text as it streams
            if (!string.IsNullOrEmpty(update.Text))
            {
                responseBuilder.Append(update.Text);
            }

            // Forward immediately to the caller
            yield return update;
        }

        var entry = $"""
        [{DateTimeOffset.Now:O}]
        PROMPT: {prompt}
        RESPONSE: {responseBuilder}
        """ + "\n\n";

        await File.AppendAllTextAsync("agent-log.txt", entry, cancellationToken);
    }

    public static async Task<AgentResponse> ApiKeyGuardMiddleware(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        var prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";

        if (ContainsApiKey(prompt))
        {
            // innerAgent is never called — pipeline is short-circuited
            return new AgentResponse
            {
                Messages = [new ChatMessage(ChatRole.Assistant, "Request blocked: possible API key detected.")]
            };
        }

        return await innerAgent.RunAsync(messages, session, options, cancellationToken);
    }

    public static async IAsyncEnumerable<AgentResponseUpdate> ApiKeyGuardStreamingMiddleware(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var prompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";

        if (ContainsApiKey(prompt))
        {
            yield return new AgentResponseUpdate(ChatRole.Assistant, "Request blocked: possible API key detected.");
            yield break;
        }

        await foreach (var update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
        {
            yield return update;
        }
    }

    private static readonly string[] ApiKeyKeywords = ["api key", "api-key", "api_key", "apikey"];

    private static bool ContainsApiKey(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return false;

        if (ApiKeyKeywords.Any(k => prompt.Contains(k, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}