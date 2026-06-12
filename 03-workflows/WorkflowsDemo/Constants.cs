using System.Text.Json;

namespace WorkflowsDemo;

internal static class Constants
{
    public static JsonSerializerOptions JsonSerializerPrettyPrint { get; } = new() { WriteIndented = true };

    public const string OpenTelemetrySourceName = "Workflow.Demo";
    public const string OpenTelemetryServiceName = "WorkflowDemo";
    public const string OpenTelemetryEndpoint = "http://localhost:18889";

    public const string ollamaModelName = "granite4.1:3b";
    public const string ollamaUrl = "http://172.23.176.1:11434/v1";

    public const string openRouterModelName = "granite4.1:3b";
    public const string openRouterApiKey = "YOUR-API-KEY";
    public const string openRouterUrl = "https://openrouter.ai/api/v1";

    public const int TotalResearchersCount = 3;

    public const int MaxAgentReviewRounds = 3;
    public const int MaxHumanReviewRounds = 3;

    public const string SharedStateScopeName = "SharedScope";
    public const string AgentReviewerRoundNumberStateKeyName = "AgentReviewerRoundNumber";
    public const string HumanReviewRoundNumberStateKeyName = "HumanReviewRoundNumber";
    public const string ProcessedTripRequirementsStateKeyName = "ProcessedRequirements";
    public const string CurrentTripPlanStateKeyName = "CurrentTripPlan";
}