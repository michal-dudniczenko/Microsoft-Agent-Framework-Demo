namespace WorkflowsDemo.Models.AttractionsResearch;

internal sealed record Attraction(
    string Id,
    string Name,
    string Category,            // e.g., "Museum", "Theme Park", "Park", "Historical"

    string Description,
    string[] Tags,               // e.g., ["indoor", "outdoor", "wheelchair-accessible", "romantic"]

    decimal TotalPriceAllTripMembersUsd,
    decimal PricePerAdultUsd,
    decimal PricePerChildUsd,

    double MinimumAge,
    double AverageDurationHours,

    string City,
    string District,
    double Latitude,
    double Longitude,

    OpeningHours[] OpeningHours,

    double Rating,
    int ReviewCount
);
