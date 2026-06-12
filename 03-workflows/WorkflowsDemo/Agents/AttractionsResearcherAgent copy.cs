using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace WorkflowsDemo.Agents;

internal static class AttractionsResearcherAgent
{
    public static AIAgent GetAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            options: new ChatClientAgentOptions()
            {
                Id = "attractions-researcher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = SystemPrompt,
                    Tools = []
                }
            });
    }

    private const string SystemPrompt = "";
}