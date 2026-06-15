using WorkflowsDemo.Models.PlanBuilder;

namespace WorkflowsDemo.Models.PlanReviewer;

internal sealed record PlanReviewerPrompt(
    InitialUserTripRequirements Requirements,
    TripPlan TripPlan
);
