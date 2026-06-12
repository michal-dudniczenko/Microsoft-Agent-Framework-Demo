using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowsDemo.Events;
using WorkflowsDemo.Models;

namespace WorkflowsDemo.Executors;

internal sealed partial class RequirementsProcessorExecutor(AIAgent agent)
    : Executor<InitialUserTripRequirements, RequirementsProcessedSignal>(nameof(RequirementsProcessorExecutor))
{
    public override async ValueTask<RequirementsProcessedSignal> HandleAsync(
        InitialUserTripRequirements requirements,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("RequirementsProcessorExecutor runs, produces and saves in shared state structured " +
            "requirements for researchers");

        return new RequirementsProcessedSignal();

        // var prompt = JsonSerializer.Serialize(requirements, JsonSerializerPrettyPrint);
        // var result = await agent.RunAsync<ProcessedTripRequirements>(prompt, cancellationToken: cancellationToken);

        // await context.QueueStateUpdateAsync(
        //     key: ProcessedTripRequirementsStateKeyName, 
        //     value: result.Result, 
        //     scopeName: SharedStateScopeName, 
        //     cancellationToken: cancellationToken);

        // return new RequirementsProcessedSignal();
    }
}