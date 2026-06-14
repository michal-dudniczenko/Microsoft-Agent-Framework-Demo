namespace WorkflowsDemo.Models.AttractionsResearch;

internal sealed record AttractionOption(
    string Name,
    string Category,        // e.g., "Museum", "Theme Park", "Park", "Historical"

    decimal TotalPriceAllTripMembersUsd,

    string City,
    string District,
    double Latitude,
    double Longitude,

    double MinimumAge,
    double AverageDurationHours,
    OpeningHours[] OpeningHours,

    string MatchExplanation
);
