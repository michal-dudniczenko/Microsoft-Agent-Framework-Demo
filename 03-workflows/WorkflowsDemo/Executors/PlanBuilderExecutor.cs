using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.AccommodationResearch;
using WorkflowsDemo.Models.AttractionsResearch;
using WorkflowsDemo.Models.PlanBuilder;
using WorkflowsDemo.Models.PlanReviewer;
using WorkflowsDemo.Models.RestaurantsResearch;
using WorkflowsDemo.Utils;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Executors;

internal sealed partial class PlanBuilderExecutor(
    AIAgent agent,
    ILogger<PlanBuilderExecutor> logger)
    : Executor(nameof(PlanBuilderExecutor))
{
    private AgentSession? agentSession;
    private int researchersCompletedCount = 0;

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.SendsMessage<PlanBuilderResult>();
        protocolBuilder.YieldsOutput<WorkflowCompletedSignal>();
        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<ResearchCompletedSignal>(HandleResearchCompletedAsync);
            routeBuilder.AddHandler<PlanReviewerFeedback>(HandlePlanReviewerFeedbackAsync);
            routeBuilder.AddHandler<HumanReviewFeedback>(HandleHumanFeedbackAsync);
        });

        return protocolBuilder;
    }

    private async ValueTask HandleResearchCompletedAsync(
        ResearchCompletedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        researchersCompletedCount++;
        if (researchersCompletedCount < TotalResearchersCount)
        {
            logger.LogInformation(
                "Waiting for all researchers to complete. Remaining: {remaining}/{allResearhers}",
                TotalResearchersCount - researchersCompletedCount, TotalResearchersCount
            );
            return;
        }

        var userRequirements = await context.ReadStateAsync<InitialUserTripRequirements>(
            key: InitialTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("User requirements are missing from state.");

        var accommodationOptions = await context.ReadStateAsync<List<AccommodationOption>>(
            key: AccommodationOptionsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Accommodation options are missing from state.");

        if (accommodationOptions.Count == 0)
            throw new InvalidOperationException("Empty list of accommodation options.");

        var attractionsOptions = await context.ReadStateAsync<List<AttractionOption>>(
            key: AttractionsOptionsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Attractions options are missing from state.");

        if (attractionsOptions.Count == 0)
            throw new InvalidOperationException("Empty list of attractions options.");

        var restaurantOptions = await context.ReadStateAsync<List<RestaurantOption>>(
            key: RestaurantsOptionsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Restaurant options are missing from state.");

        if (restaurantOptions.Count == 0)
            throw new InvalidOperationException("Empty list of restaurants options.");

        var prompt = JsonSerializer.Serialize(
            new PlanBuilderPrompt(
                Requirements: userRequirements,
                AccommodationOption: accommodationOptions,
                AttractionOptions: attractionsOptions,
                RestaurantOptions: restaurantOptions));

        agentSession ??= await agent.CreateSessionAsync(cancellationToken);
        var result = (await agent.RunAsync<TripPlan>(
            prompt,
            agentSession,
            cancellationToken: cancellationToken)).Result;
        
        result.SaveModelResponse(fileName: agent.Id, insertSeparator: true);

        // save plan in shared state
        await context.QueueStateUpdateAsync(
            key: CurrentTripPlanStateKeyName,
            value: result,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        // initialize review round numbers
        await context.QueueStateUpdateAsync(
            key: PlanReviewerRoundNumberStateKeyName,
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

        logger.LogInformation("Generated trip itinerary, passing it to the agent reviewer");

        await context.SendMessageAsync(new PlanBuilderResult(
            FinalPlanReady: false,
            PlanReadyForHumanReview: false,
            TripPlan: result), cancellationToken);
    }

    private async ValueTask HandlePlanReviewerFeedbackAsync(
        PlanReviewerFeedback feedback,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var tripPlan = await context.ReadStateAsync<TripPlan>(
            key: CurrentTripPlanStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("Trip plan instance is missing from shared state.");

        var currentAgentReviewRound = await context.ReadStateAsync<int>(
            key: PlanReviewerRoundNumberStateKeyName,
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

        logger.LogInformation(
            "Received feedback from agent reviewer. Agent review round {AgentReviewRound}/{MaxAgentReviewRounds}. "
            + "Human review round {HumanReviewRound}/{MaxHumanReviewRounds}",
            currentAgentReviewRound,
            MaxAgentReviewRounds,
            currentHumanReviewRound - 1,
            MaxHumanReviewRounds);

        if (feedback.ChangesSuggested)
        {
            logger.LogInformation("Reviewer suggested changes, iterating on plan");

            var prompt = GetReviewerFeedbackPrompt(feedback.Details);

            agentSession ??= await agent.CreateSessionAsync(cancellationToken);
            tripPlan = (await agent.RunAsync<TripPlan>(
                prompt,
                agentSession,
                cancellationToken: cancellationToken)).Result;
            
            tripPlan.SaveModelResponse(fileName: agent.Id, insertSeparator: true);

            // save updated plan in shared state
            await context.QueueStateUpdateAsync(
                key: CurrentTripPlanStateKeyName,
                value: tripPlan,
                scopeName: SharedStateScopeName,
                cancellationToken: cancellationToken
            );

            if (currentAgentReviewRound < MaxAgentReviewRounds)
            {
                logger.LogInformation("Sending updated plan for another review round");

                // increase agent review round number
                await context.QueueStateUpdateAsync(
                    key: PlanReviewerRoundNumberStateKeyName,
                    value: currentAgentReviewRound + 1,
                    scopeName: SharedStateScopeName,
                    cancellationToken: cancellationToken
                );

                await context.SendMessageAsync(new PlanBuilderResult(
                    FinalPlanReady: false,
                    PlanReadyForHumanReview: false,
                    TripPlan: tripPlan), cancellationToken);

                return;
            }
        }
        else
        {
            logger.LogInformation("Reviewer approved plan");
        }

        // reset round number
        await context.QueueStateUpdateAsync(
            key: PlanReviewerRoundNumberStateKeyName,
            value: 1,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        );

        // decide whether to send for human review or produce final plan

        if (currentHumanReviewRound <= MaxHumanReviewRounds)
        {
            logger.LogInformation("Sending plan for human review");

            await context.SendMessageAsync(new PlanBuilderResult(
                FinalPlanReady: false,
                PlanReadyForHumanReview: true,
                TripPlan: tripPlan), cancellationToken);

            return;
        }

        logger.LogInformation("Reached maximum number of human review rounds, sending plan to the plan renderer");

        await context.SendMessageAsync(new PlanBuilderResult(
            FinalPlanReady: true,
            PlanReadyForHumanReview: false,
            TripPlan: tripPlan), cancellationToken);

        return;
    }

    private async ValueTask HandleHumanFeedbackAsync(
        HumanReviewFeedback feedback,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        if (!feedback.IsPlanApproved && string.IsNullOrWhiteSpace(feedback.Details))
            throw new InvalidOperationException("Received invalid value from human feedback");

        var currentHumanReviewRound = await context.ReadStateAsync<int>(
            key: HumanReviewRoundNumberStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        if (currentHumanReviewRound == default)
            throw new InvalidOperationException("Human review round number is missing from shared state");

        if (feedback.IsPlanApproved)
        {
            logger.LogInformation("Human approved plan");

            await context.YieldOutputAsync(new WorkflowCompletedSignal(), cancellationToken);
            return;
        }

        logger.LogInformation("Received feedback from human reviewer, iterating on plan");

        agentSession ??= await agent.CreateSessionAsync(cancellationToken);
        var result = (await agent.RunAsync<TripPlan>(
            GetHumanFeedbackPrompt(feedback.Details),
            agentSession,
            cancellationToken: cancellationToken)).Result;
        
        result.SaveModelResponse(fileName: agent.Id, insertSeparator: true);

        // save updated plan in shared state
        await context.QueueStateUpdateAsync(
            key: CurrentTripPlanStateKeyName,
            value: result,
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

        logger.LogInformation("Sending updated plan to agent reviewer");

        await context.SendMessageAsync(
            new PlanBuilderResult(
                FinalPlanReady: false,
                PlanReadyForHumanReview: false,
                TripPlan: result),
            cancellationToken);
    }

    private static string GetReviewerFeedbackPrompt(string feedback) => $"""
        The Reviewer Agent has evaluated your previously generated trip plan and suggested the following corrections:

        {feedback}

        Regenerate the trip plan, strictly observing these execution rules during your revision:

        1. Fix the issues noted above by re-evaluating the original candidate data arrays (Accommodations, Attractions, Restaurants). Do not invent or enrich any data.
        2. Maintain absolute compliance with all original rules: strict trip boundaries, chronological sorting, standard meal windows, and age/reservation warnings.
        3. Your output must be exactly one valid JSON object matching the required PascalCase schema. Do not include markdown formatting, code fences (such as ```json), or any conversational text before or after the JSON.
    """.Replace("    ", string.Empty);

    private static string GetHumanFeedbackPrompt(string feedback) => $"""
        The human reviewer has evaluated your previously generated trip plan and issued the following mandatory corrections:

        {feedback}

        Regenerate the trip plan, strictly observing these execution rules during your revision:

        1. Fix the issues noted above by re-evaluating the original candidate data arrays (Accommodations, Attractions, Restaurants). Do not invent or enrich any data.
        2. Maintain absolute compliance with all original rules: strict trip boundaries, chronological sorting, standard meal windows, and age/reservation warnings.
        3. Your output must be exactly one valid JSON object matching the required PascalCase schema. Do not include markdown formatting, code fences (such as ```json), or any conversational text before or after the JSON.
        4. If the suggested fix is impossible to apply, add a note and explanation to the `OverallExplanation` field content.
    """.Replace("    ", string.Empty);
}
