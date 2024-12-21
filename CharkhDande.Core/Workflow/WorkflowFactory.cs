using System.Text.Json;
using System.Text.Json.Serialization;

public class WorkflowFactory(IServiceProvider serviceProvider)
{
    public Workflow GetGuidInstance()
    {
        var wf = new Workflow(serviceProvider, Guid.NewGuid().ToString());
        return wf;
    }
    public Workflow GetInstance(string id)
    {
        var wf = new Workflow(serviceProvider, id);
        return wf;
    }
    public Func<string, Task<Workflow>> WorkflowResolver { get; set; }
    public async Task<Workflow> FetchAsync(string id)
    {
        if (WorkflowResolver is null)
            throw new Exception("there is no external resolver to fetch workflows");
        var wf = await WorkflowResolver(id);
        return wf;
    }
    public Workflow Reconstruct(string json)
    {
        var obj = JsonSerializer.Deserialize<WorkflowSerializableObject>(json);

        var wf = new Workflow(serviceProvider, obj.id);

        var steps = obj.Steps.Select(a=> )

        return default;
    }
}