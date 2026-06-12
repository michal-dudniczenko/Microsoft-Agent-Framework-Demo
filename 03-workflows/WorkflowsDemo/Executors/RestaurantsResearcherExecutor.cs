using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;

namespace WorkflowsDemo.Executors;

internal sealed partial class RestaurantsResearcherExecutor(AIAgent agent)
    : Executor<RequirementsProcessedSignal, ResearchCompletedSignal>(nameof(RestaurantsResearcherExecutor))
{
    public override async ValueTask<ResearchCompletedSignal> HandleAsync(
        RequirementsProcessedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("RestaurantsResearcherExecutor runs, saves restaurants options to state");

        await Task.Delay(5000);

        return new ResearchCompletedSignal();
    }
}