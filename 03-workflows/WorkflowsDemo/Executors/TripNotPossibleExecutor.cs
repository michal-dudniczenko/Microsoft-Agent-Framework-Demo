using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;

namespace WorkflowsDemo.Executors;

internal sealed partial class TripNotPossibleExecutor()
    : Executor<TripNotPossibleEvent, WorkflowCompletedSignal>(nameof(TripNotPossibleExecutor))
{
    public override async ValueTask<WorkflowCompletedSignal> HandleAsync(
        TripNotPossibleEvent evt,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Unfortunately i am not able to come up with a plan that would satisfy your requirements."
            + $"\nReason: {evt.Explanation}");
        
        return new WorkflowCompletedSignal();
    }
}