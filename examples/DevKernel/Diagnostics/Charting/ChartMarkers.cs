using System.Drawing;

namespace DevKernel.Diagnostics.Charting;

// Glyph drawn under the horizontal axis for a flagged sample.
public enum ChartMarkerShape
{
    Triangle,
    Square,
    Diamond,
    Cross,
    Tick
}

// Event track drawn alongside the series: one flag per sample saying whether the
// event happened while that sample was taken. The chart shows each flagged sample
// as a vertical rule plus the track's glyph under the horizontal axis.
public sealed class ChartMarkers
{
    private readonly SampleRing<bool> _flags;

    public string Label { get; }

    public Color Color { get; }

    public ChartMarkerShape Shape { get; }

    public int Count => _flags.Count;

    public bool this[int index] => _flags[index];

    public ChartMarkers(
        string label,
        Color color,
        int capacity,
        ChartMarkerShape shape = ChartMarkerShape.Triangle)
    {
        _flags = new SampleRing<bool>(capacity);
        Label = label;
        Color = color;
        Shape = shape;
    }

    public void Add(bool occurred) => _flags.Add(occurred);

    public void Clear() => _flags.Clear();
}
