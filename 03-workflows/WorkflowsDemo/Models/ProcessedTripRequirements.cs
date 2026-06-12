namespace WorkflowsDemo.Models;

internal sealed record ProcessedTripRequirements(
    string Country,
    string City,
    int BudgetUsd,
    DateTime ArrivalTime,
    DateTime DepartureTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string AdditionalUserRequirementsInNaturalLanguage,
    string[] RequirementsUsefulForAttractionsResearcherAgent,
    string[] RequirementsUsefulForAccomodationResearcherAgent,
    string[] RequirementsUsefulForRestaurantsResearcherAgent,
    string[] RequirementsUsefulForTransportationResearcherAgent
);