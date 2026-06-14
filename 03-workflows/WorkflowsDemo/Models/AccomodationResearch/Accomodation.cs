namespace WorkflowsDemo.Models.AccomodationResearch;

internal sealed record Accommodation(
    string AccomodationId,
    string AccomodationName,
    string Type,

    decimal TotalStayPriceAllTripMembersUsd,
    decimal NightlyPriceUsd,

    double Rating,
    int ReviewCount,

    string City,
    string District,
    double Latitude,
    double Longitude,

    double DistanceToCityCenterKm,
    double DistanceToAirportKm,

    IReadOnlyList<string> Amenities,
    IReadOnlyList<string> ReviewHighlights,
    IReadOnlyList<string> ReviewComplaints,
    IReadOnlyList<string> NearbyAttractions,
    IReadOnlyList<string> NearbyRestaurants
);