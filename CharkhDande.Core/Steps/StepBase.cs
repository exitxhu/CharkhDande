public abstract class StepBase : IWorkflowStep
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

}
