﻿using System.Text.Json;

public class LambdaCondition : ICondition
{
    private readonly Func<WorkflowContext, InitiatorMetaData, bool> _predicate;
    private readonly string _conditionKey;
    public LambdaCondition(string actionKey)
    {
        _conditionKey = actionKey ?? throw new ArgumentNullException(nameof(actionKey));
        _predicate = ConditionRegistry.Resolve(actionKey);
    }
    //TODO: inject failure logic
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData)
    {
        var eval = false;
        var desc = string.Empty;
        try
        {
            eval = _predicate(context, initiatorMetaData);
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
            Key = _conditionKey
        };
    }
}
