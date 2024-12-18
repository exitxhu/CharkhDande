using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace CharkhDande.Tests;

public class WorkFlowTests
{
    [Fact]
    public async Task SimpleCreation()
    {
        var services = new ServiceCollection();





        services.AddTransient<IMessageService, ConsoleMessageService>();
        services.AddTransient<Repo>();
        services.AddSingleton<ActionRegistry>();
        services.AddSingleton<ConditionRegistry>();
        var serviceProvider = services.BuildServiceProvider();

        var myWorkFlow = new Workflow(serviceProvider);

        myWorkFlow.Should().NotBeNull();
        myWorkFlow.Context.Should().NotBeNull();

        var actionReg = serviceProvider.GetRequiredService<ActionRegistry>();
        var conditionReg = serviceProvider.GetRequiredService<ConditionRegistry>();

        Configurator.RegisterActions(actionReg);
        Configurator.RegisterConditions(conditionReg);

        var emailActionKey = "sendEmailOnCondition";

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


        var task3 = new ConditionalStep("third")
        {
            _actions = [new LambdaAction("taskRun")]
        };
        var task2 = new ConditionalStep("second")
        {
            Routes = [ new ConditionalRoute("GoTask3R"){
        GetNext = ctx => task3,
        _conditions = [
            new LambdaCondition(Configurator.ConditionDocIdOdd)
            ]
    },
    ],
            _actions = [new LambdaAction("taskRun")]
        };

        task3.Routes = [new ConditionalRoute("FinishR") {
    GetNext = ctx => null
}];



        var task1 = new ConditionalStep("first")
        {
            _conditions = new List<ICondition>
    {
        new LambdaCondition(Configurator.ConditionDocIdOdd),
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
        await Task.WhenAll(tasks);


        var histores = workflow.GetHistory();

        var js = workflow.ExportWorkFlow();

        var t = 0;

    }

    [Fact]
    public void ConditionStep()
    {

    }
}