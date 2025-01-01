namespace CharkhDande.Core.Actions;

public interface IActionRegistry
{
    void Register(string key, Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>> action);
    void Register(string key, Action<WorkflowContext, InitiatorMetaData> action);
    Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>?> Resolve(string key);
}

public class ActionRegistry : IActionRegistry
{
    private readonly Dictionary<string, Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>>> _actions = new();

    public void Register(string key, Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>?> action)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.");
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_actions.ContainsKey(key)) throw new InvalidOperationException($"Action with key '{key}' is already registered.");

        _actions[key] = action;
    }
    public void Register(string key, Action<WorkflowContext, InitiatorMetaData> action)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace.");
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (_actions.ContainsKey(key)) throw new InvalidOperationException($"Action with key '{key}' is already registered.");

        _actions[key] = AdaptAction(action);
    }
    static Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>> AdaptAction(Action<WorkflowContext, InitiatorMetaData> action)
    {
        return (arg1, arg2, arg3) => action(arg1, arg2);
    }
    public Action<WorkflowContext, InitiatorMetaData, IEnumerable<object>?> Resolve(string key)
    {
        if (!_actions.TryGetValue(key, out var action))
            throw new KeyNotFoundException($"No action found for key '{key}'.");

        return action;
    }
}