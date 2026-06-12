using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace WorkflowsDemo.Agents;

internal static class PlanGeneratorAgent
{
    public static AIAgent GetAgent(IChatClient chatClient)
    {
        return new ChatClientAgent(
            chatClient,
            options: new ChatClientAgentOptions()
            {
                Id = "plan-generator",
                ChatOptions = new ChatOptions()
                {
                    Instructions = SystemPrompt,
                    Tools = []
                }
            });
    }

    private const string SystemPrompt = "";
}