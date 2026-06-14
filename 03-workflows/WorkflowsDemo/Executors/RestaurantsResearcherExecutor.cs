using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.RestaurantsResearch;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo.Executors;

internal sealed partial class RestaurantsResearcherExecutor(AIAgent agent)
    : Executor<RequirementsProcessedSignal, ResearchCompletedSignal>(nameof(RestaurantsResearcherExecutor))
{
    public override async ValueTask<ResearchCompletedSignal> HandleAsync(
        RequirementsProcessedSignal _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("RestaurantsResearcherExecutor runs, saves restaurants options to state");

        var fullRequirements = await context.ReadStateAsync<ProcessedTripRequirements>(
            key: ProcessedTripRequirementsStateKeyName,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken
        ) ?? throw new InvalidOperationException("ProcessedRequirements are missing from shared state");

        var relevantRequirements = new RestaurantsResearcherTripRequirements(
            Country: fullRequirements.Country,
            City: fullRequirements.City,
            TripBudgetUsd: fullRequirements.TripBudgetUsd,
            ArrivalDateTime: fullRequirements.TripArrivalDateTime,
            DepartureDateTime: fullRequirements.TripDepartureDateTime,
            NumberOfAdults: fullRequirements.NumberOfAdults,
            NumberOfChildren: fullRequirements.NumberOfChildren,
            AdditionalRequirements: fullRequirements.RequirementsUsefulForRestaurantsResearcherAgent
        );

        var allRestaurantsOptions = SearchRestaurants(
            fullRequirements.NumberOfAdults,
            fullRequirements.NumberOfChildren,
            fullRequirements.TripBudgetUsd);

        var prompt = JsonSerializer.Serialize(
            new RestaurantResearcherPrompt(relevantRequirements, allRestaurantsOptions),
            JsonSerializerPrettyPrint);
        
        var result = (await agent.RunAsync<List<RestaurantRankingItem>>(
            prompt,
            cancellationToken: cancellationToken)).Result;

        var bestOptions = result.Select(o =>
        {
            var fullItem = allRestaurantsOptions.First(
                r => r.RestaurantId.Equals(o.RestaurantId, StringComparison.OrdinalIgnoreCase));

            return new RestaurantOption(
                fullItem.RestaurantName,
                fullItem.Cuisine,
                fullItem.Type,
                fullItem.EstimatedTotalPriceAllTripMembersUsd,
                fullItem.AverageMealDurationHours,
                fullItem.City,
                fullItem.District,
                fullItem.Latitude,
                fullItem.Longitude,
                fullItem.OpeningHours,
                fullItem.DietaryOptions,
                fullItem.PopularDishes,
                fullItem.ReservationRecommended,
                fullItem.ReservationRequired,
                o.RankReasoning
            );
        }).ToList();

        await context.QueueStateUpdateAsync(
            key: RestaurantsOptionsStateKeyName,
            value: bestOptions,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        return new ResearchCompletedSignal();
    }

    private static List<Restaurant> SearchRestaurants(
        int numberOfAdults,
        int numberOfChildren,
        int tripBudget)
    {
        return Restaurants.Data
            .Select(r => r with
            {
                EstimatedTotalPriceAllTripMembersUsd =
                    r.AveragePricePerAdultUsd * numberOfAdults + r.AveragePricePerChildUsd * numberOfChildren
            })
            .Where(a => a.EstimatedTotalPriceAllTripMembersUsd < tripBudget)
            .ToList();
    }
}