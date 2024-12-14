public class WorkflowContext
{
    private readonly Dictionary<string, object> _properties = new();
    internal readonly WorkflowHistoryWriter workflowHistoryWriter = new();

    public T Get<T>(string key) => (T)_properties[key];
    public void Set<T>(string key, T value) => _properties[key] = value;

    public IServiceProvider ServiceProvider { get; init; }
}
internal class WorkflowHistoryWriter
{
    public void Write(string id, StepHistoryType type, bool success, string description)
    {
        Histories.Add(new()
        {
            Description = description,
            IsSuccess = success,
            StepId = id,
            TimeStamp = DateTime.Now,
            Type = type
        });
    }
    List<StepHistoryEntry> Histories = new();
    public List<StepHistoryEntry> GetHistoryEntries() => Histories;
}
public class StepHistoryEntry
{
    public string StepId { get; set; }
    public DateTime TimeStamp { get; set; }
    public StepHistoryType Type { get; set; }
    public bool IsSuccess { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        return $"{TimeStamp}: {StepId}, {Type}, {IsSuccess}, {Description}";
    }
}

public enum StepHistoryType
{
    CONDITION,
    ACTION,
    ROUTE,
    EXECUTE_STARTED,
    EXECUTE_FINISHED
}