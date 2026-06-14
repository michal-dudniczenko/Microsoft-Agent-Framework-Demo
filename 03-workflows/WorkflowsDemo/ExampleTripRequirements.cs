using WorkflowsDemo.Models;

namespace WorkflowsDemo;

internal static class ExampleTripRequirements
{
    public static readonly InitialUserTripRequirements Requirements1 = new(
        Country: "Italy",
        City: "Rome",
        TripBudgetUsd: 2500,
        ArrivalDateTime: DateTime.Now,
        DepartureDateTime: DateTime.Now.AddDays(3),
        NumberOfAdults: 2,
        NumberOfChildren: 1,
        AdditionalUserRequirementsInNaturalLanguage: """
            We are traveling with a 7-year-old child.
            We prefer quiet hotels rather than nightlife areas.
            One traveler has a severe peanut allergy.
            We enjoy museums, technology exhibits, gardens, and local culture.
            We do not want attractions that require extensive walking.
            We prefer family-friendly restaurants.
            We would like to avoid very expensive restaurants.
        """
    );
}