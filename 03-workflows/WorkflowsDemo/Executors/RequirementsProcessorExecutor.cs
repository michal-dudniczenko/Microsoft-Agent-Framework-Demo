using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;
using WorkflowsDemo.Utils;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Executors;

internal sealed partial class RequirementsProcessorExecutor(
    AIAgent agent,
    ILogger<RequirementsProcessorExecutor> logger)
    : Executor(nameof(RequirementsProcessorExecutor))
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.SendsMessage<RequirementsProcessedSignal>();
        protocolBuilder.YieldsOutput<TripNotPossibleSignal>();

        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<InitialUserTripRequirements>(HandleAsync);
        });

        return protocolBuilder;
    }

    private async ValueTask HandleAsync(
        InitialUserTripRequirements requirements,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.QueueStateUpdateAsync(
            key: InitialTripRequirementsStateKeyName,
            value: requirements,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        var prompt = JsonSerializer.Serialize(requirements);

        // var result = (await agent.RunAsync<ProcessedTripRequirements>(prompt, cancellationToken: cancellationToken))
        //     .Result;

        var result = JsonSerializer.Deserialize<ProcessedTripRequirements>(
            File.ReadAllText("agent-responses/requirements-processor-response.txt")
        );
        await Task.Delay(7000);

        result.SaveModelResponse(fileName: agent.Id);

        logger.LogInformation("Processed initial requirements and assessed trip feasibility");

        if (!result.IsTripPossible)
        {
            await context.YieldOutputAsync(new TripNotPossibleSignal(
                result.TripNotPossibleExplanation),
                cancellationToken);

            return;
        }

        await context.QueueStateUpdateAsync(
            key: ProcessedTripRequirementsStateKeyName,
            value: result,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        await context.SendMessageAsync(new RequirementsProcessedSignal(), cancellationToken);
    }
}
