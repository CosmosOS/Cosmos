using System.Drawing;
using Cosmos.Kernel.System.Graphics;
using Cosmos.Kernel.System.Graphics.Fonts;
using DevKernel.Graphics;

namespace DevKernel.Diagnostics.Charting;

// Draws time series as polylines inside a rectangular plot area:
// the samples of a series are spread evenly from the left edge (oldest) to the
// right edge (newest), and their value is mapped to a height between the bottom
// of the plot area (0) and its top (the axis maximum).
//
// Optional marker tracks flag samples during which some event happened; each
// flagged sample gets a vertical rule across the plot area and the track's glyph
// under the horizontal axis, in a lane of its own.
public static class LineChart
{
    // Below that height the plot area is too flat to be readable.
    public const int MinimumPlotHeightPx = 60;

    // Pass this as the axis maximum to scale the chart to its own tallest sample.
    public const double AutoScale = 0;

    private const int GridlineStepPercent = 25;
    private const int SwatchGapPx = 4;

    private const int MarkerSizePx = 5;

    // Each marker track gets its own lane under the axis, so glyphs coming from two
    // tracks can never land on top of each other.
    private const int MarkerLaneHeightPx = MarkerSizePx + OverlayLayout.LineSpacingPx;

    // A glyph spans at most MarkerSizePx * 2 - 1 pixels; keep at least its full width
    // between two of them so consecutive events stay countable.
    private const int MarkerSpacingPx = MarkerSizePx * 2;

    // Length of one dash of a vertical rule. Tracks take turns along the rule, so a
    // single track draws a solid line and several tracks stay visible side by side.
    private const int MarkerDashPx = 3;

    private static readonly Color s_plotBackground = Color.FromArgb(15, 15, 15);
    private static readonly Color s_gridline = Color.FromArgb(45, 45, 45);
    private static readonly Color s_labelColor = Color.LightGray;

    // Draws the plot area, the marker gutter and the legend row. Returns the Y
    // coordinate just below everything that was drawn.
    public static int Draw(
        Canvas canvas,
        PCScreenFont font,
        ChartSeries[] series,
        Rectangle zone,
        double axisMax = AutoScale,
        string axisMaxLabel = "",
        string note = "",
        ChartMarkers[]? markers = null)
    {
        int height = zone.Height, width = zone.Width, x = zone.X, y = zone.Y;

        int trackCount = markers == null ? 0 : markers.Length;

        int lineHeight = OverlayLayout.LineHeight(font);
        int gutterHeight = trackCount * MarkerLaneHeightPx;
        int plotHeight = height - gutterHeight - lineHeight - OverlayLayout.LineSpacingPx;

        if (width <= 0 || plotHeight < MinimumPlotHeightPx)
        {
            return y;
        }

        if (axisMax <= AutoScale)
        {
            axisMax = PeakOf(series);
        }

        DrawPlotArea(canvas, font, series, x, y, width, plotHeight, axisMax, axisMaxLabel, markers);

        int legendY = y + plotHeight + gutterHeight + OverlayLayout.LineSpacingPx;
        DrawLegend(canvas, font, series, x, legendY, width, note, markers);

        return legendY + lineHeight;
    }

    private static void DrawPlotArea(
        Canvas canvas,
        PCScreenFont font,
        ChartSeries[] series,
        int x,
        int y,
        int width,
        int height,
        double axisMax,
        string axisMaxLabel,
        ChartMarkers[]? markers)
    {
        canvas.DrawFilledRectangle(s_plotBackground, x, y, width, height);

        for (int percent = GridlineStepPercent; percent < 100; percent += GridlineStepPercent)
        {
            int gridY = y + height - 1 - (height - 1) * percent / 100;
            canvas.DrawLine(s_gridline, x, gridY, x + width - 1, gridY);
        }

        // Markers go under the curves so they never hide a value.
        if (markers != null)
        {
            DrawMarkers(canvas, markers, x, y, width, height);
        }

        if (axisMaxLabel.Length > 0)
        {
            int labelX = x + width - axisMaxLabel.Length * font.Width - OverlayLayout.LineSpacingPx;
            TextRenderer.DrawTruncated(canvas, font, axisMaxLabel, s_labelColor, labelX, y, width);
        }

        foreach (ChartSeries line in series)
        {
            DrawPolyline(canvas, line, x, y, width, height, axisMax);
        }
    }

