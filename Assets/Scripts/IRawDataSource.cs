using UnityEngine;

public interface IRawDataSource
{
    ComputeBuffer PixelIndexValuesBuffer { get; }
    void SetNextStep();
    int CurrentStepIndex { get; }
    int TotalSteps { get; }
    void Dispose();
}