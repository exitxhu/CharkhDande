using System;

using System;
using System.Text.Json;

public class LambdaAction : IAction
{
    private readonly Action<WorkflowContext, InitiatorMetaData> _action;
    private readonly string _actionKey;

    public LambdaAction(string actionKey)
    {
        _actionKey = actionKey ?? throw new ArgumentNullException(nameof(actionKey));
        _action = ActionRegistry.Resolve(actionKey);
    }

    public void Execute(WorkflowContext context, InitiatorMetaData initiator)
    {
        var eval = false;
        var desc = string.Empty;
        try
        {
            _action(context, initiator);
            desc = $"{_actionKey} successfully executed";
            eval = true;
        }
        catch (Exception ex)
        {
            desc = $"{_actionKey} failed to execute, msg: {ex.Message}";
            throw;
        }
        finally
        {
            context.workflowHistoryWriter.Write(initiator.InitiatorId, StepHistoryType.ACTION, eval, _actionKey);
        }
    }

    public string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public static LambdaAction Deserialize(string serializedData)
    {
        var metadata = JsonSerializer.Deserialize<ActionSerializableObject>(serializedData);

        if (metadata == null)
            throw new InvalidOperationException("Invalid serialized data.");

        return new LambdaAction(metadata.Key);
    }

    public ActionSerializableObject SerializeObject(WorkflowContext context)
    {
        return new ActionSerializableObject
        {
            Key = _actionKey
        };
    }
}
public record InitiatorMetaData(InitiatorType InitiatorType, string InitiatorId);
