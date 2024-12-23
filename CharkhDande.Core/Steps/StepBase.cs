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

    public IEnumerable<IRoute> GetRoutes() => Routes;

    public abstract string Serialize(WorkflowContext context);
    //{
    //    return JsonSerializer.Serialize(SerializeObject(context));
    //}

    public abstract StepSerializeObject SerializeObject(WorkflowContext context);

    public void SetRoutes(params IEnumerable<IRoute> routes)
    {
        Routes.AddRange(routes);
    }
    //{
    //    var obj = new StepSerializeObject
    //    {
    //        Id = Id,
    //        State = State,
    //        Routes = Routes.Select((r) => r.SerializeObject(context)),
    //    };

    //    return obj;
    //}
}
