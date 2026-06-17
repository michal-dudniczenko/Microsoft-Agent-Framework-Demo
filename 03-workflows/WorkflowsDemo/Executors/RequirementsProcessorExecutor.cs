using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;
using static WorkflowsDemo.Config;

namespace WorkflowsDemo.Executors;

internal sealed partial class RequirementsProcessorExecutor(AIAgent agent)
    : Executor(nameof(RequirementsProcessorExecutor))
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.SendsMessage<RequirementsProcessedSignal>();
        protocolBuilder.YieldsOutput<WorkflowCompletedSignal>();

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
        Console.WriteLine("RequirementsProcessorExecutor runs, produces and saves in shared state structured " +
            "requirements for researchers");

        await context.QueueStateUpdateAsync(
            key: InitialTripRequirementsStateKeyName,
            value: requirements,
            scopeName: SharedStateScopeName,
            cancellationToken: cancellationToken);

        var prompt = JsonSerializer.Serialize(requirements);

        var result = (await agent.RunAsync<ProcessedTripRequirements>(prompt, cancellationToken: cancellationToken))
            .Result;

        if (!result.IsTripPossible)
        {
            Console.WriteLine("Unfortunately i am not able to come up with a plan that would satisfy your requirements."
                + $"\nReason: {result.TripNotPossibleExplanation}");

            await context.YieldOutputAsync(new WorkflowCompletedSignal(), cancellationToken);
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
