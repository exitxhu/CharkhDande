﻿
using CharkhDande.Core.Steps;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

using static IStep;
using static Workflow;

namespace CharkhDande.Kesho;

public class EventListenerStep : StepBase
{
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

    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        var res = new WorkflowExecutionResult
        {

        };
        var checkData = context.TryGet<object>(_eventDateKey(), out var data);
        if (checkData)
        {
            Console.WriteLine($"Workflow {context.Get<string>(WorkflowConstants.WORKFLOW_ID)} is processing valid event: {_eventKey}");

            Actions.ForEach(a => a.Execute(context, initiatorMetaData));
            State = StepState.FINISHED;
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
            IsFirstStep = IsFirstStep,
            Type = StepType,
            MetaData = { { "EventKey#", new(_eventKey) } }
        };
    }
}

public class EventListenerStepDeserializer : IStepDeserializer<EventListenerStep>
{
    public EventListenerStep Deserialize(StepSerializeObject obj)
    {
        var t = obj.MetaData["EventKey#"];

        var res = new EventListenerStep(obj.Id, t.Value);

        res.IsFirstStep = obj.IsFirstStep;

        res.State = obj.State;

        return res;
    }
}
