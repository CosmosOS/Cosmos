// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Drawing;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using Cosmos.Kernel.System.Threading;
using DevKernel.Graphics;

namespace DevKernel.Diagnostics.Smp;

// Draws the SMP monitor's screen from what the sampler read:
//
//   title                                                    machine, keys
//   banner     the step the scenario is playing, in words
//   one lane per CPU
//     name, role, load     the threads on that CPU, as chips     timeline of who ran there
//   footer     counters, legend
//
// A chip is one thread, in the thread's color: bright with a border while it is on the
// CPU, dark while it waits in the run queue, hollow while it sleeps or is blocked. A chip
// glides to its place, so a thread that changes CPU is seen crossing from one lane to the
// other.
internal sealed class SmpView
{
    // A number shown as text, converted again only when it changes.
    private struct NumberText
    {
        private int _value;
        private string? _text;

        public string Of(int value)
        {
            if (_text is null || value != _value)
            {
                _value = value;
                _text = value.ToString();
            }

            return _text;
        }
    }

    private const int MarginPx = 16;

    // Between a text and what stands right under or beside it.
    private const int TextGapPx = 2;

    private const int TitleTopPx = 10;

    private const int BannerTopPx = 52;
    private const int BannerHeightPx = 88;
    private const int BannerGapPx = 10;
    private const int BannerPadPx = 18;
    private const int BannerAccentPx = 6;
    private const int BannerStepTopPx = 8;
    private const int BannerHeadlineTopPx = 28;
    private const int BannerDetailTopPx = 66;

    // Where the detail starts when it takes two lines, and the distance between them.
    private const int BannerWrappedDetailTopPx = 56;
    private const int BannerDetailPitchPx = 14;

    private const int StepMarkWidthPx = 16;
    private const int StepMarkHeightPx = 6;
    private const int StepMarkPitchPx = 22;
    private const int PlayedStepShadePercent = 45;

    private const int LanesTopPx = BannerTopPx + BannerHeightPx + BannerGapPx;
    private const int LaneGapPx = 8;
    private const int MinLaneHeightPx = 28;
    private const int MaxLaneHeightPx = 132;

    // From this height on a lane has room for the role, the queue length and large text.
    private const int TallLanePx = 84;

    // From this height on a lane has room for its name above its load bar. A lower one
    // holds them side by side, with a shorter bar.
    private const int StackedLanePx = 42;

    private const int LanePadPx = 10;
    private const int TallChipsTopPx = 32;

    private const int InfoWidthPx = 150;
    private const int LoadBarWidthPx = 84;
    private const int BesideNameLoadBarWidthPx = 36;
    private const int LoadBarHeightPx = 10;
    private const int LoadTextGapPx = 5;
    private const int BusyLoadPercent = 50;
    private const int SaturatedLoadPercent = 90;

    // The chips take this share of the screen's width, within the two bounds.
    private const int ChipsWidthDivisor = 4;
    private const int MinChipsWidthPx = 180;
    private const int MaxChipsWidthPx = 340;
    private const int ChipWidthPx = 44;
    private const int ChipHeightPx = 24;
    private const int SlimChipWidthPx = 8;
    private const int ChipGapPx = 5;
    private const int ChipLabelTopPx = 4;
    private const int QueuedShadePercent = 45;
    private const int AsleepShadePercent = 60;

    // How far the outline of a chip that has just changed CPU stands off it.
    private const int MovedOutlinePx = 2;

    // Share of the distance to its place a chip covers at each frame.
    private const double ChipEase = 0.35;

    private const int TimelineGapPx = 12;
    private const int TimelinePadPx = 6;
    private const int ColumnWidthPx = 4;
    private const int CollectionMarkPx = 4;

    private const int FooterHeightPx = 58;
    private const int FooterStatsTopPx = 6;
    private const int FooterLegendTopPx = 32;
    private const int LegendSwatchPx = 14;
    private const int FooterGapChars = 3;

    private const int QueueTextCount = 33;

    private const string KeysText = "SPACE next step | ESC exit";

