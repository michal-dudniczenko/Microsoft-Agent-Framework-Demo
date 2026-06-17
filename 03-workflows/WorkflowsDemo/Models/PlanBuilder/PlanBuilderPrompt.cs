using WorkflowsDemo.Models.AccommodationResearch;
using WorkflowsDemo.Models.AttractionsResearch;
using WorkflowsDemo.Models.RestaurantsResearch;

namespace WorkflowsDemo.Models.PlanBuilder;

internal sealed record PlanBuilderPrompt(
    InitialUserTripRequirements Requirements,
    IReadOnlyList<AccommodationOption> AccommodationOption,
    IReadOnlyList<AttractionOption> AttractionOptions,
    IReadOnlyList<RestaurantOption> RestaurantOptions
);
