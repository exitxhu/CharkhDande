
public interface IRoute
{
    bool Execute(WorkflowContext context);
    Func<WorkflowContext, IWorkflowStep> GetNext { get; }

}
