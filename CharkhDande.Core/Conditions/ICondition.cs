using CharkhDande.Core;

public interface ICondition: ICustomSerializable
{
    bool Evaluate(WorkflowContext context);
}
