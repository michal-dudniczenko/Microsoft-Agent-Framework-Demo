using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace WorkflowsDemo.Agents;

internal static class ReviewerAgent
{
    public static AIAgent GetAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            options: new ChatClientAgentOptions()
            {
                Id = "reviewer",
                ChatOptions = new ChatOptions()
                {
                    Instructions = SystemPrompt,
                    Tools = []
                }
            });
    }

    private const string SystemPrompt = "";
}