namespace WorkflowsDemo.Models.RestaurantsResearch;

internal sealed record Restaurant(
    string RestaurantId,
    string RestaurantName,
    string Cuisine,              // e.g., "Italian", "Japanese", "Local", "Vegan"
    string Type,                 // e.g., "Fine Dining", "Casual", "Cafe", "Street Food"

    string Description,
    IReadOnlyList<string> Tags,  // e.g., ["romantic", "family-friendly", "vegetarian", "late-night"]

    decimal EstimatedTotalPriceAllTripMembersUsd,
    decimal AveragePricePerAdultUsd,
    decimal AveragePricePerChildUsd,

    string PriceLevel,           // e.g., "$", "$$", "$$$", "$$$$"

    double AverageMealDurationHours,

    string City,
    string District,
    double Latitude,
    double Longitude,

    OpeningHours[] OpeningHours,

    double Rating,
    int ReviewCount,

    IReadOnlyList<string> DietaryOptions,      // e.g., ["vegetarian", "vegan", "gluten-free", "halal"]
    IReadOnlyList<string> PopularDishes,       // e.g., ["ramen", "pierogi", "seafood paella"]

    bool ReservationRecommended,
    bool ReservationRequired
);