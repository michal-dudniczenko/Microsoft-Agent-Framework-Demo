namespace WorkflowsDemo.Models;

internal sealed record ProcessedTripRequirements(
    bool IsTripPossible,
    string TripNotPossibleExplanation,
    string Country,
    string City,
    int TripBudgetUsd,
    DateTime TripArrivalDateTime,
    DateTime TripDepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string AdditionalUserRequirementsInNaturalLanguage,
    string[] RequirementsUsefulForAttractionsResearcherAgent,
    string[] RequirementsUsefulForAccommodationResearcherAgent,
    string[] RequirementsUsefulForRestaurantsResearcherAgent
);
