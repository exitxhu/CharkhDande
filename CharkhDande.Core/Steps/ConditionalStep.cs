using CharkhDande.Core.Actions;
using CharkhDande.Core.Conditions;
using CharkhDande.Core.Steps;

using System.Text.Json;

using static IStep;

public class ConditionalStep : StepBase
{
    public List<ICondition> Conditions { get; set; } = new();
    public List<IAction> Actions { get; set; } = new();
    private readonly InitiatorMetaData initiatorMetaData;

    public ConditionalStep(string id)
        : base(id)
    {
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);

    }

    public void AddCondition(ICondition condition) => Conditions.Add(condition);

    public void AddAction(IAction action) => Actions.Add(action);


    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        var res = new WorkflowExecutionResult() { Done = false };
        if (Conditions.All(c => c.Evaluate(context, initiatorMetaData)))
        {
            foreach (var action in Actions)
            {
                action.Execute(context, initiatorMetaData);
                State = StepState.FINISHED;
            }
            res.Done = true;
        }
        return res;
    }
    public override string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public override StepSerializeObject SerializeObject(WorkflowContext context)
    {
        var meta = new Dictionary<string, ObjectMetadata>();

        for (int i = 0; i < Conditions.Count; i++)
        {
            var condition = Conditions[i];
            meta.Add("Conditions#" + (i + 1), new(condition.SerializeObject(context)));
        }
        for (int i = 0; i < Actions.Count; i++)
        {
            var action = Actions[i];
            meta.Add("Actions#" + (i + 1), new(action.SerializeObject(context)));
        }
        return new StepSerializeObject
        {
            Id = Id,
            Routes = GetAllRoutes()?.Select(a => a.SerializeObject(context)).ToArray(),
            State = State,
            IsFirstStep = IsFirstStep,
            Type = StepType,
            MetaData = meta
        };
    }
}


public class ConditionalStepDeserializer() : IStepDeserializer<ConditionalStep>
{
    public ConditionalStep Deserialize(StepSerializeObject obj)
    {
        var res = new ConditionalStep(obj.Id);
        res.State = obj.State;
        res.IsFirstStep = obj.IsFirstStep;

        foreach (var data in obj.MetaData)
        {
            if (data.Key.StartsWith("Actions#"))
            {
                res.Actions.Add(ReferenceAction.Deserialize(data.Value.JSON));
            }
            else if (data.Key.StartsWith("Conditions#"))
            {
                res.Conditions.Add(ReferenceCondition.Deserialize(data.Value.JSON));
            }
        }

        return res;
    }
}