namespace WorkflowsDemo.Models.AccomodationResearch;

internal sealed record AccommodationOption(
    string Name,
    string Type,

    decimal TotalStayPriceAllTripMembersUsd,
    decimal NightlyPriceUsd,

    string City,
    string District,
    double Latitude,
    double Longitude,

    double DistanceToCityCenterKm,
    double DistanceToAirportKm,

    IReadOnlyList<string> Amenities,
    IReadOnlyList<string> NearbyAttractions,
    IReadOnlyList<string> NearbyRestaurants,

    string MatchExplanation
);