using System.Text.Json;

using static IStep;

public class ConditionalStep : StepBase
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();
    public Func<WorkflowContext, IStep?> _nextTaskResolver;
    private readonly InitiatorMetaData initiatorMetaData;
    private const string STEP_TYPE = nameof(ConditionalStep);
    public override string StepType => STEP_TYPE;

    public ConditionalStep(string id, Func<WorkflowContext, IStep?>? nextTaskResolver = null)
        : base(id)
    {
        _nextTaskResolver = nextTaskResolver!;
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);

    }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);

    public void AddAction(IAction action) => _actions.Add(action);


    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        if (_conditions.All(c => c.Evaluate(context, initiatorMetaData)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context, initiatorMetaData);
                State = StepState.FINISHED;
            }
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

        for (int i = 0; i < _conditions.Count; i++)
        {
            var condition = _conditions[i];
            meta.Add("Consitions#" + (i + 1), condition.SerializeObject(context));
        }
        for (int i = 0; i < _actions.Count; i++)
        {
            var condition = _actions[i];
            meta.Add("Actions#" + (i + 1), condition.SerializeObject(context));
        }
        return new StepSerializeObject
        {
            Id = Id,
            Routes = GetRoutes()?.Select(a => a.SerializeObject(context)).ToArray(),
            State = State,
            Type = StepType,
            MetaData = meta
        };
    }
}
