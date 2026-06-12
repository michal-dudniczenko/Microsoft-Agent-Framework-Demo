namespace WorkflowsDemo.Models;

internal sealed record InitialUserTripRequirements(
    string Country,
    string City,
    int BudgetUsd,
    DateTime ArrivalTime,
    DateTime DepartureTime,
    int NumberOfAdults,
    int NumberOfChildren,
    string AdditionalUserRequirementsInNaturalLanguage
);