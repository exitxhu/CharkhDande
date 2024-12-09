// See https://aka.ms/new-console-template for more information
public abstract class WorkflowStepBase : IWorkflowStep
{
    public string Id { get; init; }

    protected WorkflowStepBase(string id)
    {
        Id = id;
    }

    public abstract void Execute(WorkflowContext context);

    public Func<WorkflowContext, IWorkflowStep> GetNext { get; set; } = (ctx) => null;

}
