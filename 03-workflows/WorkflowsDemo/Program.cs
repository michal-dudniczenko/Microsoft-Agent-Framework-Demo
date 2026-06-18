using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using WorkflowsDemo;
using WorkflowsDemo.Agents;
using WorkflowsDemo.Events;
using WorkflowsDemo.Executors;
using WorkflowsDemo.MockData;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.PlanBuilder;
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

// chat clients
var openRouterChatClient = ChatClients.GetOpenRouterChatClient();
var ollamaChatClient = ChatClients.GetOllamaChatClient();

// agents
var requirementsProcessorAgent = RequirementsProcessorAgent.GetAgent(ollamaChatClient);

var attractionsResearcherAgent = AttractionsResearcherAgent.GetAgent(ollamaChatClient);
var accommodationResearcherAgent = AccommodationResearcherAgent.GetAgent(ollamaChatClient);
var restaurantsResearcherAgent = RestaurantsResearcherAgent.GetAgent(ollamaChatClient);

var planBuilderAgent = PlanBuilderAgent.GetAgent(ollamaChatClient);
var planReviewerAgent = PlanReviewerAgent.GetAgent(ollamaChatClient);

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
                ConsoleWriteLineInColor(
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

            ConsoleWriteInColor(
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

#region DevUI Setup

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Warning);

builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();
builder.Services.AddDevUI();

builder.AddWorkflow(DemoWorkflowName, (_, _) => workflow);

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services
        .GetRequiredService<IServer>()
        .Features
        .Get<IServerAddressesFeature>()
        ?.Addresses;

    var address = addresses?.FirstOrDefault();

    Console.WriteLine("\n=================================================================\n");
    Console.WriteLine($"DevUI running at: {address}/devui");
});

app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();

await app.RunAsync();

#endregion

static void ConsoleWriteLineInColor(string value, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(value);
    Console.ResetColor();
}

static void ConsoleWriteInColor(string value, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.Write(value);
    Console.ResetColor();
}
