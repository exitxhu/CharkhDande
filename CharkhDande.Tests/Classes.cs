using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Tests;
public interface IMessageService
{
    void SendMessage(string message);
}

public class ConsoleMessageService : IMessageService
{
    public void SendMessage(string message)
    {
        Console.WriteLine($"Message: {message}");
    }
}
public class Repo
{
    public Entity Get(int id) => new Entity()
    {
        id = id
    };

}
public class Entity
{
    public int id { get; set; }
}

public class ConditionX : ICondition
{
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData) => context.Get<bool>("X");

    public string Serialize(WorkflowContext context)
    {
        throw new NotImplementedException();
    }

    public ConditionSerializableObject SerializeObject(WorkflowContext context)
    {
        throw new NotImplementedException();
    }
}

public class ConditionY : ICondition
{
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData) => context.Get<bool>("Y");

    public string Serialize(WorkflowContext context)
    {
        throw new NotImplementedException();
    }

    public ConditionSerializableObject SerializeObject(WorkflowContext context)
    {
        throw new NotImplementedException();
    }
}

public class ConditionZ : ICondition
{
    public bool Evaluate(WorkflowContext context, InitiatorMetaData initiatorMetaData) => context.Get<bool>("Z");

    public string Serialize(WorkflowContext context)
    {
        throw new NotImplementedException();
    }

    public ConditionSerializableObject SerializeObject(WorkflowContext context)
    {
        throw new NotImplementedException();
    }
}
public class WFRepo : IWorkflowResolver
{
    public Dictionary<string, Workflow> Store { get; set; } = new();

    public async Task<Workflow> FetchAsync(string id)
    {
        return Store.FirstOrDefault(a => a.Key == id).Value;
    }

    public Task<string> FetchJsonAsync(string id)
    {
        throw new NotImplementedException();
    }
}