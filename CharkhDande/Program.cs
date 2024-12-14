// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();

// Register services
services.AddTransient<IMessageService, ConsoleMessageService>();
services.AddTransient<Repo>();

// Build the service provider
var serviceProvider = services.BuildServiceProvider();

var monitorTask = new MonitorStep("monitor 1")
{
    Condition = new ExpressionCondition(ctx => ctx.Get<bool>("jobCompleted")),
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

var groupedCondition = conditionX.And(conditionY).Or(conditionZ.Or(conditionM.And(conditionN)));


workflow.Context.Set("jobCompleted", false);
workflow.Context.Set("X", false);
workflow.Context.Set("Y", true);
workflow.Context.Set("Z", true);

bool result = groupedCondition.Evaluate(workflow.Context);

workflow.StartStep = monitorTask;
// Link the Workflow
var task3 = new ConditionalStep("third")
{
    _actions = [
        new LambdaAction(ctx => ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("task 3 runs"))

        ]
};
var task2 = new ConditionalStep("second")
{
    Routes = [ new ConditionalRoute(){
        GetNext = ctx => task3 }
    ],
    _actions = [
        new LambdaAction(ctx => ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("task 2 runs"))
        ]
};

task3.Routes = [new ConditionalRoute {
    GetNext = ctx => null
}];

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
        new ExpressionCondition(ctx => ctx.Get<int>("age") > 5)
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
    Routes = [new ConditionalRoute() { GetNext = ctx => task2 }]
};
monitorTask.Routes = [new ConditionalRoute { GetNext = ctx => task1 }];
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
    public bool Evaluate(WorkflowContext context) => context.Get<bool>("X");
}

public class ConditionY : ICondition
{
    public bool Evaluate(WorkflowContext context) => context.Get<bool>("Y");
}

public class ConditionZ : ICondition
{
    public bool Evaluate(WorkflowContext context) => context.Get<bool>("Z");
}