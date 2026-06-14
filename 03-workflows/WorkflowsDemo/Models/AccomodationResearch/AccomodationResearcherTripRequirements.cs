namespace WorkflowsDemo.Models.AccomodationResearch;

internal sealed record AccomodationResearcherTripRequirements(
    string Country,
    string City,
    int TripBudgetUsd,
    DateTime ArrivalDateTime,
    DateTime DepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string[] AdditionalRequirements
);
