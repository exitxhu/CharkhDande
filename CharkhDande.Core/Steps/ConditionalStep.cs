public class ConditionalStep : StepBase
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();
    public Func<WorkflowContext, IWorkflowStep?> _nextTaskResolver;

    public ConditionalStep(string id, Func<WorkflowContext, IWorkflowStep?>? nextTaskResolver = null)
        : base(id)
    {
        _nextTaskResolver = nextTaskResolver!;
    }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);

    public void AddAction(IAction action) => _actions.Add(action);

    public override void Execute(WorkflowContext context)
    {
        State = StepState.RUNNING;
        if (_conditions.All(c => c.Evaluate(context)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context);
                State = StepState.FINISHED;
            }
        }
    }

}
