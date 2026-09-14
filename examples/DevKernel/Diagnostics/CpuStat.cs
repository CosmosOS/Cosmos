using System.Diagnostics;
using System.Drawing;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using DevKernel.Diagnostics.Charting;
using DevKernel.Graphics;
using SysThread = System.Threading.Thread;

namespace DevKernel.Diagnostics;

// Full-screen CPU monitor: ramps a synthetic load up and down, plots the resulting
// CPU usage against the number of stress threads, and lists the scheduler threads.
//
// Exits on ESC.
internal static class CpuStat
{
    // Sawtooth driving the number of stress threads: one step up every TargetStepMs
    // until the pool is full, then one step down until it is empty.
    private struct LoadRamp
    {
        private long _lastStepTimestamp;
        private int _step;

        public static LoadRamp Start()
        {
            return new LoadRamp
            {
                _lastStepTimestamp = Stopwatch.GetTimestamp(),
                _step = +1
            };
        }

        public int Target { get; private set; }

        public bool IsRising => _step > 0;

        public void Advance()
        {
            long stepTicks = Stopwatch.Frequency * TargetStepMs / 1000;
            long now = Stopwatch.GetTimestamp();

            if (now - _lastStepTimestamp < stepTicks)
            {
                return;
            }

            _lastStepTimestamp = now;
            Target += _step;

            if (Target >= CpuStressPool.MaxThreads)
            {
                Target = CpuStressPool.MaxThreads;
                _step = -1;
            }
            else if (Target <= 0)
            {
                Target = 0;
                _step = +1;
            }
        }
    }

    // Turns the scheduler's cumulative busy-time counter into a percentage: how much of
    // the CPU time available since the previous sample was actually spent working.
    internal struct CpuUsageSampler
    {
        private long _lastTimestamp;
        private ulong _lastBusyNs;

        public static CpuUsageSampler Start()
        {
            return new CpuUsageSampler
            {
                _lastTimestamp = Stopwatch.GetTimestamp(),
                _lastBusyNs = SchedulerInfo.BusyCpuTimeNs
            };
        }

        public double Peak { get; private set; }

        public double Current { get; private set; }

        public double Sample()
        {
            long timestamp = Stopwatch.GetTimestamp();
            ulong busyNs = SchedulerInfo.BusyCpuTimeNs;

            long elapsedTicks = timestamp - _lastTimestamp;
            uint cpuCount = SchedulerInfo.CpuCount;

            if (elapsedTicks > 0 && cpuCount > 0)
            {
                ulong busyDelta = busyNs >= _lastBusyNs ? busyNs - _lastBusyNs : 0UL;
                double availableNs = elapsedTicks * 1_000_000_000.0 / Stopwatch.Frequency * cpuCount;

                if (availableNs > 0)
                {
                    Current = Clamp(busyDelta * 100.0 / availableNs);
                    if (Current > Peak)
                    {
                        Peak = Current;
                    }
                }
            }

            _lastTimestamp = timestamp;
            _lastBusyNs = busyNs;

            return Current;
        }

        private static double Clamp(double percent)
        {
            if (percent < 0)
            {
                return 0;
            }

            return percent > 100 ? 100 : percent;
        }
    }

    private const int FrameDelayMs = 100;
    private const int TargetStepMs = 500;
    private const int HistorySize = 600;
    private const int ShutdownBudgetMs = 2000;

    private const int ChartHeightCapPx = 180;
    private const double UsageAxisMax = 100;
    private const int BusyUsagePercent = 50;
    private const int SaturatedUsagePercent = 80;

    private const int ThreadColumnChars = 18;

    private static readonly ChartSeries s_usageSeries =
        new ChartSeries("CPU %", Color.LimeGreen, HistorySize);

    private static readonly ChartSeries s_stressSeries =
        new ChartSeries("stress 0.." + CpuStressPool.MaxThreads, Color.Cyan, HistorySize);

    private static readonly ChartSeries[] s_series = [s_usageSeries, s_stressSeries];

