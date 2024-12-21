
using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

using static IStep;
using static Workflow;

namespace CharkhDande.Kesho;

public class EventListenerStep : StepBase
{
    private const string STEP_TYPE = nameof(EventListenerStep);
    private readonly string _eventKey;
    private readonly InitiatorMetaData initiatorMetaData;

    internal readonly Func<string> _eventDateKey;

    public List<IAction> Actions { get; } = new List<IAction>();

    public EventListenerStep(string id, string eventKey) : base(id)
    {
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);
        _eventKey = eventKey;
        _eventDateKey = () => $"{id}:{eventKey}";
    }

    public override string StepType => STEP_TYPE;

    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        var res = new WorkflowExecutionResult
        {

        };
        var checkData = context.TryGet<object>(_eventDateKey(), out var data);
        if (checkData)
        {
            Actions.ForEach(a => a.Execute(context, initiatorMetaData));
            res.Done = true;
        }
        else
        {

            var workflowId = context.Get<string>(WorkflowConstants.WORKFLOW_ID);

            // Register with the WorkflowRegistry
            var registry = context.ServiceProvider.GetRequiredService<WorkflowRegistry>();
            registry.RegisterStep(workflowId, Id, _eventKey);

            State = StepState.HALTED;

            Console.WriteLine($"Workflow {workflowId} is waiting for event: {_eventKey}");
            res.Done = false;

        }
        return res;
    }

    public override string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public override StepSerializeObject SerializeObject(WorkflowContext context)
    {
        return new StepSerializeObject
        {
            Id = Id,
            Routes = GetRoutes()?.Select(a => a.SerializeObject(context))!,
            State = State,
            Type = StepType
        };
    }
}
