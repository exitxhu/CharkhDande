public class ConditionalRoute : IRoute
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();

    public Func<WorkflowContext, IWorkflowStep> GetNext { get; set; } = ctx => throw new NotImplementedException("Routes must have a valid destination");
    public void AddCondition(ICondition condition) => _conditions.Add(condition);
    public void AddAction(IAction action) => _actions.Add(action);

    public bool Execute(WorkflowContext context)
    {
        if (_conditions.All(c => c.Evaluate(context)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context);
            }
            return true;
        }
        return false;
    }
}