    public static void Run()
    {
        if (!SchedulerInfo.IsSupported)
        {
            Console.WriteLine("cpustat: scheduler disabled (set CosmosEnableScheduler=true).");
            return;
        }

        Canvas canvas = Canvas.GetFullScreen();
        PCScreenFont font = PCScreenFont.DefaultFont;

        s_usageSeries.Clear();
        s_stressSeries.Clear();
        CpuStressPool.Reset();

        CpuUsageSampler sampler = CpuUsageSampler.Start();
        LoadRamp ramp = LoadRamp.Start();

        while (!EscapePressed())
        {
            ramp.Advance();
            CpuStressPool.SetThreadCount(ramp.Target);

            sampler.Sample();
            int activeThreads = CpuStressPool.Active;

            s_usageSeries.Add(sampler.Current);
            s_stressSeries.Add(activeThreads * UsageAxisMax / CpuStressPool.MaxThreads);

            Render(canvas, font, sampler, ramp, activeThreads);

            canvas.Display();
            SysThread.Sleep(FrameDelayMs);
        }

        CpuStressPool.ShutdownAndWait(ShutdownBudgetMs);
        Console.Clear();
    }

    private static bool EscapePressed()
    {
        return Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape;
    }

    private static void Render(
        Canvas canvas,
        PCScreenFont font,
        CpuUsageSampler sampler,
        LoadRamp ramp,
        int activeThreads)
    {
        int lineHeight = OverlayLayout.LineHeight(font);
        int x = OverlayLayout.TextMarginPx;
        int contentWidth = canvas.Width - x * 2;
        int row = OverlayLayout.TextMarginPx;

        canvas.Clear(Color.Black);

        TextRenderer.DrawTruncated(
            canvas, font, "CPU Utilization Monitor - ESC to exit", Color.LightGray, x, row, contentWidth);
        row += lineHeight;

        row = DrawUsageBar(canvas, font, x, row, contentWidth, sampler);
        row = DrawPoolStats(canvas, font, x, row, contentWidth, ramp, activeThreads);
        row = DrawUsageChart(canvas, font, x, row, canvas.Width - x);

        DrawThreadTable(canvas, font, x, row, canvas.Width - x, canvas.Height - row);
    }

    // Percentage headline plus a bar filled proportionally to the current usage.
    private static int DrawUsageBar(
        Canvas canvas,
        PCScreenFont font,
        int x,
        int y,
        int maxWidth,
        CpuUsageSampler sampler)
    {
        int lineHeight = OverlayLayout.LineHeight(font);
        Color usageColor = UsageColor(sampler.Current);

        string usageText = "CPU: " + (int)sampler.Current + "%";
        canvas.DrawString(usageText, font, usageColor, x, y);

        string peakText = "Peak: " + (int)sampler.Peak + "%";
        int peakX = x + (usageText.Length + 2) * font.Width;
        if (peakX + peakText.Length * font.Width <= x + maxWidth)
        {
            canvas.DrawString(peakText, font, Color.LightGray, peakX, y);
        }

        y += lineHeight;

        int barHeight = lineHeight - OverlayLayout.LineSpacingPx;
        canvas.DrawFilledRectangle(Color.FromArgb(40, 40, 40), x, y, maxWidth, barHeight);

        int filledWidth = (int)(maxWidth * sampler.Current / UsageAxisMax);
        if (filledWidth > 0)
        {
            canvas.DrawFilledRectangle(usageColor, x, y, filledWidth, barHeight);
        }

        return y + barHeight + OverlayLayout.LineSpacingPx;
    }

    private static Color UsageColor(double usagePercent)
    {
        if (usagePercent < BusyUsagePercent)
        {
            return Color.LimeGreen;
        }

        return usagePercent < SaturatedUsagePercent ? Color.Yellow : Color.OrangeRed;
    }

