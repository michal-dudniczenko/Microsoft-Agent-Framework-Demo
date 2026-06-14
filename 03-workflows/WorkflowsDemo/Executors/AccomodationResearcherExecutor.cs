using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.AccomodationResearch;
using static WorkflowsDemo.Constants;

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

        var fullRequirements = await context.ReadStateAsync<ProcessedTripRequirements>(
            key: ProcessedTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("ProcessedRequirements are missing from shared state");

        var relevantRequirements = new AccomodationResearcherTripRequirements(
            Country: fullRequirements.Country,
            City: fullRequirements.City,
            TripBudgetUsd: fullRequirements.TripBudgetUsd,
            ArrivalDateTime: fullRequirements.TripArrivalDateTime,
            DepartureDateTime: fullRequirements.TripDepartureDateTime,
            NumberOfAdults: fullRequirements.NumberOfAdults,
            NumberOfChildren: fullRequirements.NumberOfChildren,
            AdditionalRequirements: fullRequirements.RequirementsUsefulForAccomodationResearcherAgent
        );

        var allAccomodationOptions = SearchAccomodations(
            fullRequirements.TripArrivalDateTime,
            fullRequirements.TripDepartureDateTime,
            fullRequirements.TripBudgetUsd);

        var prompt = JsonSerializer.Serialize(
            new AccomodationResearcherPrompt(relevantRequirements, allAccomodationOptions),
            JsonSerializerPrettyPrint);

        var result = (await agent.RunAsync<List<AccomodationRankingItem>>(
            prompt,
            cancellationToken: cancellationToken)).Result;

        var bestOptions = result.Select(o =>
        {
            var fullItem = allAccomodationOptions.First(
                a => a.AccomodationId.Equals(o.AccomodationId, StringComparison.OrdinalIgnoreCase));

            return new AccommodationOption(
                Name: fullItem.AccomodationName,
                Type: fullItem.Type,

                TotalStayPriceAllTripMembersUsd: fullItem.TotalStayPriceAllTripMembersUsd,
                NightlyPriceUsd: fullItem.NightlyPriceUsd,

                City: fullItem.City,
                District: fullItem.District,
                Latitude: fullItem.Latitude,
                Longitude: fullItem.Longitude,

                DistanceToCityCenterKm: fullItem.DistanceToCityCenterKm,
                DistanceToAirportKm: fullItem.DistanceToAirportKm,

                Amenities: fullItem.Amenities,
                NearbyAttractions: fullItem.NearbyAttractions,
                NearbyRestaurants: fullItem.NearbyRestaurants,

                MatchExplanation: o.RankReasoning
            );
        }).ToList();

        await context.QueueStateUpdateAsync(
            key: AccomodationOptionsStateKeyName,
            value: bestOptions,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        return new ResearchCompletedSignal();
    }

    private static List<Accommodation> SearchAccomodations(
        DateTime checkInDate,
        DateTime checkOutDate,
        int tripBudget)
    {
        var nightsCount = (checkOutDate - checkInDate).Days;

        return Accommodations.Data
            .Select(a => a with { TotalStayPriceAllTripMembersUsd = a.NightlyPriceUsd * nightsCount })
            .Where(a => a.TotalStayPriceAllTripMembersUsd < tripBudget)
            .ToList();
    }
}