


public static class ConditionGroup
{
    public static CompositeCondition And(this ICondition condition, params ICondition[] conditions)
    {
        var composite = new CompositeCondition((x, y) => x && y);
        composite.AddCondition(condition);

        foreach (var cond in conditions)
        {
            composite.AddCondition(cond);
        }
        return composite;
    }

    public static CompositeCondition Or(this ICondition condition, params ICondition[] conditions)
    {
        var composite = new CompositeCondition((x, y) => x || y);
        composite.AddCondition(condition);
        foreach (var cond in conditions)
        {
            composite.AddCondition(cond);
        }
        return composite;
    }
}
