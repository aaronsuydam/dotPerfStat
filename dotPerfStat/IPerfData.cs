

public interface IStreamingCorePerfData
{
    public DateTime Timestamp { get; }
    public i8 CoreNumber { get; }
    public f32 Frequency { get; }
    public u128 Cycles { get; }
    public u64 UtilizationPercent { get; }
    public u64 UtilizationPercentUser { get; }
    public u64 UtilizationPercentKernel { get; }
    public bool IsEmpty();
}