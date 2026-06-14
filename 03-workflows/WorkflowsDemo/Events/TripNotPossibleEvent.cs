using Microsoft.Agents.AI.Workflows;

namespace WorkflowsDemo.Events;

internal sealed class TripNotPossibleEvent(string explanation) : WorkflowEvent
{
    public string Explanation { get; init; } = explanation;
};
