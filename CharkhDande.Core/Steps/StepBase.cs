using System.Text.Json;
using System.Text.Json.Serialization;

using static IStep;
using static Workflow;

public abstract class StepBase : IStep
{
    public string Id { get; init; }
    public IEnumerable<IRoute> Routes { get; set; }
    public StepState State { get; set; }

    public abstract string StepType { get; }

    protected StepBase(string id)
    {
        Id = id;
    }

    public abstract WorkflowExecutionResult Execute(WorkflowContext context);

    public IEnumerable<IRoute> GetRoutes() => Routes ?? Enumerable.Empty<IRoute>();

    public abstract string Serialize(WorkflowContext context);
    //{
    //    return JsonSerializer.Serialize(SerializeObject(context));
    //}

    public abstract StepSerializeObject SerializeObject(WorkflowContext context);
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
