
public class ConfigurableLoopDetectionPolicy : ILoopDetectionPolicy
{
    private readonly Dictionary<string, int> _stepCounts = new();
    private int _maxIterations = 1;
    private bool _throw = false;
    public void Configure(int maxIterations, bool throwExcption = true)
    {
        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Maximum iterations must be greater than 0.");
        }
        _throw = throwExcption;
        _maxIterations = maxIterations;
    }

    public void TrackStep(string stepId)
    {
        if (!_stepCounts.ContainsKey(stepId))
        {
            _stepCounts[stepId] = 0;
        }

        _stepCounts[stepId]++;

        if (_stepCounts[stepId] > _maxIterations && _throw)
        {
            throw new InvalidOperationException($"Step {stepId} exceeded the maximum allowed iterations of {_maxIterations}.");
        }
    }

    public void Clear()
    {
        _stepCounts.Clear();
    }
}
