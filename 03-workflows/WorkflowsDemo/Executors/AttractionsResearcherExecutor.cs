using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;

namespace WorkflowsDemo.Executors;

internal sealed partial class AttractionsResearcherExecutor(AIAgent agent)
    : Executor<RequirementsProcessedSignal, ResearchCompletedSignal>(nameof(AttractionsResearcherExecutor))
{
    public override async ValueTask<ResearchCompletedSignal> HandleAsync(
        RequirementsProcessedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("AttractionsResearcherExecutor runs, saves attractions options to state");

        await Task.Delay(5000);

        return new ResearchCompletedSignal();
    }
}