using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharkhDande.Kesho;

public class WorkflowRegistry(WorkflowFactory factory)
{
    private readonly Dictionary<string, HashSet<(string WorkflowId, string StepId)>> _eventSubscriptions = new();

    public void RegisterStep(string workflowId, string stepId, string eventKey)
    {
        if (_eventSubscriptions.TryGetValue(eventKey, out var values))
            values.Add((workflowId, stepId));
        else _eventSubscriptions[eventKey] = [(workflowId, stepId)];

        Console.WriteLine($"Registered step {stepId} of workflow {workflowId} to event {eventKey}");
    }

    public async void TriggerEvent(string eventKey, object eventData)
    {
        if (!_eventSubscriptions.TryGetValue(eventKey, out var subscriptions) || subscriptions is null)
            return;

        while (subscriptions.Count > 0)
        {
            var subscription = subscriptions.First();

            Console.WriteLine($"Triggered event {eventKey} for workflow {subscription.WorkflowId}");

            // Reconstruct workflow

            var workflow = await factory.FetchAsync(subscription.WorkflowId);

            var step = workflow.GetStep(subscription.StepId);
            // Inject event data into the workflow context
            workflow.Context.Set(((EventListenerStep)step!)._eventDateKey(), eventData);

            // Resume workflow
            var check = workflow?.Next() ?? false;

            // Cleanup if necessary
            if (check)
                Cleanup(eventKey, subscription);
        }
    }

    public void Cleanup(string eventKey, (string WorkflowId, string StepId) subscription)
    {
        // Remove any subscriptions and delete persisted workflow state
        _eventSubscriptions[eventKey].Remove(subscription);
        Console.WriteLine($"Cleaned up workflow {subscription.WorkflowId}");
    }
}
