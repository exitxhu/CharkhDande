﻿using Microsoft.Extensions.DependencyInjection;

namespace CharkhDande.Tests;

public static class Configurator
{
    public const string ActionSendEmail = "sendEmailOnCondition";
    public const string ActionJob = "jobAction";
    public const string ActionTimeOutJob = "jobTimeOutAction";
    public const string ActionRunTask = "taskRun";
    public const string ActionTrigger = "jobTriggerKey";
    public const string ActionDispatchEvent = "dispatchEventKey";
    public const string ConditionDocIdEven = "docIdEven";

    public static void RegisterConditions()
    {
        ConditionRegistry.Register(ConditionDocIdEven, (ctx, init) =>
        {
            var id = ctx.Get<int>("doc_id");
            Repo? repo = ctx.ServiceProvider.GetRequiredService<Repo>();
            var ent = repo.Get(id);
            return ent.id % 2 == 0;
        });
    }

    public static void RegisterActions()
    {
        ActionRegistry.Register(ActionSendEmail, (ctx, init) =>
        {
            Console.WriteLine("sendEmail");
            var T = Random.Shared.Next(0, 100) % 3 == 2;
            if (T)
                ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("email someone");
        });

        ActionRegistry.Register(ActionJob, (ctx, init) => Console.WriteLine("Job completed successfully."));
        ActionRegistry.Register(ActionTimeOutJob, (ctx, init) => Console.WriteLine("Job completion timed out."));

        ActionRegistry.Register(ActionRunTask, (ctx, init) => ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage($"task {init.InitiatorId} runs"));

        ActionRegistry.Register(ActionTrigger, (ctx, init) =>
               ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("trigger a job"));
        ActionRegistry.Register(ActionDispatchEvent, (ctx, init) =>
                ctx.ServiceProvider.GetRequiredService<IMessageService>().SendMessage("dispatch an event"));
    }

}