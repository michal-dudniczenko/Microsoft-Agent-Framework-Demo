namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record TripDayPlan(
    DateOnly Date,
    string DayTheme, // e.g. "Old Town and museums"
    IReadOnlyList<ScheduledPlanItem> Items,
    string DaySummary
);