    private const string MonitorLabel = "MON";

    private const string LegendOnCpu = "on the CPU";
    private const string LegendQueued = "waiting in its run queue";
    private const string LegendAsleep = "asleep or blocked";
    private const string LegendMonitor = MonitorLabel + " this monitor";

    // Two slim swatches stand before this entry, wider together than the square the
    // others are measured with: the leading space makes up the difference.
    private const string LegendOthers = " every other thread";

    // The worker whose color the legend's samples wear.
    private const int LegendSampleWorker = 3;

    // From the longest to the shortest: the first one that leaves the machine's line its place.
    private static readonly string[] s_titles =
    [
        "Cosmos OS: the scheduler across CPUs",
        "Cosmos OS scheduler",
        "Scheduler",
    ];

    private readonly Canvas _canvas;

    // Headings in the font as it is, everything else at half its size.
    private readonly PCScreenFont _font;
    private readonly SmallFont _small;
    private readonly int _width;
    private readonly int _height;

    // The CPUs that have a lane, from CPU 0 on: as many as fit at a lane's least height.
    private readonly int _laneCount;

    private readonly int _laneHeight;
    private readonly bool _tall;
    private readonly bool _stacked;

    // Where the load bar starts in a lane that holds it beside the name.
    private readonly int _besideNameX;

    private readonly int _chipsX;
    private readonly int _chipsWidth;
    private readonly int _chipsTop;
    private readonly int _chipRows;
    private readonly int _timelineX;

    private readonly string[] _cpuNames;
    private readonly string[] _cpuRoles;
    private readonly string[] _stepTexts;
    private readonly string[] _headlines;
    private readonly bool[] _headlineIsSmall;
    private readonly string[] _details;
    private readonly string[] _detailTails;
    private readonly string _title;
    private readonly string[] _percentTexts;
    private readonly string[] _queueTexts;
    private readonly string[] _workerLabels;
    private readonly string[] _poolLabels;
    private readonly string _machineText;
    private readonly string _timelineText;

    private readonly int[] _cursorX;
    private readonly int[] _cursorRow;

    private NumberText _movesText;
    private NumberText _collectionsText;
    private NumberText _threadsText;

    // Columns a CPU's timeline holds, which the sampler is sized on.
    public int Columns { get; }

