using CharkhDande.Core.Steps;

using System.Runtime;
using System.Text.Json;

using static IStep;

public class DecisionlStep : StepBase
{
    private readonly InitiatorMetaData initiatorMetaData;
    private readonly DecisionOutputType type;
    private readonly BaseEvaluation BaseEvaluation;
    public DecisionlStep(string id, DecisionOutputType type)
        : base(id)
    {
        initiatorMetaData = new InitiatorMetaData(InitiatorType.Step, Id);
        this.type = type;
        BaseEvaluation = RoutEvaluationStrategyResolver.Resolve(type);
    }


    public override WorkflowExecutionResult Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        var route = GetRoutes(context).FirstOrDefault();
        if (route is null)
        {
            SetState(StepState.HALTED);
            return new WorkflowExecutionResult
            {
                Done = false
            };
        }
        route.Execute(context);
        State = StepState.FINISHED;

        return new WorkflowExecutionResult
        {
            Done = true
        };
    }
    public override string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public override IEnumerable<IRoute> GetRoutes(WorkflowContext context)
    {
        var route = BaseEvaluation.GetNextRoute(context, GetAllRoutes());
        if (route is null)
            return default;
        return [route];

    }
    public override StepSerializeObject SerializeObject(WorkflowContext context)
    {
        return new StepSerializeObject
        {
            Id = Id,
            Routes = GetAllRoutes()?.Select(a => a.SerializeObject(context)).ToArray(),
            IsFirstStep = IsFirstStep,
            State = State,
            Type = StepType,
            MetaData = { { "DecisionOutputType#", new(type.ToString(), typeof(DecisionOutputType).FullName) } }
        };
    }
}
internal abstract class BaseEvaluation
{
    internal abstract IRoute? GetNextRoute(WorkflowContext context, IEnumerable<IRoute> routes);
}
internal class ForcedXOREvaluation : BaseEvaluation
{
    internal override IRoute? GetNextRoute(WorkflowContext context, IEnumerable<IRoute> routes)
    {
        var evals = routes.Where(a => a.Evaluate(context));
        if (evals.Count() > 1)
            throw new Exception("multiple routes evaluated to be possible, XOR only accept one");
        return evals.FirstOrDefault();
    }
}
internal static class RoutEvaluationStrategyResolver
{
    internal static BaseEvaluation Resolve(DecisionOutputType type) => type switch
    {
        DecisionOutputType.FIRST => new FirstEvaluation(),
        DecisionOutputType.FORCED_XOR => new ForcedXOREvaluation(),
        _ => throw new Exception($"Unhandled strategy for route evaluation: {type}"),
    };
}
internal class FirstEvaluation : BaseEvaluation
{
    internal override IRoute? GetNextRoute(WorkflowContext context, IEnumerable<IRoute> routes)
    {
        return routes.FirstOrDefault(a => a.Evaluate(context));
    }
}
public enum DecisionOutputType
{
    FIRST,
    FORCED_XOR,
}
public class DecisionlStepDeserializer() : IStepDeserializer<DecisionlStep>
{
    public DecisionlStep Deserialize(StepSerializeObject obj)
    {
        var type = obj.MetaData["DecisionOutputType#"];

        var res = new DecisionlStep(obj.Id, Enum.Parse<DecisionOutputType>(type.Value));

        res.IsFirstStep = obj.IsFirstStep;

        res.State = obj.State;

        return res;
    }
}