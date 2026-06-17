using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using WorkflowsDemo.Events;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.AttractionsResearch;
using static WorkflowsDemo.Config;

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

        var fullRequirements = await context.ReadStateAsync<ProcessedTripRequirements>(
            key: ProcessedTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("ProcessedRequirements are missing from shared state");

        var relevantRequirements = new AttractionsResearcherTripRequirements(
            Country: fullRequirements.Country,
            City: fullRequirements.City,
            TripBudgetUsd: fullRequirements.TripBudgetUsd,
            TripArrivalDateTime: fullRequirements.TripArrivalDateTime,
            TripDepartureDateTime: fullRequirements.TripDepartureDateTime,
            NumberOfAdults: fullRequirements.NumberOfAdults,
            NumberOfChildren: fullRequirements.NumberOfChildren,
            AdditionalRequirements: fullRequirements.RequirementsUsefulForAttractionsResearcherAgent
        );

        var allAttractionsOptions = SearchAttractions(
            fullRequirements.NumberOfAdults,
            fullRequirements.NumberOfChildren,
            fullRequirements.TripBudgetUsd);

        var prompt = JsonSerializer.Serialize(
            new AttractionsResearcherPrompt(relevantRequirements, allAttractionsOptions));

        var result = (await agent.RunAsync<List<AttractionsRankingItem>>(
            prompt,
            cancellationToken: cancellationToken)).Result;

        var attractionOptionsById = allAttractionsOptions.ToDictionary(
            a => a.AttractionId,
            StringComparer.OrdinalIgnoreCase);

        var bestOptions = result.Select(o =>
        {
            if (!attractionOptionsById.TryGetValue(o.AttractionId, out var fullItem))
            {
                throw new InvalidOperationException(
                    $"Ranked attraction id '{o.AttractionId}' was not found in the source options. " +
                    $"Valid attraction option count: {attractionOptionsById.Count}.");
            }

            return new AttractionOption(
                Name: fullItem.AttractionName,
                Category: fullItem.Category,

                TotalPriceAllTripMembersUsd: fullItem.TotalPriceAllTripMembersUsd,

                City: fullItem.City,
                District: fullItem.District,
                Latitude: fullItem.Latitude,
                Longitude: fullItem.Longitude,

                MinimumAge: fullItem.MinimumAge,
                AverageDurationHours: fullItem.AverageDurationHours,
                OpeningHours: fullItem.OpeningHours,

                MatchExplanation: o.RankReasoning
            );
        }).ToList();

        await context.QueueStateUpdateAsync(
            key: AttractionsOptionsStateKeyName,
            value: bestOptions,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        return new ResearchCompletedSignal();
    }

    private static List<Attraction> SearchAttractions(
        int numberOfAdults,
        int numberOfChildren,
        int tripBudget)
    {
        var query = Attractions.Data
            .Select(a => a with
            {
                TotalPriceAllTripMembersUsd =
                    a.PricePerAdultUsd * numberOfAdults + a.PricePerChildUsd * numberOfChildren
            })
            .Where(a => a.TotalPriceAllTripMembersUsd < tripBudget);

        if (numberOfChildren > 0)
            query = query.Where(a => a.MinimumAge < 18);

        return query.ToList();
    }
}
