using CharkhDande.Core;

public interface ICondition : ICustomSerializable<ConditionSerializableObject>
{
    bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData);
}
public class ConditionSerializableObject
{
    public string Key { get; set; }
    public IEnumerable<ObjectMetadata>? Paramateres { get; set; }
}
