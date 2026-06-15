using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.AccommodationResearch;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo.Executors;

internal sealed partial class AccommodationResearcherExecutor(AIAgent agent)
    : Executor<RequirementsProcessedSignal, ResearchCompletedSignal>(nameof(AccommodationResearcherExecutor))
{
    public override async ValueTask<ResearchCompletedSignal> HandleAsync(
        RequirementsProcessedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("AccommodationResearcherExecutor runs, saves accommodation options to state");

        var fullRequirements = await context.ReadStateAsync<ProcessedTripRequirements>(
            key: ProcessedTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("ProcessedRequirements are missing from shared state");

        var relevantRequirements = new AccommodationResearcherTripRequirements(
            Country: fullRequirements.Country,
            City: fullRequirements.City,
            TripBudgetUsd: fullRequirements.TripBudgetUsd,
            TripArrivalDateTime: fullRequirements.TripArrivalDateTime,
            TripDepartureDateTime: fullRequirements.TripDepartureDateTime,
            NumberOfAdults: fullRequirements.NumberOfAdults,
            NumberOfChildren: fullRequirements.NumberOfChildren,
            AdditionalRequirements: fullRequirements.RequirementsUsefulForAccommodationResearcherAgent
        );

        var allAccommodationOptions = SearchAccommodations(
            fullRequirements.TripArrivalDateTime,
            fullRequirements.TripDepartureDateTime,
            fullRequirements.TripBudgetUsd);

        var prompt = JsonSerializer.Serialize(
            new AccommodationResearcherPrompt(relevantRequirements, allAccommodationOptions),
            JsonSerializerPrettyPrint);

        var result = (await agent.RunAsync<List<AccommodationRankingItem>>(
            prompt,
            cancellationToken: cancellationToken)).Result;

        var accommodationOptionsById = allAccommodationOptions.ToDictionary(
            a => a.AccommodationId,
            StringComparer.OrdinalIgnoreCase);

        var bestOptions = result.Select(o =>
        {
            if (!accommodationOptionsById.TryGetValue(o.AccommodationId, out var fullItem))
            {
                throw new InvalidOperationException(
                    $"Ranked accommodation id '{o.AccommodationId}' was not found in the source options. " +
                    $"Valid accommodation option count: {accommodationOptionsById.Count}.");
            }

            return new AccommodationOption(
                Name: fullItem.AccommodationName,
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
            key: AccommodationOptionsStateKeyName,
            value: bestOptions,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        return new ResearchCompletedSignal();
    }

    private static List<Accommodation> SearchAccommodations(
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
