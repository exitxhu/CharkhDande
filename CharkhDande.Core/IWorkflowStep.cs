// See https://aka.ms/new-console-template for more information
public interface IWorkflowStep
{
    string Id { get; }
    void Execute(WorkflowContext context);
    IEnumerable<IRoute> GetRoutes();
}
