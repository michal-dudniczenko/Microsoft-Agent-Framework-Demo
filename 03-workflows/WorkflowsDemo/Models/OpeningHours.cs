namespace WorkflowsDemo.Models;

internal sealed record OpeningHours(
    string DayOfWeek,
    TimeSpan OpenTime,
    TimeSpan CloseTime
);
