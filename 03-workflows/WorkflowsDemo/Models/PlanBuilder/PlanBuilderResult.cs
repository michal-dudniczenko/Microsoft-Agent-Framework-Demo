namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record PlanBuilderResult(
    bool FinalPlanReady,
    bool PlanReadyForHumanReview,
    TripPlan TripPlan
);
