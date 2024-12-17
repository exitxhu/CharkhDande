
using CharkhDande.Core;

public interface IRoute : ICustomSerializable<RouteSerializableObject>
{

    public string Id { get; }
    bool Execute(WorkflowContext context);
    bool Evaluate(WorkflowContext context);
    Func<WorkflowContext, IStep> GetNext { get; }

}
public class RouteSerializableObject
{
    public string Id { get; internal set; }
    public IEnumerable<ActionSerializableObject> Action { get; internal set; }
    public IEnumerable<ConditionSerializableObject> Condition { get; internal set; }
    public StepSerializeObject? Next { get; internal set; }
}