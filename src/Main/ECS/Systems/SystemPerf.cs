using System.Diagnostics;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable once CheckNamespace
// Copypaste from friflo.ecs with very minor modifications, because in the original most of the stuff is internal.
namespace Monod.ECS.Systems;

/// <summary>
/// Provide performance statistics of system execution via the system property <see cref="SystemBase.Perf"/>.
/// </summary>
public struct SystemPerf
{
    internal static readonly double StopwatchPeriodMs = 1 / (Stopwatch.Frequency / 1000d);

    #region properties
    /// <summary> Return the number of system executions. </summary>
    public int UpdateCount => updateCount;

    /// <summary> Return the duration of the last execution in milliseconds. <br/>
    /// Can be 0 in case execution time was below <see cref="Stopwatch.Frequency"/> precision. </summary>
    public float LastMs => lastTicks >= 0 ? (float)(lastTicks * StopwatchPeriodMs) : -1;

    /// <summary> Return the sum of all execution times in milliseconds. </summary>
    public float SumMs => (float)(sumTicks * StopwatchPeriodMs);

    /// <summary>Return the duration of the last execution in timer ticks. <br/>
    /// Can be 0 in case execution time was below <see cref="Stopwatch.Frequency"/> precision. </summary>
    [Browse(Never)] public long LastTicks => lastTicks;

    /// <summary> Return the sum of all execution times in timer ticks. </summary>
    [Browse(Never)] public long SumTicks => sumTicks;

    /// <summary> Return the memory allocations of the last execution in bytes. </summary>
    [Browse(Never)] public long LastMemory => lastMemory;

    /// <summary> Return the sum of memory allocations of all executions in bytes. </summary>
    [Browse(Never)] public long SumMemory => sumMemory;

    public override string ToString() => $"updates: {UpdateCount}  last: {LastMs:0.###} ms  sum: {SumMs:0.###} ms";
    #endregion

    #region fields
    [Browse(Never)] internal int updateCount;
    [Browse(Never)] internal long lastTicks;
    [Browse(Never)] internal long sumTicks;
    [Browse(Never)] internal long lastMemory;
    [Browse(Never)] internal long sumMemory;
    internal readonly long[] history;
    #endregion

    #region methods
    internal SystemPerf(int historyLength)
    {
        history = new long[historyLength];
        lastTicks = -1;
    }

    /// <summary>
    /// Return the average duration of the last <paramref name="count"/> executions in milliseconds.
    /// </summary>
    public float LastAvgMs(int count)
    {
        long[] ticks = history;
        int length = ticks.Length;
        count = Math.Min(updateCount, Math.Min(length, count));
        if (count == 0)
        {
            return -1;
        }
        long sum = 0L;
        for (int n = updateCount - count; n < updateCount; n++)
        {
            sum += ticks[n % length];
        }
        sum /= count;
        return (float)(sum * StopwatchPeriodMs);
    }
    #endregion
}

public readonly struct PerfResource
{
    public readonly long time;
    public readonly long memory;

    public PerfResource()
    {
        time = Stopwatch.GetTimestamp();
        memory = GetAllocatedBytes();
    }

    internal static long GetAllocatedBytes()
    {
        return GC.GetAllocatedBytesForCurrentThread();
    }
}

