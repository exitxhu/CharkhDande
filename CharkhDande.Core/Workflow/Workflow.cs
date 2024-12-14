using System.Text.Json;

public class Workflow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoopDetectionPolicy _loopDetectionPolicy;
    private readonly Dictionary<string, StepState> _state = new();
    public Workflow(IServiceProvider serviceProvider, ILoopDetectionPolicy loopDetectionPolicy)
    {
        _serviceProvider = serviceProvider;
        _loopDetectionPolicy = loopDetectionPolicy;
    }

    public WorkflowContext Context { get; set; } = new WorkflowContext();
    public IWorkflowStep StartStep { get; set; }

    public void Run()
    {
        _loopDetectionPolicy.Clear(); // Ensure a clean slate for each run
        var currentStep = StartStep;
        currentStep = GoThroughSteps(currentStep);
    }
    public bool CheckState()
    {
        return true;
    }
    private IWorkflowStep GoThroughSteps(IWorkflowStep currentStep)
    {
        while (currentStep != null)
        {
            _loopDetectionPolicy.TrackStep(currentStep.Id);

            currentStep.Execute(Context);
            _state[currentStep.Id] = currentStep.State;
            var routes = currentStep.GetRoutes();
            foreach (var route in routes)
            {
                if (route.Execute(Context))
                {
                    currentStep = GoThroughSteps(route.GetNext(Context));
                    if (currentStep is not null)
                        _state[currentStep.Id] = currentStep.State;

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
        return JsonSerializer.Serialize(this);
    }
}
