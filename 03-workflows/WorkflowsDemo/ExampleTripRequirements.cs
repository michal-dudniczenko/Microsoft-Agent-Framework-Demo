using WorkflowsDemo.Models;

namespace WorkflowsDemo;

internal static class ExampleTripRequirements
{
    public static readonly InitialUserTripRequirements Requirements1 = new(
        Country: "Italy",
        City: "Rome",
        BudgetUsd: 1500,
        ArrivalTime: DateTime.Now,
        DepartureTime: DateTime.Now.AddDays(3),
        NumberOfAdults: 2,
        NumberOfChildren: 1,
        AdditionalUserRequirementsInNaturalLanguage: """
            We are traveling with a 7-year-old child.
            One adult uses a wheelchair.
            We would like to stay close to public transportation.
            We prefer quiet hotels rather than nightlife areas.
            One traveler has a severe peanut allergy.
            We enjoy museums, technology exhibits, gardens, and local culture.
            We do not want attractions that require extensive walking.
            We prefer family-friendly restaurants.
            We are comfortable using trains and metro systems.
            We would like to avoid very expensive restaurants.
        """
    );
}