    private static void DrawPolyline(
        Canvas canvas,
        ChartSeries series,
        int x,
        int y,
        int width,
        int height,
        double axisMax)
    {
        if (series.Count < 2)
        {
            return;
        }

        int previousX = 0;
        int previousY = 0;

        for (int i = 0; i < series.Count; i++)
        {
            int sampleX = SampleToX(i, series.Count, x, width);
            int sampleY = ValueToY(series[i], axisMax, y, height);

            if (i > 0)
            {
                canvas.DrawLine(series.Color, previousX, previousY, sampleX, sampleY);
            }

            previousX = sampleX;
            previousY = sampleY;
        }
    }

    private static void DrawMarkers(
        Canvas canvas,
        ChartMarkers[] markers,
        int x,
        int y,
        int width,
        int height)
    {
        int gutterY = y + height + OverlayLayout.LineSpacingPx;

        for (int lane = 0; lane < markers.Length; lane++)
        {
            ChartMarkers track = markers[lane];
            if (track.Count < 2)
            {
                continue;
            }

            int laneY = gutterY + lane * MarkerLaneHeightPx;

            // Several samples can share a pixel column once the window is full, and
            // consecutive columns are closer than a glyph is wide: draw one rule per
            // column at most, and one glyph per MarkerSpacingPx at most. Both cursors
            // start one full spacing left of the plot area so the first marker always
            // passes (int.MinValue would overflow the subtraction below).
            int lastRuleX = x - MarkerSpacingPx;
            int lastGlyphX = x - MarkerSpacingPx;

            for (int i = 0; i < track.Count; i++)
            {
                if (!track[i])
                {
                    continue;
                }

                int markerX = SampleToX(i, track.Count, x, width);

                if (markerX == lastRuleX)
                {
                    continue;
                }

                lastRuleX = markerX;
                DrawDashedRule(canvas, track.Color, markerX, y, height, lane, markers.Length);

                if (markerX - lastGlyphX < MarkerSpacingPx)
                {
                    continue;
                }

                lastGlyphX = markerX;
                DrawMarkerGlyph(canvas, track.Color, track.Shape, markerX, laneY, x, x + width - 1);
            }
        }
    }

    // Vertical rule split into dashes, one lane out of laneCount being drawn. With a
    // single lane the dashes touch and the rule is solid; with more, rules sharing a
    // column interleave instead of hiding each other.
    private static void DrawDashedRule(
        Canvas canvas,
        Color color,
        int columnX,
        int y,
        int height,
        int lane,
        int laneCount)
    {
        int period = MarkerDashPx * laneCount;

        for (int offset = lane * MarkerDashPx; offset < height; offset += period)
        {
            int end = offset + MarkerDashPx - 1;
            if (end >= height)
            {
                end = height - 1;
            }
            canvas.DrawLine(color, columnX, y + offset, columnX, y + end);
        }
    }

    // Marker glyph of MarkerSizePx rows, centered on centerX and clipped to the
    // [leftBound, rightBound] columns so a marker sitting on an edge of the plot area
    // is cut instead of being shifted away from its sample.
    private static void DrawMarkerGlyph(
        Canvas canvas,
        Color color,
        ChartMarkerShape shape,
        int centerX,
        int topY,
        int leftBound,
        int rightBound)
    {
        int lastRow = MarkerSizePx - 1;
        int middleRow = lastRow / 2;

        for (int row = 0; row < MarkerSizePx; row++)
        {
            int rowY = topY + row;

            switch (shape)
            {
                case ChartMarkerShape.Triangle:
                    DrawSpan(canvas, color, centerX - row, centerX + row, rowY, leftBound, rightBound);
                    break;

                case ChartMarkerShape.Square:
                    DrawSpan(canvas, color, centerX - middleRow, centerX + middleRow, rowY, leftBound, rightBound);
                    break;

                case ChartMarkerShape.Diamond:
                    int halfWidth = row <= middleRow ? row : lastRow - row;
                    DrawSpan(canvas, color, centerX - halfWidth, centerX + halfWidth, rowY, leftBound, rightBound);
                    break;

                case ChartMarkerShape.Cross:
                    int arm = row < middleRow ? middleRow - row : row - middleRow;
                    DrawSpan(canvas, color, centerX - arm, centerX - arm, rowY, leftBound, rightBound);
                    DrawSpan(canvas, color, centerX + arm, centerX + arm, rowY, leftBound, rightBound);
                    break;

                default:
                    DrawSpan(canvas, color, centerX, centerX, rowY, leftBound, rightBound);
                    break;
            }
        }
    }

