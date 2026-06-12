using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;

namespace WorkflowsDemo.Executors;

internal sealed partial class PlanGeneratorExecutor()
    : Executor<CoordinatorResult, WorkflowCompletedSignal>(nameof(PlanGeneratorExecutor))
{
    public override async ValueTask<WorkflowCompletedSignal> HandleAsync(
        CoordinatorResult result,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("PlanGeneratorExecutor runs, generates final plan");

        return new WorkflowCompletedSignal();
    }
}