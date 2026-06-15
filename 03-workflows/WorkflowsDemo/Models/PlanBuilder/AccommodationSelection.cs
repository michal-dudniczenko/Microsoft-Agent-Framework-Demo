namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record AccommodationSelection(
    string Name,
    string Type,
    decimal TotalStayPriceUsd,
    string District,
    double Latitude,
    double Longitude,
    IReadOnlyList<string> KeyAmenities,
    IReadOnlyList<string> WhySelected
);
