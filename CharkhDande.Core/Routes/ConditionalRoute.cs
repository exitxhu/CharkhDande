using CharkhDande.Core.Routes;
using CharkhDande.Core.Steps;

using System.Text.Json;

public class ConditionalRoute : IRoute
{
    public List<ICondition> Conditions = new();
    public List<IAction> Actions = new();
    private NextStepMetadata nextStep;
    private readonly InitiatorMetaData initiatorMetaData;



    public string Id { get; private set; }

    public NextStepMetadata NextStep
    {
        get => nextStep ?? throw new NotImplementedException("Routes must have a valid destination");
        set => nextStep = value;
    }

    public string RouteType => GetType().FullName!;

    public ConditionalRoute(string id)
    {
        Id = id;
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);

    }

    public void AddCondition(ICondition condition) => Conditions.Add(condition);
    public void AddAction(IAction action) => Actions.Add(action);

    public bool Execute(WorkflowContext context)
    {
        if (Evaluate(context))
        {
            foreach (var action in Actions)
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
            Actions = Actions.Select(a => a.SerializeObject(context)),
            Conditions = Conditions.Select(a => a.SerializeObject(context)),
            NextStepId = nextStep,
            Type = RouteType
        };
    }

    public bool Evaluate(WorkflowContext context)
    {
        return Conditions.All(c => c.Evaluate(context, initiatorMetaData));
    }
}
public class ConditionalRouteDeserializer() : IRouteDeserializer<ConditionalRoute>
{
    public ConditionalRoute Deserialize(RouteSerializableObject obj)
    {
        var res = new ConditionalRoute(obj.Id);

        foreach (var item in obj.Actions)
        {
            res.Actions.Add(new ReferenceAction(item.Key));
        }
        foreach (var item in obj.Conditions)
        {
            res.Conditions.Add(new ReferenceCondition(item.Key));
        }

        res.NextStep = obj.NextStepId;
        
        return res;
    }
}
