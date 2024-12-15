using System.Text.Json;

public class ConditionalRoute : IRoute
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();

    public Func<WorkflowContext, IStep> GetNext { get; set; } = ctx => throw new NotImplementedException("Routes must have a valid destination");

    public required string Id {  get; set; }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);
    public void AddAction(IAction action) => _actions.Add(action);

    public bool Execute(WorkflowContext context)
    {
        if (_conditions.All(c => c.Evaluate(context)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context, new InitiatorMetaData
                {
                    InitiatorId = Id,
                    InitiatorType = InitiatorType.Route
                });
            }
            return true;
        }
        return false;
    }

    public string Serialize(WorkflowContext context)
    {

        var data = new
        {
            Id,
            Action = _actions,
            Condition = _conditions,
            Next = GetNext(context).ToString(),
        };
        return JsonSerializer.Serialize(data);
    }
}
