
using CharkhDande.Core;

using System.Text.Json;

public class MonitorStep : StepBase
{
    private const string STEP_TYPE = nameof(MonitorStep);
    public override string StepType => STEP_TYPE;

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


    public override void Execute(WorkflowContext context)
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

        return new StepSerializeObject()
        {
            State = State,
            Routes = GetRoutes().Select(a => a.SerializeObject(context)).ToArray(),
            Id = Id,
            Type = StepType,
            MetaData = meta
        };
    }
}