    public SmpView(Canvas canvas, PCScreenFont font, int cpuCount)
    {
        _canvas = canvas;
        _font = font;
        _small = new SmallFont(font);
        _width = canvas.Width;
        _height = canvas.Height;

        // A machine with more CPUs than the screen has room for shows the first ones:
        // the others are read like them and left out of the drawing.
        int lanesHeight = _height - LanesTopPx - FooterHeightPx;
        _laneCount = Math.Clamp((lanesHeight + LaneGapPx) / (MinLaneHeightPx + LaneGapPx), 1, cpuCount);
        _laneHeight = Math.Clamp(
            (lanesHeight - LaneGapPx * (_laneCount - 1)) / _laneCount, MinLaneHeightPx, MaxLaneHeightPx);
        _tall = _laneHeight >= TallLanePx;
        _stacked = _laneHeight >= StackedLanePx;

        _chipsX = MarginPx + InfoWidthPx;
        _chipsWidth = Math.Clamp(_width / ChipsWidthDivisor, MinChipsWidthPx, MaxChipsWidthPx);
        _chipsTop = _tall ? TallChipsTopPx : (_laneHeight - ChipHeightPx) / 2;
        _chipRows = _tall
            ? Math.Max(1, (_laneHeight - TallChipsTopPx + ChipGapPx) / (ChipHeightPx + ChipGapPx))
            : 1;

        _timelineX = _chipsX + _chipsWidth + TimelineGapPx;
        Columns = Math.Max(1, (_width - MarginPx - LanePadPx - _timelineX) / ColumnWidthPx);

        _cpuNames = new string[cpuCount];
        _cpuRoles = new string[cpuCount];
        for (int cpu = 0; cpu < cpuCount; cpu++)
        {
            _cpuNames[cpu] = $"CPU {cpu}";
            _cpuRoles[cpu] = RoleOf((uint)cpu);
        }

        // The last lane's name is the longest.
        _besideNameX = MarginPx + LanePadPx + (_cpuNames[_laneCount - 1].Length + 1) * _small.Width;

        // The banner's lines, fitted once to its width: a headline too long for the font
        // takes the small one, a detail too long for one line takes two.
        int bannerTextWidth = _width - MarginPx * 2 - BannerPadPx * 2;
        int largeChars = bannerTextWidth / font.Width;
        int smallChars = bannerTextWidth / _small.Width;
        _stepTexts = new string[SmpScenario.StepCount];
        _headlines = new string[SmpScenario.StepCount];
        _headlineIsSmall = new bool[SmpScenario.StepCount];
        _details = new string[SmpScenario.StepCount];
        _detailTails = new string[SmpScenario.StepCount];
        for (int step = 0; step < SmpScenario.StepCount; step++)
        {
            _stepTexts[step] = $"STEP {step + 1} OF {SmpScenario.StepCount}   {SmpScenario.Title(step)}";

            string headline = SmpScenario.Headline(step);
            _headlineIsSmall[step] = headline.Length > largeChars;
            _headlines[step] = Fit(headline, _headlineIsSmall[step] ? smallChars : largeChars);

            string detail = SmpScenario.Detail(step);
            int cut = WrapAt(detail, smallChars);
            _details[step] = detail.Substring(0, cut);
            _detailTails[step] = Fit(detail.Substring(cut).TrimStart(), smallChars);
        }

        _percentTexts = new string[SmpSampler.PercentMax + 1];
        for (int percent = 0; percent <= SmpSampler.PercentMax; percent++)
        {
            _percentTexts[percent] = $"{percent}%";
        }

        _queueTexts = new string[QueueTextCount];
        for (int queued = 0; queued < QueueTextCount; queued++)
        {
            _queueTexts[queued] = queued == QueueTextCount - 1 ? $"{queued}+ waiting" : $"{queued} waiting";
        }

        _workerLabels = new string[SmpScenario.MaxWorkers];
        for (int worker = 0; worker < SmpScenario.MaxWorkers; worker++)
        {
            _workerLabels[worker] = $"W{worker + 1}";
        }

        _poolLabels = new string[SmpScenario.MaxPoolThreads];
        for (int pool = 0; pool < SmpScenario.MaxPoolThreads; pool++)
        {
            _poolLabels[pool] = $"P{pool + 1}";
        }

        ulong tickMs = SchedulerDiagnostics.TickPeriodNs / SchedulerDiagnostics.NanosecondsPerMillisecond;
        string cpus = _laneCount < cpuCount
            ? $"{cpuCount} CPUs, the first {_laneCount} shown"
            : SmpScenario.Count(cpuCount, "CPU");
        _machineText = $"{cpus} | {SchedulerDiagnostics.SchedulerName} policy | {tickMs} ms tick";

        int titleChars = (RightAligned(_machineText) - MarginPx * 2) / font.Width;
        _title = s_titles[s_titles.Length - 1];
        foreach (string title in s_titles)
        {
            if (title.Length <= titleChars)
            {
                _title = title;
                break;
            }
        }

        int columnMs = SmpStat.FramePeriodMs * SmpSampler.FramesPerColumn;
        _timelineText = $"timelines: the last {Columns * columnMs / SmpClock.MsPerSecond} s, {columnMs} ms a column";

        _cursorX = new int[cpuCount];
        _cursorRow = new int[cpuCount];
    }

    public void Draw(SmpSampler sampler)
    {
        _canvas.Clear(SmpPalette.Background);

        DrawHeader();
        DrawBanner();

        for (int cpu = 0; cpu < _laneCount; cpu++)
        {
            DrawLane(sampler, cpu);
        }

        DrawChips(sampler);
        DrawFooter(sampler);
    }

