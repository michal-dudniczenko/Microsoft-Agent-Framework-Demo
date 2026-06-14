namespace WorkflowsDemo.Models.AccomodationResearch;

internal sealed record AccomodationResearcherPrompt(
    AccomodationResearcherTripRequirements Requirements,
    IReadOnlyList<Accommodation> PossibleAccomodations
);
