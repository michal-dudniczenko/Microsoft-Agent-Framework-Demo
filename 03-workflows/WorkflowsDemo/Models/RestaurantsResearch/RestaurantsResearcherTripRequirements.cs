namespace WorkflowsDemo.Models.RestaurantsResearch;

internal sealed record RestaurantsResearcherTripRequirements(
    string Country,
    string City,
    int TripBudgetUsd,
    DateTime ArrivalDateTime,
    DateTime DepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string[] AdditionalRequirements
);