    private static string RoleOf(uint cpu)
    {
        bool housekeeping = KernelCpus.Housekeeping.Contains(cpu);
        bool compute = KernelCpus.Compute.Contains(cpu);

        if (housekeeping && compute)
        {
            return "every role";
        }

        if (housekeeping)
        {
            return "housekeeping";
        }

        // The compute CPUs are the online ones less the housekeeping ones: a CPU in
        // neither set never came up.
        return compute ? "compute" : "offline";
    }

    private int LaneTop(int cpu)
    {
        return LanesTopPx + cpu * (_laneHeight + LaneGapPx);
    }

    private static string Fit(string text, int maxChars)
    {
        return text.Length > maxChars ? text.Substring(0, Math.Max(0, maxChars)) : text;
    }

    // Where to cut a text that is drawn on lines of maxChars: its length when it fits on
    // one, the last space that does otherwise.
    private static int WrapAt(string text, int maxChars)
    {
        if (text.Length <= maxChars)
        {
            return text.Length;
        }

        int space = maxChars > 0 ? text.LastIndexOf(' ', maxChars) : -1;
        return space > 0 ? space : Math.Max(0, maxChars);
    }

    private void Small(string text, Color color, int x, int y)
    {
        _small.DrawString(_canvas, text, color, x, y);
    }

    private int RightAligned(string text)
    {
        return _width - MarginPx - text.Length * _small.Width;
    }

    private void DrawHeader()
    {
        _canvas.DrawString(_title, _font, SmpPalette.Text, MarginPx, TitleTopPx);

        Small(_machineText, SmpPalette.Text, RightAligned(_machineText), TitleTopPx);
        Small(KeysText, SmpPalette.Dim, RightAligned(KeysText), TitleTopPx + _small.Height);
    }

    private void DrawBanner()
    {
        int step = SmpScenario.Step;
        Color accent = SmpPalette.OfStep(step);
        int width = _width - MarginPx * 2;
        int textX = MarginPx + BannerPadPx;

        _canvas.DrawFilledRectangle(SmpPalette.Panel, MarginPx, BannerTopPx, width, BannerHeightPx);
        _canvas.DrawFilledRectangle(accent, MarginPx, BannerTopPx, BannerAccentPx, BannerHeightPx);

        Small(_stepTexts[step], accent, textX, BannerTopPx + BannerStepTopPx);

        // One mark per step at the right, the steps already played dimmed.
        int marksX = _width - MarginPx - BannerPadPx - SmpScenario.StepCount * StepMarkPitchPx;
        for (int mark = 0; mark < SmpScenario.StepCount; mark++)
        {
            Color color = mark == step ? accent
                : mark < step ? SmpPalette.Shade(accent, PlayedStepShadePercent)
                : SmpPalette.BarTrack;
            _canvas.DrawFilledRectangle(
                color, marksX + mark * StepMarkPitchPx, BannerTopPx + BannerStepTopPx + StepMarkHeightPx / 2,
                StepMarkWidthPx, StepMarkHeightPx);
        }

        if (_headlineIsSmall[step])
        {
            Small(
                _headlines[step], SmpPalette.Text, textX,
                BannerTopPx + BannerHeadlineTopPx + (_font.Height - _small.Height) / 2);
        }
        else
        {
            _canvas.DrawString(_headlines[step], _font, SmpPalette.Text, textX, BannerTopPx + BannerHeadlineTopPx);
        }

        if (_detailTails[step].Length == 0)
        {
            Small(_details[step], SmpPalette.Dim, textX, BannerTopPx + BannerDetailTopPx);
        }
        else
        {
            int detailTop = BannerTopPx + BannerWrappedDetailTopPx;
            Small(_details[step], SmpPalette.Dim, textX, detailTop);
            Small(_detailTails[step], SmpPalette.Dim, textX, detailTop + BannerDetailPitchPx);
        }
    }

