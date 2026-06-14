namespace WorkflowsDemo.Models.AttractionsResearch;

internal sealed record AttractionsResearcherPrompt(
    AttractionsResearcherTripRequirements Requirements,
    IReadOnlyList<Attraction> PossibleAttractions
);
