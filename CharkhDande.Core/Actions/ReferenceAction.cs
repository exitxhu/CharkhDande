using CharkhDande.Core.Actions;

using Microsoft.Extensions.DependencyInjection;

using System;

using System;
using System.Text.Json;

public class ReferenceAction : IAction
{
    private Action<WorkflowContext, InitiatorMetaData> _action;
    private readonly string _actionKey;

    public ReferenceAction(string actionKey)
    {
        _actionKey = actionKey ?? throw new ArgumentNullException(nameof(actionKey));
    }

    public void Execute(WorkflowContext context, InitiatorMetaData initiator)
    {
        var actionRegistry = context.ServiceProvider.GetRequiredService<IActionRegistry>();
        _action = actionRegistry.Resolve(_actionKey);

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

    public static ReferenceAction Deserialize(string serializedData)
    {
        var metadata = JsonSerializer.Deserialize<ActionSerializableObject>(serializedData);

        if (metadata == null)
            throw new InvalidOperationException("Invalid serialized data.");

        return new ReferenceAction(metadata.Key);
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
