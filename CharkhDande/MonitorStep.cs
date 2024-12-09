// See https://aka.ms/new-console-template for more information
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
