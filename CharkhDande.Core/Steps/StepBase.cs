using System.Text.Json;
using System.Text.Json.Serialization;

using static IStep;
using static Workflow;

public abstract class StepBase : IStep
{
    public string Id { get; init; }
    public List<IRoute> Routes { get; set; } = new();
    public StepState State { get; set; }
    public string StepType => GetType().FullName!;

    public bool IsFirstStep { get; set; }

    protected StepBase(string id)
    {
        Id = id;
    }

    public abstract WorkflowExecutionResult Execute(WorkflowContext context);

    public virtual IEnumerable<IRoute> GetAllRoutes() => Routes;

    public abstract string Serialize(WorkflowContext context);

    public abstract StepSerializeObject SerializeObject(WorkflowContext context);

    public void SetRoutes(params IEnumerable<IRoute> routes)
    {
        Routes.AddRange(routes);
    }

    public virtual void SetState(StepState state)
    {
        State = state;
    }

    public virtual void Reset(WorkflowContext context)
    {
        SetState(StepState.WAITING);
    }

    public virtual IEnumerable<IRoute> GetRoutes(WorkflowContext context)
    {
        return GetAllRoutes().Where(a => a.Evaluate(context));
    }
}
