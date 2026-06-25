using Anthropic;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using WorkflowsDemo.Agents;
using WorkflowsDemo.Events;
using WorkflowsDemo.Executors;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.PlanBuilder;
using WorkflowsDemo.Utils;
using static WorkflowsDemo.Config;

#region  OpenTelemetry Setup

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(OpenTelemetryServiceName);

using var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddConsole();
    logging.AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        options.SetResourceBuilder(resourceBuilder);
        options.AddOtlpExporter(opt => opt.Endpoint = new Uri(OpenTelemetryEndpoint));
    });
});

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource(OpenTelemetrySourceName)
    .AddOtlpExporter(opt => opt.Endpoint = new Uri(OpenTelemetryEndpoint))
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddMeter(OpenTelemetrySourceName)
    .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
    {
        exporterOptions.Endpoint = new Uri(OpenTelemetryEndpoint);
        metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000; // 5s
    })
    .Build();

#endregion

IConfiguration configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

// chat clients
var anthropicClient = new AnthropicClient()
{
    ApiKey = configuration[AnthropicApiKeyEnvVariableName]
};

var ollamaChatClient = OllamaChatClientFactory.Create();

// agents
var requirementsProcessorAgent = RequirementsProcessorAgent.GetAgent(ollamaChatClient);

var attractionsResearcherAgent = AttractionsResearcherAgent.GetAgent(anthropicClient);
var accommodationResearcherAgent = AccommodationResearcherAgent.GetAgent(anthropicClient);
var restaurantsResearcherAgent = RestaurantsResearcherAgent.GetAgent(anthropicClient);

var planBuilderAgent = PlanBuilderAgent.GetAgent(anthropicClient);
var planReviewerAgent = PlanReviewerAgent.GetAgent(anthropicClient);

// executors
var requirementsProcessorExecutor = new RequirementsProcessorExecutor(
    requirementsProcessorAgent,
    loggerFactory.CreateLogger<RequirementsProcessorExecutor>());

var accommodationResearcherExecutor = new AccommodationResearcherExecutor(
    accommodationResearcherAgent,
    loggerFactory.CreateLogger<AccommodationResearcherExecutor>());

var attractionsResearcherExecutor = new AttractionsResearcherExecutor(
    attractionsResearcherAgent,
    loggerFactory.CreateLogger<AttractionsResearcherExecutor>());

var restaurantsResearcherExecutor = new RestaurantsResearcherExecutor(
    restaurantsResearcherAgent,
    loggerFactory.CreateLogger<RestaurantsResearcherExecutor>());

var planBuilderExecutor = new PlanBuilderExecutor(
    planBuilderAgent,
    loggerFactory.CreateLogger<PlanBuilderExecutor>());

var planReviewerExecutor = new PlanReviewerExecutor(
    planReviewerAgent,
    loggerFactory.CreateLogger<PlanReviewerExecutor>());

var planRendererExecutor = new PlanRendererExecutor(
    loggerFactory.CreateLogger<PlanRendererExecutor>());

// building workflow
const string DemoWorkflowName = "DemoWorkflow";

var humanReviewPort = RequestPort.Create<PlanBuilderResult, HumanReviewFeedback>("HumanReview");

var workflow = new WorkflowBuilder(start: requirementsProcessorExecutor)

    .AddFanOutEdge(
        requirementsProcessorExecutor,
        targets: [
            accommodationResearcherExecutor,
            attractionsResearcherExecutor,
            restaurantsResearcherExecutor
        ])
    .AddFanInBarrierEdge(
        [
            accommodationResearcherExecutor,
            attractionsResearcherExecutor,
            restaurantsResearcherExecutor
        ],
        target: planBuilderExecutor
    )

    .AddEdge<PlanBuilderResult>(
        planBuilderExecutor,
        planReviewerExecutor,
        condition: result => result?.PlanReadyForHumanReview == false && result?.FinalPlanReady == false)

    .AddEdge(planReviewerExecutor, planBuilderExecutor)

    .AddEdge<PlanBuilderResult>(
        planBuilderExecutor,
        planRendererExecutor,
        condition: result => result?.FinalPlanReady == true || result?.PlanReadyForHumanReview == true)

    .AddEdge(planRendererExecutor, humanReviewPort)

    .AddEdge(humanReviewPort, planBuilderExecutor)

    .WithOutputFrom(requirementsProcessorExecutor, planRendererExecutor, planBuilderExecutor)
    .WithName(DemoWorkflowName)
    .WithOpenTelemetry(
        activitySource: new ActivitySource(OpenTelemetrySourceName),
        configure: c => c.EnableSensitiveData = true)
    .Build();

// export workflow as mermaid diagram
File.WriteAllText("workflow-diagram.md", "```mermaid\n" + workflow.ToMermaidString() + "\n```\n");

// ====================================================================================================================

var inputData = UserRequirements.Data;

var workflowLogger = loggerFactory.CreateLogger("WorkflowEvents");

StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, inputData);
await foreach (WorkflowEvent workflowEvent in run.WatchStreamAsync())
{
    switch (workflowEvent)
    {
        case WorkflowStartedEvent:
            workflowLogger.LogInformation("Workflow started");
            break;

        case WorkflowOutputEvent evt:
            if (evt.Data is TripNotPossibleSignal signal)
            {
                ColoredConsole.WriteLine(
                    "\nUnfortunately i am not able to come up with a plan that would satisfy your requirements."
                    + "\nExplanation: " + signal.Explanation + "\n",
                    ConsoleColor.Red);

                workflowLogger.LogInformation("Assessed user requirements as not feasible");
            }

            workflowLogger.LogInformation("Workflow completed");
            break;

        case WorkflowErrorEvent evt:
            workflowLogger.LogError(evt.Exception, "Workflow encountered an error");
            break;

        case WorkflowWarningEvent evt:
            workflowLogger.LogWarning("Workflow encountered a warning: {warningMessage}", evt);
            break;

        case ExecutorInvokedEvent evt:
            workflowLogger.LogInformation("Executor '{executorId}' runs", evt.ExecutorId);
            break;

        case ExecutorFailedEvent evt:
            workflowLogger.LogError(evt.Data, "Executor '{executorId}' encountered an error", evt.ExecutorId);
            break;

        case RequestInfoEvent requestInputEvt:
            workflowLogger.LogInformation("Feedback requested from the user");

            Console.WriteLine("\n=================================================================\n");

            ColoredConsole.Write(
                "Please review generated plan. Answer 'y' to approve or suggest changes: ",
                ConsoleColor.Cyan
            );

            var userResponse = Console.ReadLine()?.Trim()
                ?? throw new InvalidOperationException("Received NULL human feedback");

            Console.WriteLine("\n=================================================================\n");

            workflowLogger.LogInformation("Received feedback from user: {feedback}", userResponse);

            if (userResponse.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                await run.SendResponseAsync(requestInputEvt.Request.CreateResponse(new HumanReviewFeedback(
                    IsPlanApproved: true,
                    Details: string.Empty
                )));
            }
            else
            {
                await run.SendResponseAsync(requestInputEvt.Request.CreateResponse(new HumanReviewFeedback(
                    IsPlanApproved: false,
                    Details: userResponse
                )));
            }
            break;
    }
}
