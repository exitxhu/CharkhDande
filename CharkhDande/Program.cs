
using CharkhDande.Core;
using CharkhDande.Core.Actions;
using CharkhDande.Core.Conditions;
using CharkhDande.Core.Steps;
using CharkhDande.Kesho;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.SymbolStore;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();

// Register services
services.AddTransient<IMessageService, ConsoleMessageService>();
services.AddTransient<Repo>();
services.AddSingleton<WFRepo>();

services.AddCharkhDande(a => a.AddKesho(services));

services.AddSingleton<IWorkflowResolver>(a => a.GetRequiredService<WFRepo>());

services.AddSingleton<ActionRegistry>();
services.AddSingleton<ConditionRegistry>();



// Build the service provider
var serviceProvider = services.BuildServiceProvider();
var actionRegistry = serviceProvider.GetRequiredService<ActionRegistry>();
var conditionRegistry = serviceProvider.GetRequiredService<ConditionRegistry>();

var emailActionKey = "sendEmailOnCondition";
actionRegistry.Register(emailActionKey, (ctx, init) =>
{
    Console.WriteLine("sendEmail");
    var T = Random.Shared.Next(0, 100) % 3 == 2;
    if (T)
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("email someone");
});

actionRegistry.Register("jobAction", (ctx, init) => Console.WriteLine("Job completed successfully."));
actionRegistry.Register("jobTimeOutAction", (ctx, init) => Console.WriteLine("Job completion timed out."));
conditionRegistry.Register("docIdEven", (ctx, init) =>
{
    var id = ctx.Get<int>("doc_id");
    Repo? repo = ctx.ServiceProvider.GetRequiredService<Repo>();
    var ent = repo.Get(id);
    return ent.id % 2 == 1;
});
var lambdaAction = new ReferenceAction(emailActionKey);

var eventStep = new EventListenerStep("eventStep", "accept_topic");

eventStep.Actions.Add(new ReferenceAction("jobAction"));

var monitorStep = new MonitorStep("monitor 1")
{
    Condition = new ExpressionCondition(ctx => ctx.Get<bool>("jobCompleted")),
    PollingInterval = TimeSpan.FromSeconds(1),
    Timeout = TimeSpan.FromSeconds(10),
    OnSuccessActions = new List<IAction>
    {
        new ReferenceAction("jobAction"),
    },
    OnTimeoutActions = new List<IAction>
    {
        new ReferenceAction("jobTimeOutAction"),
    },
};



var conditionX = new ConditionX();
var conditionY = new ConditionY();
var conditionZ = new ConditionZ();
var conditionM = new ConditionX();
var conditionN = new ExpressionCondition(ctx => 1 == 1);

CompositeCondition? groupedCondition = conditionX.And(conditionY).Or(conditionZ.Or(conditionM.And(conditionN)));

var factory = serviceProvider.GetRequiredService<WorkflowFactory>();
var wfRepo = serviceProvider.GetRequiredService<WFRepo>();

var workflow = factory.GetGuidInstance();

wfRepo.Store.Add(workflow.Id, workflow);

workflow.Context.Set("jobCompleted", false);
workflow.Context.Set("X", false);
workflow.Context.Set("Y", true);
workflow.Context.Set("Z", true);

bool result = groupedCondition.Evaluate(workflow.Context, new(InitiatorType.WorkFlow, "0"));

workflow.StartStep = monitorStep;
// Link the Workflow

actionRegistry.Register("taskRun", (ctx, init) => ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage($"task {init.InitiatorId} runs"));

var step3 = new ConditionalStep("third")
{
    Actions = [new ReferenceAction("taskRun")]
};
var step2 = new ConditionalStep("second")
{
    Routes = [ new ConditionalRoute("GoTask3R"){
        NextStep = new NextStepMetadate( step3.Id),
        _conditions = [
            new ReferenceCondition("docIdEven")
            ]
    },
    ],
    Actions = [new ReferenceAction("taskRun")]
};

actionRegistry.Register("jobTriggerKey", (ctx, init) =>
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("trigger a job"));
actionRegistry.Register("dispatchEventKey", (ctx, init) =>
        ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("dispatch an event"));



var step1 = new ConditionalStep("first")
{
    Conditions = new List<ICondition>
    {
        new ReferenceCondition("docIdEven"),
    },
    Actions = new List<IAction>
    {
        new ReferenceAction(emailActionKey) ,
    new ReferenceAction("jobTriggerKey"),
    new ReferenceAction("dispatchEventKey")
    }
,
    Routes = [new ConditionalRoute("GoTask2R") { NextStep = new NextStepMetadate(step2.Id) }]
};
monitorStep.Routes = [new ConditionalRoute("GoTask1R") { NextStep = new NextStepMetadate(eventStep.Id) }];
// Run the Workflow
workflow.Context.Set("age", 10);
workflow.Context.Set("doc_id", 1);

var evn = serviceProvider.GetRequiredService<KafkaEventSource>();

workflow.AddSteps(step1, step2, step3, monitorStep, eventStep);



var tasks = new List<Task>()
{

Task.Run(workflow.Run),


Task.Run(async () =>
{
    await Task.Delay(200);
    workflow.Context.Set("jobCompleted", true);
}),
Task.Run(async () =>
{
    await Task.Delay(2000);
    evn.PushEvent("accept_topic", "something");
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

public class WFRepo : IWorkflowResolver
{
    public Dictionary<string, Workflow> Store { get; set; } = new();

    public async Task<Workflow> FetchAsync(string id)
    {
        return Store.FirstOrDefault(a => a.Key == id).Value;
    }
}