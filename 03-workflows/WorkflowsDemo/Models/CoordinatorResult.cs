namespace WorkflowsDemo.Models;

internal sealed record CoordinatorResult(
    bool FinalPlanReady,
    bool PlanReadyForHumanReview,
    FinalTripPlan TripPlan
);
