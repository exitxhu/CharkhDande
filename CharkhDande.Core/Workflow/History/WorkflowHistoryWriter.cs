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
