
using CharkhDande.Core;

using System.Numerics;

public interface IRoute : ICustomSerializable<RouteSerializableObject>
{

    public string Id { get; }
    bool Execute(WorkflowContext context);
    bool Evaluate(WorkflowContext context);
    NextStepMetadate NextStep { get; }

}
public class RouteSerializableObject
{
    public string Id { get; set; }
    public IEnumerable<ActionSerializableObject> Action { get; set; }
    public IEnumerable<ConditionSerializableObject> Condition { get; set; }
    public NextStepMetadate NextStepId { get; set; }
}
public record NextStepMetadate(string id);

