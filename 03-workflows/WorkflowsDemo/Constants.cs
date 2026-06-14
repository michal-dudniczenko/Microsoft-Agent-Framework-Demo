using System.Text.Json;

namespace WorkflowsDemo;

internal static class Constants
{
    public static JsonSerializerOptions JsonSerializerPrettyPrint { get; } = new() { WriteIndented = true };

    public const string OpenTelemetrySourceName = "Workflow.Demo";
    public const string OpenTelemetryServiceName = "WorkflowDemo";
    public const string OpenTelemetryEndpoint = "<OTEL-ENDPOINT>";

    public const string ollamaModelName = "<OLLAMA-MODEL-NAME>";
    public const string ollamaUrl = "<OLLAMA-URL>";

    public const string openRouterModelName = "<OPEN-ROUTER-MODEL-NAME>";
    public const string openRouterApiKey = "<OPEN-ROUTER-API-KEY>";
    public const string openRouterOpenApiUrl = "https://openrouter.ai/api/v1";

    public const int TotalResearchersCount = 3;

    public const int MaxAgentReviewRounds = 1;
    public const int MaxHumanReviewRounds = 1;

    public const string SharedStateScopeName = "SharedScope";

    public const string AgentReviewerRoundNumberStateKeyName = "AgentReviewerRoundNumber";
    public const string HumanReviewRoundNumberStateKeyName = "HumanReviewRoundNumber";
    public const string ProcessedTripRequirementsStateKeyName = "ProcessedRequirements";
    public const string CurrentTripPlanStateKeyName = "CurrentTripPlan";

    public const string AccomodationOptionsStateKeyName = "AccomodationOptions";
    public const string AttractionsOptionsStateKeyName = "AttractionsOptions";
    public const string RestaurantsOptionsStateKeyName = "RestaurantsOptions";
}