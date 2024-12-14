

public class CompositeCondition : ICondition
{
    private readonly List<ICondition> _conditions;
    private readonly Func<bool, bool, bool> _combiner;

    public CompositeCondition(Func<bool, bool, bool> combiner)
    {
        _conditions = new List<ICondition>();
        _combiner = combiner;
    }

    public void AddCondition(ICondition condition)
    {
        _conditions.Add(condition);
    }

    public bool Evaluate(WorkflowContext context)
    {
        if (!_conditions.Any())
        {
            throw new InvalidOperationException("CompositeCondition must contain at least one condition.");
        }

        // Combine all conditions using the combiner function
        return _conditions
            .Select(c => c.Evaluate(context))
            .Aggregate(_combiner);
    }
}
