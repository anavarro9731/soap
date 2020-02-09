namespace Soap.Utility.Functions.Operations
{
    using System;
    using System.Diagnostics;

    public static class StopwatchOps
    {
        public static TimeSpan? CalculateLatency(long? startStopwatchTimestamp, long? stopStopwatchTimestamp)
        {
            if (!startStopwatchTimestamp.HasValue || !stopStopwatchTimestamp.HasValue)
            {
                return null;
            }
            return CalculateLatency(startStopwatchTimestamp.Value, stopStopwatchTimestamp.Value);
        }

        public static TimeSpan? CalculateLatency(long startStopwatchTimestamp, long stopStopwatchTimestamp)
        {
            return CalculateLatency(startStopwatchTimestamp, stopStopwatchTimestamp, Stopwatch.IsHighResolution, Stopwatch.Frequency);
        }

        // See http://referencesource.microsoft.com/#System/services/monitoring/system/diagnosticts/Stopwatch.cs
        public static TimeSpan? CalculateLatency(long startStopwatchTimestamp, long stopStopwatchTimestamp, bool isHighResolution, long frequency)
        {
            TimeSpan? latency = null;
            if (startStopwatchTimestamp > 0 && stopStopwatchTimestamp > 0)
            {
                var elapsedTicks = stopStopwatchTimestamp - startStopwatchTimestamp;
                if (elapsedTicks >= 0)
                {
                    // When measuring small time periods the elapsed can calculate
                    // to be a negative value.  This is due to bugs in the basic 
                    // input/output system (BIOS) or the hardware abstraction layer
                    // (HAL) on machines with variable-speed CPUs (e.g. Intel SpeedStep).

                    if (isHighResolution)
                    {
                        const long TicksPerMillisecond = 10000;
                        const long TicksPerSecond = TicksPerMillisecond * 1000;
                        double tickFrequency = TicksPerSecond;
                        tickFrequency /= frequency;

                        var highResElapsedTicks = elapsedTicks;

                        // Convert high resolution perf counter to DateTime ticks
                        double dateTimeTicks = highResElapsedTicks;
                        dateTimeTicks *= tickFrequency;
                        elapsedTicks = unchecked((long)dateTimeTicks);
                    }

                    latency = TimeSpan.FromTicks(elapsedTicks);
                }
            }
            return latency;
        }

        public static TimeSpan? CalculateLatencyTilNow(long startStopwatchTimestamp, out long stopStopwatchTimestamp)
        {
            TimeSpan? latency = null;
            if (startStopwatchTimestamp > 0)
            {
                stopStopwatchTimestamp = GetStopwatchTimestamp();
                latency = CalculateLatency(startStopwatchTimestamp, stopStopwatchTimestamp);
            }
            else
            {
                stopStopwatchTimestamp = 0;
            }
            return latency;
        }

        public static TimeSpan? CalculateLatencyTilNow(long startStopwatchTimestamp)
        {
            TimeSpan? latency = null;
            if (startStopwatchTimestamp > 0)
            {
                var stopStopwatchTimestamp = GetStopwatchTimestamp();
                latency = CalculateLatency(startStopwatchTimestamp, stopStopwatchTimestamp);
            }
            return latency;
        }

        public static long GetStopwatchTimestamp()
        {
            return Stopwatch.GetTimestamp();
        }
    }
}
