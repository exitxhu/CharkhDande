// See https://aka.ms/new-console-template for more information
public class Workflow
{
    private readonly IServiceProvider _serviceProvider;

    public Workflow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public WorkflowContext Context { get; set; } = new WorkflowContext();

    public IWorkflowStep StartStep { get; set; }

    public void Run()
    {
        var currentStep = StartStep;
        currentStep = GoThroughSteps(currentStep);
    }

    private IWorkflowStep GoThroughSteps(IWorkflowStep currentTask)
    {
        while (currentTask != null)
        {
            currentTask.Execute(Context);
            var routes = currentTask.GetRoutes();
            foreach (var route in routes)
            {
                if (route.Execute(Context))
                {
                    currentTask = GoThroughSteps(route.GetNext(Context));
                }
            }
        }
        return currentTask;
    }
}
