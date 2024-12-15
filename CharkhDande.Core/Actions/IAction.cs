using CharkhDande.Core;

public interface IAction : ICustomSerializable
{
    void Execute(WorkflowContext context, InitiatorMetaData initiator);
}
