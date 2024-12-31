using CharkhDande.Core;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Workflow
{
    private readonly IServiceProvider _serviceProvider;
    private ILoopDetectionPolicy? _loopDetectionPolicy = null;
    private IStep currentStep;
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

    [JsonIgnore]
    public IStep StartStep => _steps.SingleOrDefault(a => a.Value.IsFirstStep).Value;
    [JsonInclude]
    public IStep CurrentStep { get => currentStep ?? StartStep; internal set => currentStep = value; }
    public void AddSteps(params IEnumerable<IStep> steps)
    {
        foreach (var step in steps)
        {
            if (!CheckState())
                throw new Exception("Workflow is not in a defenitive state");
            _steps.Add(step.Id, step);
        }
    }
    public bool Next()
    {
        _loopDetectionPolicy?.Clear();


        CurrentStep.Execute(Context);

        var routes = CurrentStep.GetRoutes(Context);

        foreach (var route in routes)
        {
            if (!route.Execute(Context))
            {
                Console.WriteLine("something failed");
                return false;
            }
            CurrentStep = this.GetStep(route.NextStep.id);
        }
        return true;
    }

    public void Run()
    {
        _loopDetectionPolicy?.Clear();
        CurrentStep = StartStep;
        CurrentStep = GoThroughSteps(CurrentStep);
    }
    public bool CheckState()
    {
        if (_steps.Count(a => a.Value.IsFirstStep) > 1)
            return false;
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

            var routes = currentStep.GetRoutes(Context);
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
            Context.SerializeObject(),
            CurrentStep.SerializeObject(Context)
        );
        return JsonSerializer.Serialize(wf, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
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

public record WorkflowSerializableObject(string id, IEnumerable<StepSerializeObject> Steps, ContextSerializableObject Context, StepSerializeObject CurrentStep);
