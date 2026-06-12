using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo.Executors;

internal sealed partial class CoordinatorExecutor(AIAgent agent)
    : Executor(nameof(CoordinatorExecutor))
{
    private int researchersCompletedCount = 0;

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<ResearchCompletedSignal>(HandleResearchCompletedAsync);
            routeBuilder.AddHandler<AgentReviewerFeedback, CoordinatorResult>(HandleAgentReviewerFeedbackAsync);
            routeBuilder.AddHandler<HumanReviewFeedback, CoordinatorResult>(HandleHumanFeedbackAsync);
        });

        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask HandleResearchCompletedAsync(
        ResearchCompletedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        researchersCompletedCount++;
        if (researchersCompletedCount < TotalResearchersCount)
        {
            return;
        }

        Console.WriteLine("CoordinatorExecutor runs, all researches completed, generating plan");

        var generatedPlan = new FinalTripPlan("todo");

        // save plan in shared state
        await context.QueueStateUpdateAsync(
            key: CurrentTripPlanStateKeyName,
            value: generatedPlan,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        // initialize review round numbers
        await context.QueueStateUpdateAsync(
            key: AgentReviewerRoundNumberStateKeyName,
            value: 1,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        await context.QueueStateUpdateAsync(
            key: HumanReviewRoundNumberStateKeyName,
            value: 1,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        await context.SendMessageAsync(new CoordinatorResult(
            FinalPlanReady: false,
            PlanReadyForHumanReview: false,
            TripPlan: generatedPlan), cancellationToken);
    }

    [MessageHandler]
    private async ValueTask<CoordinatorResult> HandleAgentReviewerFeedbackAsync(
        AgentReviewerFeedback feedback,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var tripPlan = await context.ReadStateAsync<FinalTripPlan>(
            key: CurrentTripPlanStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Trip plan instance is missing from shared state.");

        var currentAgentReviewRound = await context.ReadStateAsync<int>(
            key: AgentReviewerRoundNumberStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        if (currentAgentReviewRound == default)
            throw new InvalidOperationException("Agent review round number is missing from shared state");

        var currentHumanReviewRound = await context.ReadStateAsync<int>(
            key: HumanReviewRoundNumberStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        if (currentHumanReviewRound == default)
            throw new InvalidOperationException("Human review round number is missing from shared state");

        Console.WriteLine("CoordinatorExecutor runs, received AgentReviewerFeedback\n\t"
            + $"agent review round {currentAgentReviewRound}/{MaxAgentReviewRounds}\n\t"
            + $"human review round {currentHumanReviewRound - 1}/{MaxAgentReviewRounds}");

        if (feedback.ChangesSuggested)
        {
            Console.WriteLine("Reviewer suggested changes, iterating on plan");
            tripPlan = new FinalTripPlan("updated plan");

            // save updated plan in shared state
            await context.QueueStateUpdateAsync(
                key: CurrentTripPlanStateKeyName,
                value: tripPlan,
                scopeName: SharedStateScopeName,
                cancellationToken: cancellationToken
            );

            if (currentAgentReviewRound < MaxAgentReviewRounds)
            {
                Console.WriteLine("Sending updated plan for another review round");

                // increase agent review round number
                await context.QueueStateUpdateAsync(
                    key: AgentReviewerRoundNumberStateKeyName,
                    value: currentAgentReviewRound + 1,
                    scopeName: SharedStateScopeName,
                    cancellationToken: cancellationToken
                );

                return new CoordinatorResult(
                    FinalPlanReady: false,
                    PlanReadyForHumanReview: false,
                    TripPlan: tripPlan);
            }
        }

        // reset round number
        await context.QueueStateUpdateAsync(
            key: AgentReviewerRoundNumberStateKeyName,
            value: 1,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        // decide whether to send for human review or produce final plan

        if (currentHumanReviewRound <= MaxHumanReviewRounds)
        {
            Console.WriteLine("Sending plan for human review");

            return new CoordinatorResult(
                FinalPlanReady: false,
                PlanReadyForHumanReview: true,
                TripPlan: tripPlan);
        }

        Console.WriteLine("Reached maximum number of human review rounds, sending plan to plan generator");

        return new CoordinatorResult(
            FinalPlanReady: true,
            PlanReadyForHumanReview: false,
            TripPlan: tripPlan);
    }

    [MessageHandler]
    private async ValueTask<CoordinatorResult> HandleHumanFeedbackAsync(
        HumanReviewFeedback feedback,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        if (!feedback.IsPlanApproved && string.IsNullOrWhiteSpace(feedback.Details))
            throw new InvalidOperationException("Received invalid value from human feedback");

        var tripPlan = await context.ReadStateAsync<FinalTripPlan>(
            key: CurrentTripPlanStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Trip plan instance is missing from shared state.");

        var currentHumanReviewRound = await context.ReadStateAsync<int>(
            key: HumanReviewRoundNumberStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        if (currentHumanReviewRound == default)
            throw new InvalidOperationException("Human review round number is missing from shared state");

        if (feedback.IsPlanApproved)
        {
            Console.WriteLine("Human approved plan, sending plan to plan generator");

            return new CoordinatorResult(
                FinalPlanReady: true,
                PlanReadyForHumanReview: false,
                TripPlan: tripPlan);
        }

        Console.WriteLine("Received feedback from human reviewer, iterating on plan...");
        tripPlan = new FinalTripPlan("updated-after-human-review-todo");

        // save updated plan in shared state
        await context.QueueStateUpdateAsync(
            key: CurrentTripPlanStateKeyName,
            value: tripPlan,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        // increase human review round number
        await context.QueueStateUpdateAsync(
            key: HumanReviewRoundNumberStateKeyName,
            value: currentHumanReviewRound + 1,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        Console.WriteLine("Sending updated plan to agent reviewer");
        return new CoordinatorResult(
            FinalPlanReady: false,
            PlanReadyForHumanReview: false,
            TripPlan: tripPlan);
    }
}