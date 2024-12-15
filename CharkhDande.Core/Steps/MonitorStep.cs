﻿
using System.Text.Json;

public class MonitorStep : StepBase
{
    public MonitorStep(string id) : base(id)
    {
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
            if (Condition.Evaluate(context))
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
            OnSuccessActions.ForEach(action => action.Execute(context, new InitiatorMetaData
            {
                InitiatorId = Id,
                InitiatorType = InitiatorType.Step,
            }));
        }
        else
        {
            OnTimeoutActions.ForEach(action => action.Execute(context, new InitiatorMetaData
            {
                InitiatorType = InitiatorType.Step,
                InitiatorId = Id
            }));
        }
    }
    public override string Serialize(WorkflowContext context)
    {
        var routesjson = Routes.Select(r => r.Serialize(context)).ToArray();
        var routes = string.Join(",", routesjson);

        var data = new
        {
            Id,
            Routes = routes,
            State,
            OnSuccessActions,
            OnTimeoutActions,
            PollingInterval,
            Timeout,
            Condition
        };

        return JsonSerializer.Serialize(data);
    }
}
