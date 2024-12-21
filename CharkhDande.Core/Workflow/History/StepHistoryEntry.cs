using System.Text.Json.Serialization;

public record StepHistoryEntry
{
    public string StepId { get; set; }
    public DateTime TimeStamp { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StepHistoryType Type { get; set; }
    public bool IsSuccess { get; set; }
    public string Description { get; set; }

    public override string ToString()
    {
        return $"{TimeStamp}: {StepId}, {Type}, {IsSuccess}, {Description}";
    }
}
