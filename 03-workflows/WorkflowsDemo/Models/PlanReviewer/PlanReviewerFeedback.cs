namespace WorkflowsDemo.Models.PlanReviewer;

internal sealed record PlanReviewerFeedback(
    bool ChangesSuggested,
    string Details
);