    private void DrawLane(SmpSampler sampler, int cpu)
    {
        int top = LaneTop(cpu);
        int textX = MarginPx + LanePadPx;

        _canvas.DrawFilledRectangle(SmpPalette.Panel, MarginPx, top, _width - MarginPx * 2, _laneHeight);

        int barX = textX;
        int barWidth = LoadBarWidthPx;
        int barTop = top + _laneHeight - LanePadPx - LoadBarHeightPx;

        if (_tall)
        {
            _canvas.DrawString(_cpuNames[cpu], _font, SmpPalette.Text, textX, top + LanePadPx);
            Small(_cpuRoles[cpu], SmpPalette.Dim, textX, top + LanePadPx + _font.Height + TextGapPx);
        }
        else if (_stacked)
        {
            Small(_cpuNames[cpu], SmpPalette.Text, textX, top + (_laneHeight - _small.Height - LoadBarHeightPx) / 2 - TextGapPx);
        }
        else
        {
            barX = _besideNameX;
            barWidth = BesideNameLoadBarWidthPx;
            barTop = top + (_laneHeight - LoadBarHeightPx) / 2;
            Small(_cpuNames[cpu], SmpPalette.Text, textX, top + (_laneHeight - _small.Height) / 2);
        }

        int load = sampler.LoadPercent(cpu);
        Color loadColor = load < BusyLoadPercent ? SmpPalette.LoadLow
            : load < SaturatedLoadPercent ? SmpPalette.LoadBusy
            : SmpPalette.LoadSaturated;

        _canvas.DrawFilledRectangle(SmpPalette.BarTrack, barX, barTop, barWidth, LoadBarHeightPx);
        if (load > 0)
        {
            _canvas.DrawFilledRectangle(loadColor, barX, barTop, barWidth * load / SmpSampler.PercentMax, LoadBarHeightPx);
        }

        Small(
            _percentTexts[load], SmpPalette.Text,
            barX + barWidth + LoadTextGapPx, barTop - (_small.Height - LoadBarHeightPx) / 2);

        if (_tall)
        {
            int queued = Math.Min(sampler.RunQueue(cpu), QueueTextCount - 1);
            Small(_queueTexts[queued], queued > 0 ? SmpPalette.Text : SmpPalette.Dim, _chipsX, top + LanePadPx);
        }

        DrawTimeline(sampler, cpu, top);
    }

    // The newest column at the right edge, each one a stack of the threads' shares of the
    // CPU during that column, from the bottom up. What is left above the stack was idle.
    private void DrawTimeline(SmpSampler sampler, int cpu, int laneTop)
    {
        int top = laneTop + TimelinePadPx;
        int height = _laneHeight - TimelinePadPx * 2;
        int width = Columns * ColumnWidthPx;

        _canvas.DrawFilledRectangle(SmpPalette.Plot, _timelineX, top, width, height);
        _canvas.DrawFilledRectangle(SmpPalette.Gridline, _timelineX, top + height / 2, width, 1);

        for (int age = 0; age < sampler.ColumnCount; age++)
        {
            int column = sampler.ColumnAt(age);
            int x = _timelineX + width - (age + 1) * ColumnWidthPx;
            int bottom = top + height;

            int shares = sampler.ShareCount(cpu, column);
            for (int share = 0; share < shares && bottom > top; share++)
            {
                int pixels = Math.Max(1, sampler.SharePermille(cpu, column, share) * height / SmpSampler.FullPermille);
                pixels = Math.Min(pixels, bottom - top);

                _canvas.DrawFilledRectangle(
                    Color.FromArgb(sampler.ShareColor(cpu, column, share)), x, bottom - pixels, ColumnWidthPx, pixels);
                bottom -= pixels;
            }

            if (sampler.Collected(column))
            {
                _canvas.DrawFilledRectangle(
                    SmpPalette.Highlight, x, top, ColumnWidthPx - 1, CollectionMarkPx);
            }
        }
    }

