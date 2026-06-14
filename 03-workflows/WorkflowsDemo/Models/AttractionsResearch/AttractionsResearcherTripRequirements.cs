namespace WorkflowsDemo.Models.AttractionsResearch;

internal sealed record AttractionsResearcherTripRequirements(
    string Country,
    string City,
    int TripBudgetUsd,
    DateTime TripArrivalDateTime,
    DateTime TripDepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string[] AdditionalRequirements
);
