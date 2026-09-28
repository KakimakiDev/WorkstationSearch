using System.Diagnostics;

namespace WorkstationSearch
{
    internal sealed class RebuildTiming
    {
        private static long lastReport;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private double restoreMs;
        private double captureStartMs;
        internal void Restored() => restoreMs = clock.Elapsed.TotalMilliseconds;
        internal void BeginCapture() => captureStartMs = clock.Elapsed.TotalMilliseconds;
        internal void Finish(bool reclaim)
        {
            double searchMs = restoreMs + clock.Elapsed.TotalMilliseconds - captureStartMs;
            double otherMs = captureStartMs - restoreMs;
            long now = Stopwatch.GetTimestamp();
            if ((searchMs < 8 && otherMs < 30) ||
                (lastReport != 0 && (now - lastReport) / (double)Stopwatch.Frequency < 5)) return;
            lastReport = now;
            Plugin.ReportSlowRebuild($"Slow {(reclaim ? "reclaim" : "recipe")} list rebuild: " +
                $"{searchMs:F1} ms Workstation Search; {otherMs:F1} ms game/other mods.");
        }
    }
}
