namespace WorkflowsDemo.Models.RestaurantsResearch;

internal sealed record RestaurantResearcherPrompt(
    RestaurantsResearcherTripRequirements Requirements,
    IReadOnlyList<Restaurant> PossibleRestaurants
);
