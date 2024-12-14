public interface IWorkflowStep
{
    string Id { get; }
    void Execute(WorkflowContext context);
    StepState State { get; }
    IEnumerable<IRoute> GetRoutes();
}
public enum StepState
{
    WAITING,
    RUNNING,
    FINISHED
}