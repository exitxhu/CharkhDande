namespace CharkhDande.Kesho;

public class KafkaEventSource
{
    private readonly WorkflowRegistry _registry;

    public KafkaEventSource(WorkflowRegistry registry)
    {
        _registry = registry;
    }

    public void PushEvent(string eventKey, string eventData)
    {
        Console.WriteLine($"Event received: {eventKey}");
        _registry.TriggerEvent(eventKey, eventData);
    }
}