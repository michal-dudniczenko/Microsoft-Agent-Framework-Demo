namespace WorkflowsDemo.Models.AccommodationResearch;

internal sealed record AccommodationResearcherPrompt(
    AccommodationResearcherTripRequirements Requirements,
    IReadOnlyList<Accommodation> PossibleAccommodations
);
