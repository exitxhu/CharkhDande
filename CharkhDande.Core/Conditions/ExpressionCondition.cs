
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;

public class ExpressionCondition : ICondition
{
    private readonly Expression<Func<WorkflowContext, bool>> _predicate;
    private readonly string _predicateHumanized;
    public ExpressionCondition(Expression<Func<WorkflowContext, bool>> predicate)
    {
        _predicate = predicate;
    }
    //TODO: inject failure logic
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData)
    {
        var eval = false;
        var desc = string.Empty;
        try
        {
            eval = _predicate.Compile().Invoke(context);
            desc = $"{_predicate.Body} successfully evaluated";
        }
        catch (Exception ex)
        {
            desc = $"{_predicate.Body} failed to evaluate, msg: {ex.Message}";
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
        return new ConditionSerializableObject()
        {
            Key = _predicate.Body.ToString()
        };
    }
}
