namespace WorkflowsDemo.Models.RestaurantsResearch;

internal sealed record RestaurantOption(
    string Name,
    string Cuisine,              // e.g., "Italian", "Japanese", "Local", "Vegan"
    string Type,                 // e.g., "Fine Dining", "Casual", "Cafe", "Street Food"

    decimal EstimatedTotalPriceAllTripMembersUsd,

    double AverageMealDurationHours,

    string City,
    string District,
    double Latitude,
    double Longitude,

    OpeningHours[] OpeningHours,

    IReadOnlyList<string> DietaryOptions,      // e.g., ["vegetarian", "vegan", "gluten-free", "halal"]
    IReadOnlyList<string> PopularDishes,       // e.g., ["ramen", "pierogi", "seafood paella"]

    bool ReservationRecommended,
    bool ReservationRequired,

    string MatchExplanation
);
