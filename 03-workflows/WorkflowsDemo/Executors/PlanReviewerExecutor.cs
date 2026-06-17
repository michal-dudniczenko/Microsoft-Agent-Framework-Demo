using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.PlanBuilder;
using WorkflowsDemo.Models.PlanReviewer;
using WorkflowsDemo.Utils;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Executors;

internal sealed partial class PlanReviewerExecutor(
    AIAgent agent,
    ILogger<PlanReviewerExecutor> logger)
    : Executor<PlanBuilderResult, PlanReviewerFeedback>(nameof(PlanReviewerExecutor))
{
    public override async ValueTask<PlanReviewerFeedback> HandleAsync(
        PlanBuilderResult planBuilderResult,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var requirements = await context.ReadStateAsync<InitialUserTripRequirements>(
            key: InitialTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Trip requirements are missing from the state.");

        var prompt = JsonSerializer.Serialize(
            new PlanReviewerPrompt(Requirements: requirements, TripPlan: planBuilderResult.TripPlan));

        // File.WriteAllText("agent-prompts/plan-reviewer-prompt.txt", prompt);

        // var result = (await agent.RunAsync<PlanReviewerFeedback>(
        //     prompt,
        //     cancellationToken: cancellationToken)).Result;

        var result = JsonSerializer.Deserialize<PlanReviewerFeedback>(
            File.ReadAllText("agent-responses/plan-reviewer-response.txt")
        );
        await Task.Delay(5000);

        result.SaveModelResponse(fileName: agent.Id, insertSeparator: true);

        logger.LogInformation("Reviewed itinerary, sending feedback back to the plan builder");

        return result;
    }
}
