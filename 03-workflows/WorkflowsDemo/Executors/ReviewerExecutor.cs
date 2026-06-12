using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Models;

namespace WorkflowsDemo.Executors;

internal sealed partial class ReviewerExecutor(AIAgent agent)
    : Executor<CoordinatorResult, AgentReviewerFeedback>(nameof(ReviewerExecutor))
{
    public override async ValueTask<AgentReviewerFeedback> HandleAsync(
        CoordinatorResult result,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("ReviewerExecutor runs, sends feedback back to coordinator");

        return new AgentReviewerFeedback(true, "todo");
        // return new AgentReviewerFeedback(false, null);
    }
}