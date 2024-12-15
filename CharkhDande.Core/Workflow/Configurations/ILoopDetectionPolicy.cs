
public interface ILoopDetectionPolicy
{
    void TrackStep(string stepId);
    void Clear();
}
