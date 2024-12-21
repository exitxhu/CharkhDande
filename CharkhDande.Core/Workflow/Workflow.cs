using CharkhDande.Core;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Encodings.Web;
using System.Text.Json;

public class Workflow
{
    private readonly IServiceProvider _serviceProvider;
    private ILoopDetectionPolicy? _loopDetectionPolicy = null;
    internal readonly Dictionary<string, IStep> _steps = new();
    public WorkflowContext Context { get; }
    public string Id { get; }
    internal Workflow(IServiceProvider serviceProvider, string id)
    {
        _serviceProvider = serviceProvider.CreateScope().ServiceProvider;
        Id = id;
        Context = new WorkflowContext()
        {
            ServiceProvider = _serviceProvider

        };
        Context.Set(WorkflowConstants.WORKFLOW_ID, Id);

    }

    public void SetLoopHandlingPolicy(ILoopDetectionPolicy loopDetectionPolicy)
    {
        _loopDetectionPolicy = loopDetectionPolicy;
    }

    public IStep StartStep { get; set; }
    public void AddSteps(params IEnumerable<IStep> steps)
    {
        if (!CheckState())
            throw new Exception("Workflow is not in a defenitive state");
        foreach (var step in steps)
        {
            if (!CheckState())
                throw new Exception("Workflow is not in a defenitive state");
            _steps.Add(step.Id, step);
        }
    }
    public void Next()
    {
        _loopDetectionPolicy?.Clear(); // Ensure a clean slate for each run

        var currentStep = StartStep;

        currentStep.Execute(Context);

        var routes = currentStep.GetRoutes();

        foreach (var route in routes)
        {
            if (!route.Execute(Context))
            {
                Console.WriteLine("something failed");
            }
        }
    }

    public void Run()
    {
        _loopDetectionPolicy?.Clear(); // Ensure a clean slate for each run
        var currentStep = StartStep;
        currentStep = GoThroughSteps(currentStep);
    }
    public bool CheckState()
    {
        return true;
    }
    private IStep? GoThroughSteps(IStep currentStep)
    {
        while (currentStep != null)
        {
            _loopDetectionPolicy?.TrackStep(currentStep.Id);

            if (currentStep?.State == StepState.HALTED)
                break;

            currentStep.Execute(Context);

            var routes = currentStep.GetRoutes();
            foreach (var route in routes)
            {
                if (route.Execute(Context))
                {
                    currentStep = GoThroughSteps(this.GetStep(route.NextStep.id));
                }
            }
        }
        return currentStep;
    }

    public object GetHistory()
    {
        return Context.workflowHistoryWriter.GetHistoryEntries();
    }

    public string ExportWorkFlow()
    {
        var wf = new WorkflowSerializableObject(
            this.Id,
            _steps.Select(a => a.Value.SerializeObject(Context)),
            Context.SerializeObject()
        );
        return JsonSerializer.Serialize(wf, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Reduces escaping
            WriteIndented = true
        });
    }


}
public static class WorkflowHelper
{
    public static IStep? GetStep(this Workflow workflow, string id)
    {
        workflow._steps.TryGetValue(id, out var step);
        return step;
    }
}

internal record WorkflowSerializableObject(string id, IEnumerable<StepSerializeObject> Steps, ContextSerializableObject Context);
