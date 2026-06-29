namespace WorkflowsDemo;

internal static class Config
{
    public const string OpenTelemetrySourceName = "Workflows.Demo";
    public const string OpenTelemetryServiceName = "WorkflowsDemo";
    public const string OpenTelemetryEndpoint = "http://localhost:4317";

    public const string AnthropicApiKeyEnvVariableName = "AnthropicApiKey";
    public const string ClaudeSonnetModelName = "claude-sonnet-4-6";
    public const string ClaudeHaikuModelName = "claude-haiku-4-5";

    public const string OllamaModelName = "<OLLAMA-MODEL-NAME>";
    public const string OllamaOpenAIUrl = "<OLLAMA-URL>";

    public const int TotalResearchersCount = 3;

    public const int MaxAgentReviewRounds = 1;
    public const int MaxHumanReviewRounds = 2;

    public const string SharedStateScopeName = "SharedScope";

    public const string PlanReviewerRoundNumberStateKeyName = "PlanReviewerRoundNumber";
    public const string HumanReviewRoundNumberStateKeyName = "HumanReviewRoundNumber";
    public const string InitialTripRequirementsStateKeyName = "InitialRequirements";
    public const string ProcessedTripRequirementsStateKeyName = "ProcessedRequirements";
    public const string CurrentTripPlanStateKeyName = "CurrentTripPlan";

    public const string AccommodationOptionsStateKeyName = "AccommodationOptions";
    public const string AttractionsOptionsStateKeyName = "AttractionsOptions";
    public const string RestaurantsOptionsStateKeyName = "RestaurantsOptions";
}
