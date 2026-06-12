using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;

namespace WorkflowsDemo.Executors;

internal sealed partial class AccomodationResearcherExecutor(AIAgent agent)
    : Executor<RequirementsProcessedSignal, ResearchCompletedSignal>(nameof(AccomodationResearcherExecutor))
{
    public override async ValueTask<ResearchCompletedSignal> HandleAsync(
        RequirementsProcessedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("AccomodationResearcherExecutor runs, saves accomodation options to state");

        await Task.Delay(5000);

        return new ResearchCompletedSignal();
    }
}