    private void DrawChips(SmpSampler sampler)
    {
        SmpThread[] threads = sampler.Threads;
        int[] order = sampler.Order;

        Array.Clear(_cursorX);
        Array.Clear(_cursorRow);

        for (int i = 0; i < sampler.OrderCount; i++)
        {
            PlaceChip(ref threads[order[i]]);
        }

        // The chips on their way to another lane last, over the ones that stand still.
        for (int i = 0; i < sampler.OrderCount; i++)
        {
            ref SmpThread thread = ref threads[order[i]];
            if (thread.ChipVisible && thread.MovedFrames == 0)
            {
                DrawChip(ref thread);
            }
        }

        for (int i = 0; i < sampler.OrderCount; i++)
        {
            ref SmpThread thread = ref threads[order[i]];
            if (thread.ChipVisible && thread.MovedFrames > 0)
            {
                DrawChip(ref thread);
            }
        }
    }

    private static bool IsLabelled(SmpThreadRole role)
    {
        return role is SmpThreadRole.Worker or SmpThreadRole.Pool or SmpThreadRole.Monitor;
    }

    // Gives the chip the next free place of its CPU's lane and moves it part of the way.
    private void PlaceChip(ref SmpThread thread)
    {
        int cpu = (int)thread.Cpu;
        if (cpu >= _laneCount)
        {
            thread.ChipVisible = false;
            return;
        }

        int width = IsLabelled(thread.Role) ? ChipWidthPx : SlimChipWidthPx;
        if (_cursorX[cpu] + width > _chipsWidth - ChipGapPx)
        {
            _cursorRow[cpu]++;
            _cursorX[cpu] = 0;
        }

        if (_cursorRow[cpu] >= _chipRows)
        {
            thread.ChipVisible = false;
            return;
        }

        double targetX = _chipsX + _cursorX[cpu];
        double targetY = LaneTop(cpu) + _chipsTop + _cursorRow[cpu] * (ChipHeightPx + ChipGapPx);
        _cursorX[cpu] += width + ChipGapPx;

        if (!thread.ChipPlaced || !thread.ChipVisible)
        {
            thread.ChipX = targetX;
            thread.ChipY = targetY;
        }
        else
        {
            thread.ChipX = Ease(thread.ChipX, targetX);
            thread.ChipY = Ease(thread.ChipY, targetY);
        }

        thread.ChipPlaced = true;
        thread.ChipVisible = true;
    }

    private static double Ease(double from, double to)
    {
        double distance = to - from;
        return Math.Abs(distance) < 1 ? to : from + distance * ChipEase;
    }

    private void DrawChip(ref SmpThread thread)
    {
        bool labelled = IsLabelled(thread.Role);
        int width = labelled ? ChipWidthPx : SlimChipWidthPx;
        int x = (int)thread.ChipX;
        int y = (int)thread.ChipY;
        Color color = SmpPalette.Of(thread.Role, thread.Ordinal);
        Color ink;

        switch (thread.State)
        {
            case KernelThreadState.Running:
                _canvas.DrawFilledRectangle(color, x, y, width, ChipHeightPx);
                _canvas.DrawRectangle(SmpPalette.Highlight, x, y, width, ChipHeightPx);
                ink = SmpPalette.ChipInk;
                break;
            case KernelThreadState.Ready:
            case KernelThreadState.Created:
                _canvas.DrawFilledRectangle(SmpPalette.Shade(color, QueuedShadePercent), x, y, width, ChipHeightPx);
                ink = SmpPalette.Text;
                break;
            default:
                _canvas.DrawFilledRectangle(SmpPalette.Plot, x, y, width, ChipHeightPx);
                _canvas.DrawRectangle(SmpPalette.Shade(color, AsleepShadePercent), x, y, width, ChipHeightPx);
                ink = SmpPalette.Shade(color, AsleepShadePercent);
                break;
        }

        if (thread.MovedFrames > 0)
        {
            _canvas.DrawRectangle(
                SmpPalette.Highlight, x - MovedOutlinePx, y - MovedOutlinePx,
                width + MovedOutlinePx * 2, ChipHeightPx + MovedOutlinePx * 2);
        }

        if (!labelled)
        {
            return;
        }

        string label = thread.Role switch
        {
            SmpThreadRole.Worker => _workerLabels[thread.Ordinal % _workerLabels.Length],
            SmpThreadRole.Pool => _poolLabels[thread.Ordinal % _poolLabels.Length],
            _ => MonitorLabel,
        };

        Small(label, ink, x + (width - label.Length * _small.Width) / 2, y + ChipLabelTopPx);
    }

