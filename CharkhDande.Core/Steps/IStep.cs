using CharkhDande.Core;

public interface IStep : ICustomSerializable<StepSerializeObject>
{
    string Id { get; }
    void Execute(WorkflowContext context);
    StepState State { get; }
    public string StepType { get; }
    IEnumerable<IRoute> GetRoutes();
}
public enum StepState
{
    WAITING,
    RUNNING,
    FINISHED
}

public class StepSerializeObject
{
    public string Id { get; internal set; }
    public StepState State { get; internal set; }
    public IEnumerable<RouteSerializableObject> Routes { get; internal set; }
    public string Type { get; internal set; }
    public Dictionary<string, object> MetaData { get; internal set; }
}