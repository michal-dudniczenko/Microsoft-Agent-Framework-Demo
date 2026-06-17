namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record TripPlanSummary(
    string Destination,
    DateTime ArrivalDateTime,
    DateTime DepartureDateTime,
    int NumberOfAdults,
    int NumberOfChildren,
    decimal TotalEstimatedCostUsd,
    decimal BudgetUsd,
    decimal RemainingBudgetUsd,
    string TripStyle, // e.g. "family-friendly", "budget", "food-focused"
    string ShortDescription
);
