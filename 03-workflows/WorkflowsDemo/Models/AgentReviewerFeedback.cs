namespace WorkflowsDemo.Models;

internal sealed record AgentReviewerFeedback(
    bool ChangesSuggested,
    string Details
);