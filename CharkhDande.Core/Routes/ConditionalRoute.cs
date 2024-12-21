using System.Text.Json;

public class ConditionalRoute : IRoute
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();
    private NextStepMetadate nextStep;
    private readonly InitiatorMetaData initiatorMetaData;


    public string Id { get; private set; }

    public NextStepMetadate NextStep { get => nextStep ?? throw new NotImplementedException("Routes must have a valid destination"); set => nextStep = value; }

    public ConditionalRoute(string id)
    {
        Id = id;
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);

    }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);
    public void AddAction(IAction action) => _actions.Add(action);

    public bool Execute(WorkflowContext context)
    {
        if (Evaluate(context))
        {
            foreach (var action in _actions)
            {
                action.Execute(context, initiatorMetaData);
            }
            return true;
        }
        return false;
    }

    public string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public RouteSerializableObject SerializeObject(WorkflowContext context)
    {
        return new()
        {
            Id = Id,
            Action = _actions.Select(a => a.SerializeObject(context)),
            Condition = _conditions.Select(a => a.SerializeObject(context)),
            NextStepId = nextStep,
        };
    }

    public bool Evaluate(WorkflowContext context)
    {
        return _conditions.All(c => c.Evaluate(context, initiatorMetaData));
    }
}
