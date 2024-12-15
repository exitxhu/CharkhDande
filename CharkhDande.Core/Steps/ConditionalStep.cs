using System.Text.Json;

public class ConditionalStep : StepBase
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();
    public Func<WorkflowContext, IStep?> _nextTaskResolver;

    public ConditionalStep(string id, Func<WorkflowContext, IStep?>? nextTaskResolver = null)
        : base(id)
    {
        _nextTaskResolver = nextTaskResolver!;
    }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);

    public void AddAction(IAction action) => _actions.Add(action);

    public override void Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        if (_conditions.All(c => c.Evaluate(context)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context, new InitiatorMetaData
                {
                    InitiatorType = InitiatorType.Step,
                    InitiatorId = Id
                });
                State = StepState.FINISHED;
            }
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
            Action = _actions,
            Condition = _conditions,
        };
        return JsonSerializer.Serialize(data);
    }
}
