namespace Asos.OpenTelemetry.AspNetCore.Sampling.Tail;

internal class SamplingDecisionResult
{
    public bool ShouldSample { get; set; }
    
    public double SampleRate  { get; set; }
} 