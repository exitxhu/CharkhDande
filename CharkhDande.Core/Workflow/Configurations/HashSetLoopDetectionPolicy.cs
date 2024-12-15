
public class HashSetLoopDetectionPolicy : ILoopDetectionPolicy
{
    private readonly HashSet<string> _visitedSteps = new();

    public void TrackStep(string stepId)
    {
        if (!_visitedSteps.Add(stepId))
        {
            throw new InvalidOperationException($"Loop detected at step {stepId}");
        }
    }

    public void Clear()
    {
        _visitedSteps.Clear();
    }
}
