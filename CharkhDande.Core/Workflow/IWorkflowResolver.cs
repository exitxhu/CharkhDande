public interface IWorkflowResolver
{
    Task<Workflow> FetchAsync(string id);
    Task<string> FetchJsonAsync(string id);
}