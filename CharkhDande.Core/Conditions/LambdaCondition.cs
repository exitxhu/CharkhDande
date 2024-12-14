// See https://aka.ms/new-console-template for more information
using System.Linq.Expressions;

public class LambdaCondition : ICondition
{
    private readonly Func<WorkflowContext, bool> _predicate;
    private readonly string _predicateHumanized;
    public LambdaCondition(Func<WorkflowContext, bool> predicate)
    {
        _predicate = predicate;
    }
    //TODO: inject failure logic
    public bool Evaluate(WorkflowContext context)
    {
        var eval = false;
        var desc = string.Empty;
        try
        {
            eval = _predicate(context);
            desc = $"{_predicate} successfully evaluated";
        }
        catch (Exception ex)
        {
            desc = $"{_predicate} failed to evaluate, msg: {ex.Message}";
            throw;
        }
        finally
        {
            context.workflowHistoryWriter.Write("", StepHistoryType.CONDITION, eval, desc);
        }
        return eval;
    }
}

public class ExpressionCondition : ICondition
{
    private readonly Expression<Func<WorkflowContext, bool>> _predicate;
    private readonly string _predicateHumanized;
    public ExpressionCondition(Expression<Func<WorkflowContext, bool>> predicate)
    {
        _predicate = predicate;
    }
    //TODO: inject failure logic
    public bool Evaluate(WorkflowContext context)
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
            context.workflowHistoryWriter.Write("", StepHistoryType.CONDITION, eval, desc);
        }
        return eval;
    }
}
