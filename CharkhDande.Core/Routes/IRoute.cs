
using CharkhDande.Core;

using System.Numerics;

public interface IRoute : ICustomSerializable<RouteSerializableObject>
{

    public string Id { get; }
    bool Execute(WorkflowContext context);
    bool Evaluate(WorkflowContext context);
    public string RouteType { get; }
    public NextStepMetadata NextStep { get; }

}
public class RouteSerializableObject
{
    public string Id { get; set; }
    public IEnumerable<ActionSerializableObject> Actions { get; set; }
    public IEnumerable<ConditionSerializableObject> Conditions { get; set; }
    public string Type { get; set; }
    public NextStepMetadata NextStepId { get; set; }
}
public record NextStepMetadata(string id);

