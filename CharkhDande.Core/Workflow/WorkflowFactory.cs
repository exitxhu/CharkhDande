using CharkhDande.Core.Steps;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;
using System.Text.Json.Serialization;

public class WorkflowFactory(IServiceProvider serviceProvider,
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
        using var scop = serviceProvider.CreateScope();

        var workflowResolver = scop.ServiceProvider.GetService<IWorkflowResolver>();

        var json = await workflowResolver.FetchJsonAsync(id);
        var wf = Reconstruct(json);
        return wf;
    }
    public Workflow Reconstruct(string json)
    {
        var obj = JsonSerializer.Deserialize<WorkflowSerializableObject>(json);

        var wf = new Workflow(serviceProvider, obj.id);

        foreach (var val in obj.Context.Properties)
        {
            wf.Context.Set(val.Key, val.Value);
        }

        var steps = obj.Steps.Select(stepFactory.Deserialize).ToList();

        wf.AddSteps(steps);

        wf.CurrentStep = wf.GetStep(obj.CurrentStep.Id) ?? wf.StartStep;

        return wf;
    }
    public Workflow ReconstructAsNew(string json)
    {
        var obj = JsonSerializer.Deserialize<WorkflowSerializableObject>(json);

        var wf = new Workflow(serviceProvider, Guid.NewGuid().ToString());

        var steps = obj.Steps.Select(stepFactory.Deserialize).ToList();

        wf.AddSteps(steps);

        foreach (var step in wf._steps)
        {
            step.Value.Reset(wf.Context);
        }

        wf.CurrentStep = wf.StartStep;

        wf.CheckState();


        return wf;
    }
}
