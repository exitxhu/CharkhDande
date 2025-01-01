using CharkhDande.Core.Actions;

using Microsoft.Extensions.DependencyInjection;

using System;

using System;
using System.Text.Json;

using static System.Runtime.InteropServices.JavaScript.JSType;

public class ReferenceAction : IAction
{
    private Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>?> _action;
    private readonly string _actionKey;
    private readonly IEnumerable<object> parameters;

    public ReferenceAction(string actionKey, params IEnumerable<object> parameters)
    {
        _actionKey = actionKey ?? throw new ArgumentNullException(nameof(actionKey));
        this.parameters = parameters;
    }

    public void Execute(WorkflowContext context, InitiatorMetaData initiator)
    {
        var actionRegistry = context.ServiceProvider.GetRequiredService<IActionRegistry>();
        _action = actionRegistry.Resolve(_actionKey);

        var eval = false;
        var desc = string.Empty;
        try
        {
            _action(context, initiator, parameters);
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

        return Deserialize(metadata);
    }

    public ActionSerializableObject SerializeObject(WorkflowContext context)
    {
        return new ActionSerializableObject
        {
            Key = _actionKey,
            Paramateres = parameters?.Select(a => new ObjectMetadata(a))
        };
    }

    internal static ReferenceAction Deserialize(ActionSerializableObject metadata)
    {
        var par = metadata.Paramateres?.Select(a => a.GetObject()).ToList();

        return new ReferenceAction(metadata.Key, par);
    }
}
public record InitiatorMetaData(InitiatorType InitiatorType, string InitiatorId);