    private static int DrawPoolStats(
        Canvas canvas,
        PCScreenFont font,
        int x,
        int y,
        int maxWidth,
        LoadRamp ramp,
        int activeThreads)
    {
        string trend = ramp.IsRising ? "+" : "-";

        string stats = TextRenderer.LongestThatFits(
            font,
            maxWidth,
            "target=" + ramp.Target + "  alive=" + CpuStressPool.Alive + "  leaving=" + CpuStressPool.ExitRequests
                + "  active=" + activeThreads + "  trend=" + trend,
            "tgt=" + ramp.Target + " alive=" + CpuStressPool.Alive + " act=" + activeThreads + " " + trend,
            "t=" + ramp.Target + " a=" + activeThreads);

        canvas.DrawString(stats, font, Color.LightGray, x, y);

        return y + OverlayLayout.LineHeight(font);
    }

    private static int DrawUsageChart(Canvas canvas, PCScreenFont font, int x, int y, int width)
    {
        int lineHeight = OverlayLayout.LineHeight(font);
        int footerHeight = lineHeight * OverlayLayout.SectionBreakRowCount;
        int available = canvas.Height - y - footerHeight;
        int chartHeight = available > ChartHeightCapPx ? ChartHeightCapPx : available;

        return LineChart.Draw(
            canvas,
            font,
            s_series,
            new Rectangle(x, y, width, chartHeight),
            UsageAxisMax,
            "100%",
            "window " + s_usageSeries.Count * FrameDelayMs / 1000 + "s");
    }

    // Scheduler thread registry, laid out in as many columns as the width allows.
    private static void DrawThreadTable(
        Canvas canvas,
        PCScreenFont font,
        int x,
        int y,
        int maxWidth,
        int maxHeight)
    {
        int lineHeight = OverlayLayout.LineHeight(font);
        int threadCount = SchedulerInfo.ThreadCount;

        if (threadCount <= 0 || maxHeight < lineHeight * 2)
        {
            return;
        }

        TextRenderer.DrawTruncated(
            canvas,
            font,
            "Scheduler threads (" + threadCount + " live):",
            Color.LightGray,
            x,
            y,
            maxWidth);

        int tableY = y + lineHeight;

        int columnWidth = ThreadColumnChars * font.Width;
        if (columnWidth > maxWidth)
        {
            columnWidth = maxWidth;
        }

        int columns = maxWidth / columnWidth;
        int rows = (maxHeight - lineHeight) / lineHeight;
        int capacity = columns * rows;

        int drawn = 0;
        for (int slot = 0; slot < SchedulerInfo.ThreadSlotCount && drawn < capacity; slot++)
        {
            if (!SchedulerInfo.TryGetThreadInSlot(slot, out KernelThreadInfo thread))
            {
                continue;
            }

            TextRenderer.DrawTruncated(
                canvas,
                font,
                FormatThread(thread),
                Color.White,
                x + drawn % columns * columnWidth,
                tableY + drawn / columns * lineHeight,
                columnWidth - font.Width);

            drawn++;
        }
    }

    private static string FormatThread(KernelThreadInfo thread)
    {
        string kind = thread.IsIdle ? "idle"
                    : thread.IsManaged ? "mgd"
                    : "krn";

        string state = thread.State switch
        {
            KernelThreadState.Running => "RUN",
            KernelThreadState.Ready => "RDY",
            KernelThreadState.Blocked => "BLK",
            KernelThreadState.Sleeping => "SLP",
            KernelThreadState.Dead => "DED",
            KernelThreadState.Created => "NEW",
            _ => "???"
        };

        return "T" + thread.Id + " " + kind + " " + state + " " + FormatRuntime(thread.TotalRuntimeNs);
    }

    private static string FormatRuntime(ulong nanoseconds)
    {
        ulong milliseconds = nanoseconds / 1_000_000UL;
        if (milliseconds < 1000)
        {
            return milliseconds + "ms";
        }

        ulong seconds = milliseconds / 1000UL;
        if (seconds < 60)
        {
            return seconds + "." + milliseconds % 1000UL / 100UL + "s";
        }

        ulong minutes = seconds / 60UL;
        if (minutes < 60)
        {
            return minutes + "m";
        }

        return minutes / 60UL + "h";
    }
}
