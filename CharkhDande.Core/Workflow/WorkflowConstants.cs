public sealed class WorkflowConstants
{
    public const string WORKFLOW_ID = "wfid";

    public static IEnumerable<string> GetPredefinedKeys()
    {
        yield return WORKFLOW_ID;
    }
    public static bool NewKeyAcceptable(string key) => !GetPredefinedKeys().Contains(key);
}