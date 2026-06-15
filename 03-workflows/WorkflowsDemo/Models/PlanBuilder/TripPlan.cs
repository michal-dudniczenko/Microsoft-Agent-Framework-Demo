namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record TripPlan(
    TripPlanSummary Summary,
    AccommodationSelection Accommodation,
    IReadOnlyList<TripDayPlan> Days,
    IReadOnlyList<string> Warnings,
    string OverallExplanation
);
