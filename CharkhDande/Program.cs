
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();

// Register services
services.AddTransient<IMessageService, ConsoleMessageService>();
services.AddTransient<Repo>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

var emailActionKey = "sendEmailOnCondition";
ActionRegistry.Register(emailActionKey, (ctx, init) =>
{
    Console.WriteLine("sendEmail");
    var T = Random.Shared.Next(0, 100) % 3 == 2;
    if (T)
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("email someone");
});

ActionRegistry.Register("jobAction", (ctx, init) => Console.WriteLine("Job completed successfully."));
ActionRegistry.Register("jobTimeOutAction", (ctx, init) => Console.WriteLine("Job completion timed out."));
ConditionRegistry.Register("docIdEven", (ctx, init) =>
{
    var id = ctx.Get<int>("doc_id");
    Repo? repo = ctx.ServiceProvider.GetRequiredService<Repo>();
    var ent = repo.Get(id);
    return ent.id % 2 == 1;
});
var lambdaAction = new LambdaAction(emailActionKey);


var monitorTask = new MonitorStep("monitor 1")
{
    Condition = new ExpressionCondition(ctx => ctx.Get<bool>("jobCompleted")),
    PollingInterval = TimeSpan.FromSeconds(1),
    Timeout = TimeSpan.FromSeconds(10),
    OnSuccessActions = new List<IAction>
    {
        new LambdaAction("jobAction"),
    },
    OnTimeoutActions = new List<IAction>
    {
        new LambdaAction("jobTimeOutAction"),
    },
};


// Resolve the main application class and run it
var workflow = new Workflow(serviceProvider, new ConfigurableLoopDetectionPolicy()
{

})
{
    Context = new WorkflowContext
    {
        ServiceProvider = serviceProvider
    }
};

var conditionX = new ConditionX();
var conditionY = new ConditionY();
var conditionZ = new ConditionZ();
var conditionM = new ConditionX();
var conditionN = new ExpressionCondition(ctx => 1 == 1);

CompositeCondition? groupedCondition = conditionX.And(conditionY).Or(conditionZ.Or(conditionM.And(conditionN)));


workflow.Context.Set("jobCompleted", false);
workflow.Context.Set("X", false);
workflow.Context.Set("Y", true);
workflow.Context.Set("Z", true);

bool result = groupedCondition.Evaluate(workflow.Context, new(InitiatorType.WorkFlow, "0"));

workflow.StartStep = monitorTask;
// Link the Workflow

ActionRegistry.Register("taskRun", (ctx, init) => ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage($"task {init.InitiatorId} runs"));

var task3 = new ConditionalStep("third")
{
    _actions = [new LambdaAction("taskRun")]
};
var task2 = new ConditionalStep("second")
{
    Routes = [ new ConditionalRoute("GoTask3R"){
        GetNext = ctx => task3,
        _conditions = [
            new LambdaCondition("docIdEven")
            ]
    },
    ],
    _actions = [new LambdaAction("taskRun")]
};

task3.Routes = [new ConditionalRoute("FinishR") {
    GetNext = ctx => null
}];
ActionRegistry.Register("jobTriggerKey", (ctx, init) =>
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("trigger a job"));
ActionRegistry.Register("dispatchEventKey", (ctx, init) =>
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("dispatch an event"));



var task1 = new ConditionalStep("first")
{
    _conditions = new List<ICondition>
    {
        new LambdaCondition("docIdEven"),
        new ExpressionCondition(ctx => ctx.Get<int>("age") > 5)
    },
    _actions = new List<IAction>
    {
        new LambdaAction(emailActionKey) ,
    new LambdaAction("jobTriggerKey"),
    new LambdaAction("dispatchEventKey")
    }
,
    Routes = [new ConditionalRoute("GoTask2R") { GetNext = ctx => task2 }]
};
monitorTask.Routes = [new ConditionalRoute("GoTask1R") { GetNext = ctx => task1 }];
// Run the Workflow
workflow.Context.Set("age", 10);
workflow.Context.Set("doc_id", 1);


var tasks = new List<Task>()
{

Task.Run(workflow.Run),


Task.Run(async () =>
{
    await Task.Delay(1500);
    workflow.Context.Set("jobCompleted", true);
})
};
Task.WhenAll(tasks).Wait();


var histores = workflow.GetHistory();

var js = workflow.ExportWorkFlow();

var t = 0;
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