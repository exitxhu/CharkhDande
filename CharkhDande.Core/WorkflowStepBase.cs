// See https://aka.ms/new-console-template for more information

public abstract class WorkflowStepBase : IWorkflowStep
{
    public string Id { get; init; }
    public IEnumerable<IRoute> Routes { get; set; }
    protected WorkflowStepBase(string id)
    {
        Id = id;
    }

    public abstract void Execute(WorkflowContext context);

    public IEnumerable<IRoute> GetRoutes() => Routes;

}
