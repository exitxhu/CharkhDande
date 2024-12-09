// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();

// Register services
services.AddTransient<IMessageService, ConsoleMessageService>();
services.AddTransient<Repo>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

var monitorTask = new MonitorStep()
{
    Condition = new LambdaCondition(ctx => ctx.Get<bool>("jobCompleted")),
    PollingInterval = TimeSpan.FromSeconds(1),
    Timeout = TimeSpan.FromSeconds(10),
    OnSuccessActions = new List<IAction>
    {
        new LambdaAction(ctx => Console.WriteLine("Job completed successfully.")),
    },
    OnTimeoutActions = new List<IAction>
    {
        new LambdaAction(ctx => Console.WriteLine("Job completion timed out.")),
    },
};


// Resolve the main application class and run it
var workflow = new Workflow(serviceProvider)
{
    Context = new WorkflowContext
    {
        ServiceProvider = serviceProvider
    }
};

workflow.Context.Set("jobCompleted", false);

workflow.StartTask = monitorTask;
// Link the Workflow
var task3 = new ConditionalStep("third");
var task2 = new ConditionalStep("second")
{
    GetNext = ctx => task3
};

task3.GetNext = ctx => task2;

var task1 = new ConditionalStep("first")
{
    _conditions = new List<ICondition>
    {
        new LambdaCondition(ctx =>{
            var id = ctx.Get<int>("doc_id");
            Repo? repo = ctx.ServiceProvider.GetRequiredService<Repo>();
            var ent = repo.Get(id);
           return ent.id %2 == 1;
        }),
        new LambdaCondition(ctx => ctx.Get<int>("age") > 5)
    },
    _actions = new List<IAction>
    {
        new LambdaAction(ctx =>
            ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("email someone")),
        new LambdaAction(ctx =>
            ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("trigger a job")),
        new LambdaAction(ctx =>
            ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("dispatch an event"))
    },
    GetNext = ctx => task2 // Next task resolver
};
monitorTask.GetNext = ctx => task1;
// Run the Workflow
workflow.Context.Set("age", 10);
workflow.Context.Set("doc_id", 1);

var tasks = new List<Task>()
{
Task.Run(() => workflow.Run()),


Task.Run(async () =>
{
    await Task.Delay(4500);
    workflow.Context.Set("jobCompleted", true);
})
};
Task.WhenAll(tasks).Wait();





public class LambdaCondition : ICondition
{
    private readonly Func<WorkflowContext, bool> _predicate;

    public LambdaCondition(Func<WorkflowContext, bool> predicate)
    {
        _predicate = predicate;
    }

    public bool Evaluate(WorkflowContext context)
    {
        return _predicate(context);
    }
}

public class LambdaAction : IAction
{
    private readonly Action<WorkflowContext> _action;

    public LambdaAction(Action<WorkflowContext> action)
    {
        _action = action;
    }

    public void Execute(WorkflowContext context)
    {
        _action(context);
    }
}

public class Workflow
{
    private readonly IServiceProvider _serviceProvider;

    public Workflow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public WorkflowContext Context { get; set; } = new WorkflowContext();

    public IWorkflowStep StartTask { get; set; }

    public void Run()
    {
        var currentTask = StartTask;
        while (currentTask != null)
        {
            currentTask.Execute(Context);
            currentTask = currentTask.GetNext(Context);
        }
    }
}

public interface IWorkflowStep
{
    string Id { get; }
    void Execute(WorkflowContext context);
    Func<WorkflowContext, IWorkflowStep> GetNext { get; }
}
public abstract class WorkflowStepBase : IWorkflowStep
{
    public string Id { get; init; }

    protected WorkflowStepBase(string id)
    {
        Id = id;
    }

    public abstract void Execute(WorkflowContext context);

    public Func<WorkflowContext, IWorkflowStep> GetNext { get; set; } = (ctx) => null;

}
public class MonitorStep : IWorkflowStep
{
    public Func<WorkflowContext, IWorkflowStep> GetNext { get; set; }

    public string Id { get; }
    public ICondition Condition { get; set; }
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public List<IAction> OnTimeoutActions { get; set; } = new();
    public List<IAction> OnSuccessActions { get; set; } = new();

    public void Execute(WorkflowContext context)
    {
        var startTime = DateTime.UtcNow;
        bool conditionMet = false;

        while (DateTime.UtcNow - startTime < Timeout)
        {
            if (Condition.Evaluate(context))
            {
                conditionMet = true;
                break;
            }
            Console.WriteLine("monitor iteration...");
            Thread.Sleep(PollingInterval);
        }

        if (conditionMet)
        {
            OnSuccessActions.ForEach(action => action.Execute(context));
        }
        else
        {
            OnTimeoutActions.ForEach(action => action.Execute(context));
        }
    }
}

public class ConditionalStep : WorkflowStepBase
{
    public List<ICondition> _conditions = new();
    public List<IAction> _actions = new();
    public Func<WorkflowContext, IWorkflowStep> _nextTaskResolver;

    public ConditionalStep(string id, Func<WorkflowContext, IWorkflowStep> nextTaskResolver = null)
        : base(id)
    {
        _nextTaskResolver = nextTaskResolver;
    }

    public void AddCondition(ICondition condition) => _conditions.Add(condition);

    public void AddAction(IAction action) => _actions.Add(action);

    public override void Execute(WorkflowContext context)
    {
        if (_conditions.All(c => c.Evaluate(context)))
        {
            foreach (var action in _actions)
            {
                action.Execute(context);
            }
        }
    }

}

public interface ICondition
{
    bool Evaluate(WorkflowContext context);
}

public interface IAction
{
    void Execute(WorkflowContext context);
}

public class WorkflowContext
{
    private readonly Dictionary<string, object> _properties = new();

    public T Get<T>(string key) => (T)_properties[key];
    public void Set<T>(string key, T value) => _properties[key] = value;

    public IServiceProvider ServiceProvider { get; init; }
}

//public class WF
//{
//    public WF(IServiceProvider service)
//    {
//        Context = new WFContext { ServiceProvider = service };
//    }
//    public WFContext Context { get; set; }
//    public WFTask First { get; set; }

//    public void Run()
//    {
//        First.Execute(Context);
//    }
//}
//public class WFTask
//{
//    public List<Func<WFContext, bool>> Conditions { get; set; }
//    public List<Action<WFContext>> Actions { get; set; }
//    public required string Id { get; set; }
//    public WFTask Next { get; set; }
//    public void Execute(WFContext context)
//    {
//        if (Conditions?.All(a => a(context)) == true)
//        {
//            Actions.ForEach(a => a(context));
//            Next.Execute(context);
//        }
//    }
//}
//public class WFContext
//{
//    public Dictionary<string, object> Properties { get; set; } = new();
//    public required IServiceProvider ServiceProvider { get; set; }
//    public void Set<T>(string key, T value)
//    {
//        Properties[key] = value;
//    }
//    public T Get<T>(string key)
//    {
//        Properties.TryGetValue(key, out object val);
//        return (T)Convert.ChangeType(val, typeof(T));
//    }

//}
public class FuncResolver
{
    public void Resolve(string name)
    {

    }
}
public static class FunctionStore
{
}

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