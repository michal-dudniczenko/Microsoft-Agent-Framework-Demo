using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;
using static WorkflowsDemo.Constants;

namespace WorkflowsDemo.Executors;

internal sealed partial class RequirementsProcessorExecutor(AIAgent agent)
    : Executor(nameof(RequirementsProcessorExecutor))
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<InitialUserTripRequirements>(HandleAsync);
            routeBuilder.AddHandler<string, TripNotPossibleEvent>(TemporaryWorkAround);
            routeBuilder.AddHandler<int, RequirementsProcessedSignal>(TemporaryWorkAround2);
        });

        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask HandleAsync(
        InitialUserTripRequirements requirements,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("RequirementsProcessorExecutor runs, produces and saves in shared state structured " +
            "requirements for researchers");

        var prompt = JsonSerializer.Serialize(requirements, JsonSerializerPrettyPrint);

        var result = (await agent.RunAsync<ProcessedTripRequirements>(prompt, cancellationToken: cancellationToken))
            .Result;

        if (!result.IsTripPossible)
        {
            await context.SendMessageAsync(new TripNotPossibleEvent(
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

    private static async ValueTask<TripNotPossibleEvent> TemporaryWorkAround(
        string _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return new TripNotPossibleEvent("");
    }

    private static async ValueTask<RequirementsProcessedSignal> TemporaryWorkAround2(
        int _,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return new RequirementsProcessedSignal();
    }
}