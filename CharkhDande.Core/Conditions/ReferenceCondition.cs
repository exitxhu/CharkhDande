using CharkhDande.Core.Conditions;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json;

public class ReferenceCondition : ICondition
{
    private Func<WorkflowContext, InitiatorMetaData, IEnumerable<object>?, bool> _predicate = default!;
    private readonly string _conditionKey;
    private readonly IEnumerable<object> parameters;

    public ReferenceCondition(string actionKey, params IEnumerable<object> parameters)
    {
        _conditionKey = actionKey ?? throw new ArgumentNullException(nameof(actionKey));
        this.parameters = parameters;
    }
    //TODO: inject failure logic
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData)
    {
        var conditionRegistry = context.ServiceProvider.GetRequiredService<IConditionRegistry>();

        _predicate ??= conditionRegistry.Resolve(_conditionKey);
        var eval = false;
        var desc = string.Empty;
        try
        {
            eval = _predicate(context, initiatorMetaData, parameters);
            desc = $"{_predicate} successfully evaluated";
        }
        catch (Exception ex)
        {
            desc = $"{_predicate} failed to evaluate, msg: {ex.Message}";
            throw;
        }
        finally
        {
            context.workflowHistoryWriter.Write(initiatorMetaData.InitiatorId, StepHistoryType.CONDITION, eval, desc);
        }
        return eval;
    }

    public string Serialize(WorkflowContext context)
    {
        return JsonSerializer.Serialize(SerializeObject(context));
    }

    public ConditionSerializableObject SerializeObject(WorkflowContext context)
    {
        return new ConditionSerializableObject
        {
            Key = _conditionKey,
            Paramateres = parameters?.Select(a => new ObjectMetadata(a))
        };
    }
    public static ReferenceCondition Deserialize(string serializedData)
    {
        var metadata = JsonSerializer.Deserialize<ConditionSerializableObject>(serializedData);
        if (metadata == null)
            throw new InvalidOperationException("Invalid serialized data.");
        var par = metadata.Paramateres?.Select(a => a.GetObject());

        return Deserialize(metadata);
    }
    public static ReferenceCondition Deserialize(ConditionSerializableObject metadata)
    {
        var par = metadata.Paramateres?.Select(a => a.GetObject());

        return new ReferenceCondition(metadata.Key, par);
    }
}
