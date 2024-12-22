
using CharkhDande.Core;
using CharkhDande.Core.Steps;

using System;
using System.Text.Json;

using static IStep;

public class MonitorStep : StepBase
{
    private readonly InitiatorMetaData initiatorMetaData;
    public MonitorStep(string id) : base(id)
    {
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);
    }

    public ICondition Condition { get; set; }
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public List<IAction> OnTimeoutActions { get; set; } = new();
    public List<IAction> OnSuccessActions { get; set; } = new();


    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        var startTime = DateTime.UtcNow;
        State = StepState.RUNNING;
        context.workflowHistoryWriter.Write(Id, StepHistoryType.EXECUTE_STARTED, true, "");
        bool conditionMet = false;

        while (DateTime.UtcNow - startTime < Timeout)
        {
            if (Condition.Evaluate(context, initiatorMetaData))
            {
                conditionMet = true;
                State = StepState.FINISHED;
                break;
            }
            Console.WriteLine("monitor iteration...");
            Thread.Sleep(PollingInterval);
        }

        if (conditionMet)
        {
            OnSuccessActions.ForEach(action => action.Execute(context, new InitiatorMetaData(InitiatorType.Step, Id)));
        }
        else
        {
            OnTimeoutActions.ForEach(action => action.Execute(context, new InitiatorMetaData(InitiatorType.Step, Id)));
        }
        return new WorkflowExecutionResult
        {

        };
    }
    public override string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public override StepSerializeObject SerializeObject(WorkflowContext context)
    {
        var meta = new Dictionary<string, object>();

        for (int i = 0; i < OnSuccessActions.Count; i++)
        {
            var action = OnSuccessActions[i];
            meta.Add("OnSuccessActions#" + (i + 1), action.SerializeObject(context));
        }
        for (int i = 0; i < OnTimeoutActions.Count; i++)
        {
            var action = OnTimeoutActions[i];
            meta.Add("OnTimeoutActions#" + (i + 1), action.SerializeObject(context));
        }
        meta.Add("Timeout#", Timeout.ToString());
        meta.Add("PollingInterval#", PollingInterval.ToString());

        return new StepSerializeObject()
        {
            State = State,
            Routes = GetRoutes()?.Select(a => a.SerializeObject(context)).ToArray(),
            Id = Id,
            Type = StepType,
            MetaData = meta
        };
    }
}
public class MonitorStepDeserializer() : IStepDeserializer<MonitorStep>
{
    public MonitorStep Deserialize(StepSerializeObject obj)
    {
        var res = new MonitorStep(obj.Id);
        res.State = obj.State;

        foreach (var data in obj.MetaData)
        {
            var split = data.Key.Split('#');
            if (split.Length < 2)
                continue;

            if (data.Key.StartsWith("OnSuccessActions#") && data.Value is ActionSerializableObject aobj)
            {
                res.OnSuccessActions.Add(new ReferenceAction(aobj.Key));
            }
            else if (data.Key.StartsWith("OnTimeoutActions#") && data.Value is ActionSerializableObject tobj)
            {
                res.OnTimeoutActions.Add(new ReferenceAction(tobj.Key));
            }
            else if (data.Key.StartsWith("Timeout#"))
            {
                res.Timeout = TimeSpan.Parse(data.Value.ToString());
            }
            else if (data.Key.StartsWith("PollingInterval#") )
            {
                res.PollingInterval = TimeSpan.Parse(data.Value.ToString());

            }
        }

        return res;
    }
}