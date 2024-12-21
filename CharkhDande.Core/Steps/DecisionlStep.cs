using System.Text.Json;

using static IStep;

public class DecisionlStep : StepBase
{
    private readonly InitiatorMetaData initiatorMetaData;
    private readonly DecisionOutputType type;
    private const string STEP_TYPE = nameof(ConditionalStep);
    public override string StepType => STEP_TYPE;
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
        var route = BaseEvaluation.GetNextRoute(context, GetRoutes());
        if (route is null)
            return new WorkflowExecutionResult
            {

            };
        route.Execute(context);
        State = StepState.FINISHED;
        
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
        return new StepSerializeObject
        {
            Id = Id,
            Routes = GetRoutes()?.Select(a => a.SerializeObject(context)).ToArray(),
            State = State,
            Type = StepType,
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