using WorkflowsDemo.Models;
using WorkflowsDemo.Models.AttractionsResearch;
using WorkflowsDemo.Models.RestaurantsResearch;

namespace WorkflowsDemo.MockData;

internal static class Restaurants
{
    public static IReadOnlyList<Restaurant> Data { get; } =
    [
        new Restaurant(
            RestaurantId: "rome-rest-001",
            RestaurantName: "Da Francesco",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Traditional Roman trattoria near Piazza Navona known for thin-crust pizza and classic pasta dishes.",
            Tags: ["authentic", "local-favorite", "family-friendly", "pizza"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 35m,
            AveragePricePerChildUsd: 18m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.5,

            City: "Rome",
            District: "Centro Storico",
            Latitude: 41.8996,
            Longitude: 12.4724,

            OpeningHours:
            [
                new("Monday", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0)),
                new("Tuesday", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0)),
                new("Wednesday", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0)),
                new("Thursday", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0)),
                new("Friday", new TimeSpan(12, 0, 0), new TimeSpan(23, 30, 0)),
                new("Saturday", new TimeSpan(12, 0, 0), new TimeSpan(23, 30, 0)),
                new("Sunday", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.5,
            ReviewCount: 7600,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Carbonara", "Margherita Pizza", "Cacio e Pepe"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-002",
            RestaurantName: "Roscioli",
            Cuisine: "Italian",
            Type: "Fine Dining",

            Description: "Iconic Roman restaurant and delicatessen offering elevated regional cuisine and an exceptional wine list.",
            Tags: ["fine-dining", "wine", "gourmet", "romantic"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 75m,
            AveragePricePerChildUsd: 30m,

            PriceLevel: "$$$$",

            AverageMealDurationHours: 2.5,

            City: "Rome",
            District: "Campo de' Fiori",
            Latitude: 41.8948,
            Longitude: 12.4727,

            OpeningHours:
            [
                new("Monday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Tuesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Wednesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Thursday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Friday", new TimeSpan(12, 30, 0), new TimeSpan(23, 30, 0)),
                new("Saturday", new TimeSpan(12, 30, 0), new TimeSpan(23, 30, 0))
            ],

            Rating: 4.7,
            ReviewCount: 9100,

            DietaryOptions: ["vegetarian", "gluten-free"],
            PopularDishes: ["Carbonara", "Burrata", "Amatriciana"],

            ReservationRecommended: true,
            ReservationRequired: true
        ),

        new Restaurant(
            RestaurantId: "rome-rest-003",
            RestaurantName: "Ai Tre Scalini",
            Cuisine: "Italian",
            Type: "Wine Bar",

            Description: "Historic Monti wine bar serving Roman specialties, cured meats and regional wines.",
            Tags: ["wine-bar", "historic", "local-favorite", "evening"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 30m,
            AveragePricePerChildUsd: 15m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.75,

            City: "Rome",
            District: "Monti",
            Latitude: 41.8955,
            Longitude: 12.4947,

            OpeningHours:
            [
                new("Daily", new TimeSpan(12, 0, 0), new TimeSpan(0, 0, 0))
            ],

            Rating: 4.4,
            ReviewCount: 4200,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Tagliere", "Roman Meatballs", "Bruschetta"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-004",
            RestaurantName: "Armando al Pantheon",
            Cuisine: "Italian",
            Type: "Fine Dining",

            Description: "Legendary family-run Roman restaurant steps from the Pantheon.",
            Tags: ["iconic", "traditional", "authentic", "historic"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 55m,
            AveragePricePerChildUsd: 25m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 2.0,

            City: "Rome",
            District: "Pantheon",
            Latitude: 41.8986,
            Longitude: 12.4767,

            OpeningHours:
            [
                new("Monday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Tuesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Wednesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Thursday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Friday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Saturday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.8,
            ReviewCount: 5100,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Carbonara", "Saltimbocca", "Roman Artichokes"],

            ReservationRecommended: true,
            ReservationRequired: true
        ),

        new Restaurant(
            RestaurantId: "rome-rest-005",
            RestaurantName: "Tonnarello",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Highly popular Trastevere restaurant famous for Roman pasta dishes.",
            Tags: ["tourist-favorite", "family-friendly", "pasta", "busy"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 32m,
            AveragePricePerChildUsd: 16m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.5,

            City: "Rome",
            District: "Trastevere",
            Latitude: 41.8895,
            Longitude: 12.4684,

            OpeningHours:
            [
                new("Daily", new TimeSpan(11, 0, 0), new TimeSpan(23, 30, 0))
            ],

            Rating: 4.6,
            ReviewCount: 65000,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Cacio e Pepe", "Carbonara", "Amatriciana"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-006",
            RestaurantName: "Osteria der Belli",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Sardinian-influenced trattoria in Trastevere known for seafood dishes.",
            Tags: ["seafood", "authentic", "local", "sardinian"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 42m,
            AveragePricePerChildUsd: 20m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 2.0,

            City: "Rome",
            District: "Trastevere",
            Latitude: 41.8885,
            Longitude: 12.4678,

            OpeningHours:
            [
                new("Tuesday", new TimeSpan(19, 0, 0), new TimeSpan(23, 0, 0)),
                new("Wednesday", new TimeSpan(19, 0, 0), new TimeSpan(23, 0, 0)),
                new("Thursday", new TimeSpan(19, 0, 0), new TimeSpan(23, 0, 0)),
                new("Friday", new TimeSpan(19, 0, 0), new TimeSpan(23, 0, 0)),
                new("Saturday", new TimeSpan(19, 0, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.6,
            ReviewCount: 2100,

            DietaryOptions: ["gluten-free"],
            PopularDishes: ["Seafood Pasta", "Octopus", "Bottarga"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-007",
            RestaurantName: "Nannarella",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Lively Trastevere restaurant serving Roman comfort food in generous portions.",
            Tags: ["family-friendly", "popular", "traditional", "pasta"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 30m,
            AveragePricePerChildUsd: 15m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.5,

            City: "Rome",
            District: "Trastevere",
            Latitude: 41.8889,
            Longitude: 12.4681,

            OpeningHours:
            [
                new("Daily", new TimeSpan(12, 0, 0), new TimeSpan(23, 30, 0))
            ],

            Rating: 4.5,
            ReviewCount: 24000,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Carbonara", "Lasagna", "Amatriciana"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-008",
            RestaurantName: "La Taverna dei Fori Imperiali",
            Cuisine: "Italian",
            Type: "Fine Dining",

            Description: "Refined Roman cuisine near the Colosseum with excellent service.",
            Tags: ["romantic", "historic-center", "traditional", "refined"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 50m,
            AveragePricePerChildUsd: 24m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 2.0,

            City: "Rome",
            District: "Monti",
            Latitude: 41.8929,
            Longitude: 12.4914,

            OpeningHours:
            [
                new("Daily", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.7,
            ReviewCount: 5200,

            DietaryOptions: ["vegetarian", "gluten-free"],
            PopularDishes: ["Carbonara", "Osso Buco", "Tiramisu"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-009",
            RestaurantName: "Fatamorgana",
            Cuisine: "Dessert",
            Type: "Cafe",

            Description: "Artisanal gelateria famous for creative flavors and natural ingredients.",
            Tags: ["gelato", "dessert", "family-friendly", "quick-stop"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 8m,
            AveragePricePerChildUsd: 5m,

            PriceLevel: "$",

            AverageMealDurationHours: 0.3,

            City: "Rome",
            District: "Prati",
            Latitude: 41.9084,
            Longitude: 12.4589,

            OpeningHours:
            [
                new("Daily", new TimeSpan(11, 0, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.7,
            ReviewCount: 3900,

            DietaryOptions: ["vegetarian", "gluten-free"],
            PopularDishes: ["Pistachio Gelato", "Mango Gelato", "Chocolate Sorbet"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-010",
            RestaurantName: "Trattoria Monti",
            Cuisine: "Italian",
            Type: "Fine Dining",

            Description: "Beloved family-run restaurant blending Roman and Marche regional cuisine.",
            Tags: ["local-favorite", "gourmet", "traditional", "romantic"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 55m,
            AveragePricePerChildUsd: 25m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 2.0,

            City: "Rome",
            District: "Monti",
            Latitude: 41.8963,
            Longitude: 12.5008,

            OpeningHours:
            [
                new("Tuesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Wednesday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Thursday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Friday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0)),
                new("Saturday", new TimeSpan(12, 30, 0), new TimeSpan(23, 0, 0))
            ],

            Rating: 4.8,
            ReviewCount: 2800,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Ravioli", "Duck Ragu", "Pollo alla Cacciatora"],

            ReservationRecommended: true,
            ReservationRequired: true
        ),

        new Restaurant(
            RestaurantId: "rome-rest-011",
            RestaurantName: "Mercato Centrale",
            Cuisine: "International",
            Type: "Food Hall",

            Description: "Large gourmet food market featuring numerous local and international vendors.",
            Tags: ["food-market", "family-friendly", "casual", "variety"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 25m,
            AveragePricePerChildUsd: 12m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.25,

            City: "Rome",
            District: "Esquilino",
            Latitude: 41.9012,
            Longitude: 12.5018,

            OpeningHours: [ new("Daily", new TimeSpan(8, 0, 0), new TimeSpan(0, 0, 0)) ],

            Rating: 4.5,
            ReviewCount: 17000,

            DietaryOptions: ["vegetarian", "vegan", "gluten-free"],
            PopularDishes: ["Pizza", "Pasta", "Gelato"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-012",
            RestaurantName: "Ristorante Coreano Hana",
            Cuisine: "Korean",
            Type: "Casual",

            Description: "Authentic Korean restaurant serving barbecue, bibimbap and traditional dishes.",
            Tags: ["asian", "korean", "family-friendly", "casual"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 32m,
            AveragePricePerChildUsd: 15m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.75,

            City: "Rome",
            District: "Esquilino",
            Latitude: 41.8978,
            Longitude: 12.5031,

            OpeningHours: [ new("Daily", new TimeSpan(12, 0, 0), new TimeSpan(22, 30, 0)) ],

            Rating: 4.4,
            ReviewCount: 1400,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Bibimbap", "Bulgogi", "Kimchi Fried Rice"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-013",
            RestaurantName: "Pizzarium",
            Cuisine: "Italian",
            Type: "Street Food",

            Description: "Famous pizza al taglio destination created by master pizzaiolo Gabriele Bonci.",
            Tags: ["pizza", "street-food", "quick-bite", "iconic"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 16m,
            AveragePricePerChildUsd: 8m,

            PriceLevel: "$",

            AverageMealDurationHours: 0.75,

            City: "Rome",
            District: "Prati",
            Latitude: 41.9073,
            Longitude: 12.4465,

            OpeningHours: [ new("Daily", new TimeSpan(11, 0, 0), new TimeSpan(22, 0, 0)) ],

            Rating: 4.6,
            ReviewCount: 11000,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Pizza al Taglio", "Potato Pizza", "Mortadella Pizza"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-014",
            RestaurantName: "La Zanzara",
            Cuisine: "Italian",
            Type: "Bistro",

            Description: "Modern all-day restaurant popular for brunch, cocktails and contemporary Italian dishes.",
            Tags: ["brunch", "modern", "cocktails", "stylish"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 38m,
            AveragePricePerChildUsd: 18m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 1.75,

            City: "Rome",
            District: "Prati",
            Latitude: 41.9087,
            Longitude: 12.4598,

            OpeningHours: [ new("Daily", new TimeSpan(7, 0, 0), new TimeSpan(1, 0, 0)) ],

            Rating: 4.4,
            ReviewCount: 7300,

            DietaryOptions: ["vegetarian", "vegan"],
            PopularDishes: ["Eggs Benedict", "Carbonara", "Tuna Tartare"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-015",
            RestaurantName: "Il Sorpasso",
            Cuisine: "Italian",
            Type: "Wine Bar",

            Description: "Popular Prati wine bar serving creative small plates and regional wines.",
            Tags: ["wine", "small-plates", "local", "evening"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 36m,
            AveragePricePerChildUsd: 16m,

            PriceLevel: "$$$",

            AverageMealDurationHours: 1.75,

            City: "Rome",
            District: "Prati",
            Latitude: 41.9080,
            Longitude: 12.4585,

            OpeningHours: [ new("Daily", new TimeSpan(12, 0, 0), new TimeSpan(23, 30, 0)) ],

            Rating: 4.6,
            ReviewCount: 3100,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Charcuterie Board", "Burrata", "Wine Pairings"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-016",
            RestaurantName: "Apuleius",
            Cuisine: "Italian",
            Type: "Fine Dining",

            Description: "Elegant rooftop restaurant with panoramic city views and contemporary cuisine.",
            Tags: ["rooftop", "romantic", "views", "fine-dining"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 80m,
            AveragePricePerChildUsd: 30m,

            PriceLevel: "$$$$",

            AverageMealDurationHours: 2.5,

            City: "Rome",
            District: "Monti",
            Latitude: 41.8968,
            Longitude: 12.4957,

            OpeningHours: [ new("Daily", new TimeSpan(18, 30, 0), new TimeSpan(23, 0, 0)) ],

            Rating: 4.6,
            ReviewCount: 900,

            DietaryOptions: ["vegetarian", "vegan", "gluten-free"],
            PopularDishes: ["Tasting Menu", "Sea Bass", "Risotto"],

            ReservationRecommended: true,
            ReservationRequired: true
        ),

        new Restaurant(
            RestaurantId: "rome-rest-017",
            RestaurantName: "Panna & Co",
            Cuisine: "Dessert",
            Type: "Cafe",

            Description: "Specialty dessert shop known for artisanal tiramisu and sweet treats.",
            Tags: ["dessert", "tiramisu", "cafe", "quick-stop"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 9m,
            AveragePricePerChildUsd: 5m,

            PriceLevel: "$",

            AverageMealDurationHours: 0.5,

            City: "Rome",
            District: "Trastevere",
            Latitude: 41.8880,
            Longitude: 12.4687,

            OpeningHours: [ new("Daily", new TimeSpan(10, 0, 0), new TimeSpan(22, 0, 0)) ],

            Rating: 4.7,
            ReviewCount: 2600,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Tiramisu", "Panna Cotta", "Cannoli"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-018",
            RestaurantName: "Rosso",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Contemporary pizzeria and restaurant offering Roman classics and craft beers.",
            Tags: ["pizza", "casual", "modern", "groups"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 28m,
            AveragePricePerChildUsd: 14m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.5,

            City: "Rome",
            District: "San Lorenzo",
            Latitude: 41.9005,
            Longitude: 12.5147,

            OpeningHours: [ new("Daily", new TimeSpan(18, 0, 0), new TimeSpan(0, 0, 0)) ],

            Rating: 4.3,
            ReviewCount: 1900,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Roman Pizza", "Suppli", "Craft Beer"],

            ReservationRecommended: false,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-019",
            RestaurantName: "Mamma Angela",
            Cuisine: "Italian",
            Type: "Casual",

            Description: "Welcoming family restaurant near Termini serving regional Italian cuisine.",
            Tags: ["family-friendly", "local", "traditional", "value"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 30m,
            AveragePricePerChildUsd: 15m,

            PriceLevel: "$$",

            AverageMealDurationHours: 1.5,

            City: "Rome",
            District: "Castro Pretorio",
            Latitude: 41.9043,
            Longitude: 12.5038,

            OpeningHours: [ new("Daily", new TimeSpan(12, 0, 0), new TimeSpan(23, 0, 0)) ],

            Rating: 4.5,
            ReviewCount: 6200,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Carbonara", "Lasagna", "Tiramisu"],

            ReservationRecommended: true,
            ReservationRequired: false
        ),

        new Restaurant(
            RestaurantId: "rome-rest-020",
            RestaurantName: "Pinsere",
            Cuisine: "Italian",
            Type: "Street Food",

            Description: "Popular takeaway spot specializing in high-quality Roman pinsa.",
            Tags: ["pinsa", "quick-bite", "street-food", "budget-friendly"],

            EstimatedTotalPriceAllTripMembersUsd: 0,
            AveragePricePerAdultUsd: 14m,
            AveragePricePerChildUsd: 7m,

            PriceLevel: "$",

            AverageMealDurationHours: 0.75,

            City: "Rome",
            District: "Sallustiano",
            Latitude: 41.9060,
            Longitude: 12.4959,

            OpeningHours: [ new("Daily", new TimeSpan(11, 0, 0), new TimeSpan(22, 0, 0)) ],

            Rating: 4.7,
            ReviewCount: 8500,

            DietaryOptions: ["vegetarian"],
            PopularDishes: ["Margherita Pinsa", "Prosciutto Pinsa", "Vegetarian Pinsa"],

            ReservationRecommended: false,
            ReservationRequired: false
        )
    ];
}