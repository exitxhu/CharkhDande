using CharkhDande.Core;
using CharkhDande.Core.Actions;
using CharkhDande.Core.Conditions;
using CharkhDande.Kesho;

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
        services.AddCharkhDande(a => a
             .AddKesho(services)
             .AddWorkflowResolver<WFRepo>(services));


        var serviceProvider = services.BuildServiceProvider();

        var actionReg = serviceProvider.GetRequiredService<IActionRegistry>();
        var conditionReg = serviceProvider.GetRequiredService<IConditionRegistry>();

        Configurator.RegisterActions(actionReg);
        Configurator.RegisterConditions(conditionReg);

        var WFfactory = serviceProvider.GetRequiredService<WorkflowFactory>();

        var myWorkFlow = WFfactory.GetGuidInstance();

        myWorkFlow.Should().NotBeNull();
        myWorkFlow.Context.Should().NotBeNull();

    }
    [Fact]
    public async Task HappySimpleWorkflow()
    {
        var services = new ServiceCollection();

        services.AddTransient<IMessageService, ConsoleMessageService>();
        services.AddTransient<Repo>();
        services.AddCharkhDande(a => a
             .AddKesho(services)
             .AddWorkflowResolver<WFRepo>(services));


        var serviceProvider = services.BuildServiceProvider();

        var actionReg = serviceProvider.GetRequiredService<IActionRegistry>();
        var conditionReg = serviceProvider.GetRequiredService<IConditionRegistry>();

        Configurator.RegisterActions(actionReg);
        Configurator.RegisterConditions(conditionReg);

        var WFfactory = serviceProvider.GetRequiredService<WorkflowFactory>();


        var myWorkFlow = WFfactory.GetGuidInstance();

        myWorkFlow.Should().NotBeNull();
        myWorkFlow.Context.Should().NotBeNull();


        var step1 = new ConditionalStep("step1");

        var step2 = new ConditionalStep("step2");

        var decision = new DecisionlStep("decision", DecisionOutputType.FORCED_XOR)
        {
            IsFirstStep = true,
        };

        decision.Routes = [
            new ConditionalRoute("r1"){
                NextStep= new NextStepMetadata(step1.Id),
                Conditions = {new ReferenceCondition(Configurator.ConditionTrue) },
                Actions = { new ReferenceAction(Configurator.ActionSendEmail) }
            },
            new ConditionalRoute("r2"){
                NextStep = new NextStepMetadata(step2.Id),
                Conditions = {new ReferenceCondition(Configurator.ConditionFalse) },
                Actions = { new ReferenceAction(Configurator.ActionSendEmail) }
            }];

        myWorkFlow.AddSteps(decision, step1, step2);

    }
    [Fact]
    public async Task ComplextFlow()
    {
        var services = new ServiceCollection();

        services.AddTransient<IMessageService, ConsoleMessageService>();
        services.AddTransient<Repo>();
        services.AddSingleton<ActionRegistry>();
        services.AddSingleton<ConditionRegistry>();
        services.AddTransient<WorkflowFactory>();

        services.AddCharkhDande(a => a
            .AddKesho(services)
            .AddWorkflowResolver<WFRepo>(services));

        var serviceProvider = services.BuildServiceProvider();

        var actionReg = serviceProvider.GetRequiredService<ActionRegistry>();
        var conditionReg = serviceProvider.GetRequiredService<ConditionRegistry>();

        Configurator.RegisterActions(actionReg);
        Configurator.RegisterConditions(conditionReg);

        var WFfactory = serviceProvider.GetRequiredService<WorkflowFactory>();

        var lambdaAction = new ReferenceAction(Configurator.ActionSendEmail);

        var monitorTask = new MonitorStep("monitor 1")
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
            IsFirstStep = true
        };


        // Resolve the main application class and run it
        var workflow = WFfactory.GetGuidInstance();

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


        var task3 = new ConditionalStep("third")
        {
            Actions = [new ReferenceAction("taskRun")]
        };
        var task2 = new ConditionalStep("second")
        {
            Routes = [ new ConditionalRoute("GoTask3R"){
        NextStep = new NextStepMetadata(task3.Id),
        Conditions = [
            new ReferenceCondition(Configurator.ConditionDocIdOdd)
            ]
    },
    ],
            Actions = [new ReferenceAction("taskRun")]
        };



        var task1 = new ConditionalStep("first")
        {
            Conditions = new List<ICondition>
        {
            new ReferenceCondition(Configurator.ConditionDocIdOdd),
            new ExpressionCondition(ctx => ctx.Get<int>("age") > 5)
        },
            Actions = new List<IAction>
        {
            new ReferenceAction(Configurator.ActionSendEmail),
            new ReferenceAction("jobTriggerKey"),
            new ReferenceAction("dispatchEventKey")
        },
            Routes = [new ConditionalRoute("GoTask2R") { NextStep = new NextStepMetadata(task2.Id) }]
        };
        monitorTask.Routes = [new ConditionalRoute("GoTask1R") { NextStep = new NextStepMetadata(task1.Id) }];
        // Run the Workflow
        workflow.Context.Set("age", 10);
        workflow.Context.Set("doc_id", 1);

        workflow.AddSteps(monitorTask, task1, task2, task3);


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
}
