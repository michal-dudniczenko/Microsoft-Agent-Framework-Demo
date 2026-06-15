namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record ScheduledPlanItem(
    PlanItemType Type,
    string Title,
    DateTime StartTime,
    DateTime EndTime,

    string? District,
    double? Latitude,
    double? Longitude,

    decimal EstimatedCostUsd,
    double DurationHours,

    string Description
);