    // Horizontal run of pixels, clipped to the given columns.
    private static void DrawSpan(
        Canvas canvas,
        Color color,
        int left,
        int right,
        int y,
        int leftBound,
        int rightBound)
    {
        if (left < leftBound)
        {
            left = leftBound;
        }
        if (right > rightBound)
        {
            right = rightBound;
        }
        if (left <= right)
        {
            canvas.DrawLine(color, left, y, right, y);
        }
    }

    private static int SampleToX(int index, int count, int x, int width)
    {
        return count < 2 ? x : x + (int)((long)index * (width - 1) / (count - 1));
    }

    private static int ValueToY(double value, double axisMax, int y, int height)
    {
        int bottom = y + height - 1;

        if (axisMax <= 0 || value <= 0)
        {
            return bottom;
        }
        if (value >= axisMax)
        {
            return y;
        }

        return bottom - (int)(value * (height - 1) / axisMax);
    }

    // Draws a color swatch plus its label for each series, then one entry per marker
    // track showing its glyph, then the note. Anything that would overflow maxWidth is dropped instead of being
    // clipped mid-word.
    private static void DrawLegend(
        Canvas canvas,
        PCScreenFont font,
        ChartSeries[] series,
        int x,
        int y,
        int maxWidth,
        string note,
        ChartMarkers[]? markers)
    {
        int swatchSize = font.Height - SwatchGapPx;
        int cursor = x;
        int remaining = maxWidth;

        foreach (ChartSeries line in series)
        {
            int entryWidth = EntryWidth(font, swatchSize, line.Label);
            if (entryWidth > remaining)
            {
                return;
            }

            canvas.DrawFilledRectangle(line.Color, cursor, y + OverlayLayout.LineSpacingPx, swatchSize, swatchSize);
            canvas.DrawString(line.Label, font, s_labelColor, cursor + swatchSize + SwatchGapPx, y);

            cursor += entryWidth;
            remaining -= entryWidth;
        }

        if (markers != null)
        {
            foreach (ChartMarkers track in markers)
            {
                int entryWidth = EntryWidth(font, swatchSize, track.Label);
                if (entryWidth > remaining)
                {
                    return;
                }

                DrawMarkerGlyph(
                    canvas,
                    track.Color,
                    track.Shape,
                    cursor + swatchSize / 2,
                    y + OverlayLayout.LineSpacingPx,
                    cursor,
                    cursor + swatchSize);
                canvas.DrawString(track.Label, font, s_labelColor, cursor + swatchSize + SwatchGapPx, y);

                cursor += entryWidth;
                remaining -= entryWidth;
            }
        }

        if (note.Length > 0 && note.Length * font.Width <= remaining)
        {
            canvas.DrawString(note, font, s_labelColor, cursor, y);
        }
    }

    private static int EntryWidth(PCScreenFont font, int swatchSize, string label)
    {
        return swatchSize + SwatchGapPx + (label.Length + 2) * font.Width;
    }

    private static double PeakOf(ChartSeries[] series)
    {
        double peak = 0;
        foreach (ChartSeries line in series)
        {
            double linePeak = line.Peak();
            if (linePeak > peak)
            {
                peak = linePeak;
            }
        }
        return peak;
    }
}
