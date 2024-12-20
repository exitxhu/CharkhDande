﻿using CharkhDande.Core;

using System.Text.Json;

public class WorkflowContext
{
    private readonly Dictionary<string, object> _properties = new();
    internal readonly WorkflowHistoryWriter workflowHistoryWriter = new();
    public IServiceProvider ServiceProvider { get; init; }

    public T Get<T>(string key) => (T)_properties[key];
    public bool TryGet<T>(string key, out T value)
    {
        var res = _properties.TryGetValue(key, out var val);
        if (val is T converted) value = converted;
        else value = default!;
        return res;
    }
    public void Set<T>(string key, T value)
    {
        _properties[key] = value;
    }

    public ContextSerializableObject SerializeObject()
    {
        return new ContextSerializableObject()
        {
            Properties = _properties,
        };
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(SerializeObject());
    }

}

public record ContextSerializableObject
{
    public Dictionary<string, object> Properties { get; set; }
    public List<StepHistoryEntry> History { get; set; }
}
