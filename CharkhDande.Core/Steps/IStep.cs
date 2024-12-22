using CharkhDande.Core;

using System.Text.Json.Serialization;


using static Workflow;

public interface IStep : ICustomSerializable<StepSerializeObject>
{
    string Id { get; }
    WorkflowExecutionResult Execute(WorkflowContext context);
    [JsonConverter(typeof(JsonStringEnumConverter))]
    StepState State { get; }
    /// <summary>
    /// FQDN
    /// </summary>
    public string StepType { get; }
    IEnumerable<IRoute> GetRoutes();
    public class WorkflowExecutionResult
    {
        public bool Done { get; set; }
    }

}
public enum StepState
{
    /// <summary>
    /// Did not run yet
    /// </summary>
    WAITING,
    /// <summary>
    /// proccessing
    /// </summary>
    RUNNING,
    /// <summary>
    /// finished proccessing with a result
    /// </summary>
    FINISHED,
    /// <summary>
    /// stopped during execution
    /// </summary>
    HALTED,
    /// <summary>
    /// execution failed
    /// </summary>
    FAILED
}


public record StepSerializeObject
{
    public string Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StepState State { get; set; }
    public IEnumerable<RouteSerializableObject> Routes { get; set; }
    public string Type { get; set; }
    public Dictionary<string, object> MetaData { get; set; } = new();
}