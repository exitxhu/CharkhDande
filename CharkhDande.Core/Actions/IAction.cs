using CharkhDande.Core;

public interface IAction: ICustomSerializable<ActionSerializableObject>
{
    void Execute(WorkflowContext context, InitiatorMetaData initiator);
}

public class ActionSerializableObject
{
    public string Key { get; set; }
}