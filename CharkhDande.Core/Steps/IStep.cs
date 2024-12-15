using CharkhDande.Core;

public interface IStep : ICustomSerializable
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