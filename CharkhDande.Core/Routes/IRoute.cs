
using CharkhDande.Core;

public interface IRoute : ICustomSerializable
{
    public string Id { get; }
    bool Execute(WorkflowContext context);
    Func<WorkflowContext, IStep> GetNext { get; }

}
