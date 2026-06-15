using System.Diagnostics;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WorkflowsDemo;
using WorkflowsDemo.Agents;
using WorkflowsDemo.Executors;
using WorkflowsDemo.Models;
using WorkflowsDemo.Models.PlanBuilder;
using static WorkflowsDemo.Constants;

#region  OpenTelemetry Setup

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(OpenTelemetryServiceName);

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

var planBuilderAgent = PlanBuilderAgent.GetAgent(openRouterChatClient);
var planReviewerAgent = PlanReviewerAgent.GetAgent(ollamaChatClient);

// executors
var requirementsProcessorExecutor = new RequirementsProcessorExecutor(requirementsProcessorAgent);

var accommodationResearcherExecutor = new AccommodationResearcherExecutor(accommodationResearcherAgent);
var attractionsResearcherExecutor = new AttractionsResearcherExecutor(attractionsResearcherAgent);
var restaurantsResearcherExecutor = new RestaurantsResearcherExecutor(restaurantsResearcherAgent);

var planBuilderExecutor = new PlanBuilderExecutor(planBuilderAgent);
var planReviewerExecutor = new PlanReviewerExecutor(planReviewerAgent);

var planRendererExecutor = new PlanRendererExecutor();

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

var inputData = ExampleTripRequirements.Requirements1;

StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, inputData);
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    switch (evt)
    {
        case RequestInfoEvent requestInputEvt:
            Console.WriteLine("Please review generated plan. Answer 'y' to approve or suggest changes.\n");

            var userResponse = Console.ReadLine()?.Trim()
                ?? throw new InvalidOperationException("Received NULL human feedback");

            if (userResponse.Equals("Y", StringComparison.OrdinalIgnoreCase))
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

        case WorkflowOutputEvent outputEvt:
            Console.WriteLine($"Workflow completed.");
            break;

        case WorkflowErrorEvent errorEvt:
            Console.WriteLine("ERROR: " + errorEvt.Exception);
            break;

        case ExecutorFailedEvent executorFailedEvt:
            Console.WriteLine($"Executor {executorFailedEvt.ExecutorId} failed. ERROR: {executorFailedEvt.Data}");
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

app.MapOpenAIResponses();
app.MapOpenAIConversations();
app.MapDevUI();

await app.RunAsync();

#endregion
