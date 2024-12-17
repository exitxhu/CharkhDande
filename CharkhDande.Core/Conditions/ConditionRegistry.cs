public static class ConditionRegistry
{
    private static readonly Dictionary<string, Func<WorkflowContext, InitiatorMetaData, bool>> _conditions = new();

    public static void Register(string key, Func<WorkflowContext, InitiatorMetaData, bool> action)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.");
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_conditions.ContainsKey(key)) throw new InvalidOperationException($"Action with key '{key}' is already registered.");

        _conditions[key] = action;
    }

    public static Func<WorkflowContext, InitiatorMetaData, bool> Resolve(string key)
    {
        if (!_conditions.TryGetValue(key, out var condition))
            throw new KeyNotFoundException($"No action found for key '{key}'.");

        return condition;
    }
}
