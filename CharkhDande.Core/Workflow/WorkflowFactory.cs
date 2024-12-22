using CharkhDande.Core.Steps;

using System.Text.Json;
using System.Text.Json.Serialization;

public class WorkflowFactory(IServiceProvider serviceProvider,
    IWorkflowResolver workflowResolver,
    IStepFactory stepFactory)
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
    public async Task<Workflow> FetchAsync(string id)
    {
        var wf = await workflowResolver.FetchAsync(id);
        return wf;
    }
    public Workflow Reconstruct(string json)
    {
        var obj = JsonSerializer.Deserialize<WorkflowSerializableObject>(json);

        var wf = new Workflow(serviceProvider, obj.id);

        var steps = obj.Steps.Select(stepFactory.Deserialize).ToList();

        wf.AddSteps(steps);

        return wf;
    }
}