    private void DrawFooter(SmpSampler sampler)
    {
        int top = _height - FooterHeightPx;
        int statsY = top + FooterStatsTopPx;
        int x = MarginPx;

        x = DrawPair("threads moved to another CPU", _movesText.Of(sampler.Moves), x, statsY);
        x = DrawPair("collections", _collectionsText.Of(sampler.Collections), x, statsY);
        x = DrawPair("threads", _threadsText.Of(sampler.ThreadCount), x, statsY);
        x = DrawPair("machine load", _percentTexts[Math.Clamp(sampler.TotalLoadPercent, 0, SmpSampler.PercentMax)], x, statsY);

        if (RightAligned(_timelineText) >= x)
        {
            Small(_timelineText, SmpPalette.Dim, RightAligned(_timelineText), statsY);
        }

        DrawLegend(top + FooterLegendTopPx);
    }

    // A dim label and its value. Returns where the next pair starts.
    private int DrawPair(string label, string value, int x, int y)
    {
        Small(label, SmpPalette.Dim, x, y);
        x += (label.Length + 1) * _small.Width;

        Small(value, SmpPalette.Text, x, y);
        return x + (value.Length + FooterGapChars) * _small.Width;
    }

    private void DrawLegend(int y)
    {
        Color sample = SmpPalette.Of(SmpThreadRole.Worker, LegendSampleWorker);
        int swatchY = y + (_small.Height - LegendSwatchPx) / 2;
        int x = MarginPx;

        if (LegendFits(LegendOnCpu, x))
        {
            _canvas.DrawFilledRectangle(sample, x, swatchY, LegendSwatchPx, LegendSwatchPx);
            _canvas.DrawRectangle(SmpPalette.Highlight, x, swatchY, LegendSwatchPx, LegendSwatchPx);
            x = DrawLegendText(LegendOnCpu, x, y);
        }

        if (LegendFits(LegendQueued, x))
        {
            _canvas.DrawFilledRectangle(
                SmpPalette.Shade(sample, QueuedShadePercent), x, swatchY, LegendSwatchPx, LegendSwatchPx);
            x = DrawLegendText(LegendQueued, x, y);
        }

        if (LegendFits(LegendAsleep, x))
        {
            _canvas.DrawRectangle(
                SmpPalette.Shade(sample, AsleepShadePercent), x, swatchY, LegendSwatchPx, LegendSwatchPx);
            x = DrawLegendText(LegendAsleep, x, y);
        }

        if (LegendFits(LegendMonitor, x))
        {
            _canvas.DrawFilledRectangle(SmpPalette.Monitor, x, swatchY, LegendSwatchPx, LegendSwatchPx);
            x = DrawLegendText(LegendMonitor, x, y);
        }

        if (LegendFits(LegendOthers, x))
        {
            _canvas.DrawFilledRectangle(SmpPalette.Managed, x, swatchY, SlimChipWidthPx, LegendSwatchPx);
            _canvas.DrawFilledRectangle(
                SmpPalette.Kernel, x + SlimChipWidthPx + TextGapPx, swatchY, SlimChipWidthPx, LegendSwatchPx);
            DrawLegendText(LegendOthers, x, y);
        }
    }

    // Whether a legend entry that starts at x ends before the right margin.
    private bool LegendFits(string text, int x)
    {
        return x + LegendSwatchPx + (text.Length + 1) * _small.Width <= _width - MarginPx;
    }

    private int DrawLegendText(string text, int x, int y)
    {
        x += LegendSwatchPx + _small.Width;
        Small(text, SmpPalette.Dim, x, y);
        return x + (text.Length + FooterGapChars) * _small.Width;
    }
}
