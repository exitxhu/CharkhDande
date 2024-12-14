// See https://aka.ms/new-console-template for more information
using System;

public class LambdaAction : IAction
{
    private readonly Action<WorkflowContext> _action;

    public LambdaAction(Action<WorkflowContext> action)
    {
        _action = action;
    }

    public void Execute(WorkflowContext context)
    {
        var eval = false;
        var desc = string.Empty;
        try
        {
             _action(context);
            desc = $"{_action} successfully executed";
            eval = true ;
        }
        catch (Exception ex)
        {
            desc = $"{_action} failed to execute, msg: {ex.Message}";
            throw;
        }
        finally
        {
            context.workflowHistoryWriter.Write("", StepHistoryType.ACTION, eval, desc);
        }
    }
}
