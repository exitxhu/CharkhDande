using System.Text.Json;
using System.Text.Json.Serialization;

public abstract class StepBase : IStep
{
    public string Id { get; init; }
    public IEnumerable<IRoute> Routes { get; set; }
    public StepState State { get; set; }

    protected StepBase(string id)
    {
        Id = id;
    }

    public abstract void Execute(WorkflowContext context);

    public IEnumerable<IRoute> GetRoutes() => Routes;

    public virtual string Serialize(WorkflowContext context)
    {
        var routesjson = Routes.Select(r => r.Serialize(context)).ToArray();
        var routes = string.Join(",", routesjson);

        var data = new
        {
            Id,
            Routes = routes,
            State,
        };

        return JsonSerializer.Serialize(data);
    }
}
