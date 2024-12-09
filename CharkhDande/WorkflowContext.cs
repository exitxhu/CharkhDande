// See https://aka.ms/new-console-template for more information
public class WorkflowContext
{
    private readonly Dictionary<string, object> _properties = new();

    public T Get<T>(string key) => (T)_properties[key];
    public void Set<T>(string key, T value) => _properties[key] = value;

    public IServiceProvider ServiceProvider { get; init; }
}
