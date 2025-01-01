using CharkhDande.Core;

using System.Security.AccessControl;
using System.Text.Json;
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
    bool IsFirstStep { get; set; }

    IEnumerable<IRoute> GetAllRoutes();
    IEnumerable<IRoute> GetRoutes(WorkflowContext context);

    void SetRoutes(params IEnumerable<IRoute> routes);
    void SetState(StepState state);
    void Reset(WorkflowContext context);

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
    public bool IsFirstStep { get; set; }
    public Dictionary<string, ObjectMetadata> MetaData { get; set; } = new();
}
[method: JsonConstructor]
public record ObjectMetadata(string Value, string Type)
{
    public ObjectMetadata(object obj) : this(JsonSerializer.Serialize(obj), obj.GetType().AssemblyQualifiedName!)
    {

    }
    public object GetObject()
    {

        try
        {
            return JsonSerializer.Deserialize(Value, System.Type.GetType(Type)!)!;
        }
        catch (Exception ex)
        {
            throw new Exception($"{Value}:{Type}", ex);
        }

    }
}
