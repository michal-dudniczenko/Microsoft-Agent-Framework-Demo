using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.PlanBuilder;
using WorkflowsDemo.Models.PlanReviewer;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo.Executors;

internal sealed partial class PlanReviewerExecutor(AIAgent agent)
    : Executor<PlanBuilderResult, PlanReviewerFeedback>(nameof(PlanReviewerExecutor))
{
    public override async ValueTask<PlanReviewerFeedback> HandleAsync(
        PlanBuilderResult planBuilderResult,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("ReviewerExecutor runs, sends feedback back to plan builder");

        var requirements = await context.ReadStateAsync<InitialUserTripRequirements>(
            key: InitialTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Trip requirements are missing from the state.");

        var prompt = JsonSerializer.Serialize(
            new PlanReviewerPrompt(Requirements: requirements, TripPlan: planBuilderResult.TripPlan),
            JsonSerializerPrettyPrint);

        var result = (await agent.RunAsync<PlanReviewerFeedback>(
            prompt, 
            cancellationToken: cancellationToken)).Result;

        return result;
    }
}