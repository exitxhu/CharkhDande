// See https://aka.ms/new-console-template for more information
public interface ILoopDetectionPolicy
{
    void TrackStep(string stepId);
    void Clear();
}
