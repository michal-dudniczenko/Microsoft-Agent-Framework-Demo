namespace WorkflowsDemo.Models.AccommodationResearch;

internal sealed record AccommodationResearcherTripRequirements(
    string Country,
    string City,
    int TripBudgetUsd,
    DateTime TripArrivalDateTime,
    DateTime TripDepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string[] AdditionalRequirements
);
