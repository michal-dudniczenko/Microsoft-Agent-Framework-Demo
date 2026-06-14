namespace WorkflowsDemo.Models;

internal sealed record HumanReviewFeedback(
    bool IsPlanApproved,
    string Details
);
