public interface IWorkflowResolver
{
    Task<Workflow> FetchAsync(string id);
}