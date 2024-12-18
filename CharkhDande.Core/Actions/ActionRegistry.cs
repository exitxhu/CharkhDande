public class ActionRegistry
{
    private readonly Dictionary<string, Action<WorkflowContext,InitiatorMetaData>> _actions = new();

    public void Register(string key, Action<WorkflowContext, InitiatorMetaData> action)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.");
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_actions.ContainsKey(key)) throw new InvalidOperationException($"Action with key '{key}' is already registered.");

        _actions[key] = action;
    }

    public Action<WorkflowContext, InitiatorMetaData> Resolve(string key)
    {
        if (!_actions.TryGetValue(key, out var action))
            throw new KeyNotFoundException($"No action found for key '{key}'.");

        return action;
    }
}
