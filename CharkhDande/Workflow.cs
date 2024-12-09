// See https://aka.ms/new-console-template for more information
public class Workflow
{
    private readonly IServiceProvider _serviceProvider;

    public Workflow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public WorkflowContext Context { get; set; } = new WorkflowContext();

    public IWorkflowStep StartTask { get; set; }

    public void Run()
    {
        var currentTask = StartTask;
        while (currentTask != null)
        {
            currentTask.Execute(Context);
            currentTask = currentTask.GetNext(Context);
        }
    }
}
