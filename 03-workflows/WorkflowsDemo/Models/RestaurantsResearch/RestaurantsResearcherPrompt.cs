namespace WorkflowsDemo.Models.RestaurantsResearch;

internal sealed record RestaurantsResearcherPrompt(
    RestaurantsResearcherTripRequirements Requirements,
    IReadOnlyList<Restaurant> PossibleRestaurants
);
