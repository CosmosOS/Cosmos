using System.Drawing;

namespace DevKernel.Diagnostics.Charting;

// One line of a chart: a label, a color, and the samples to plot.
public sealed class ChartSeries
{
    private readonly SampleRing<double> _samples;

    public string Label { get; }

    public Color Color { get; }

    public int Capacity => _samples.Capacity;

    public int Count => _samples.Count;

    public double Newest => Count == 0 ? 0 : _samples.Newest;

    public double this[int index] => _samples[index];

    public ChartSeries(string label, Color color, int capacity)
    {
        _samples = new SampleRing<double>(capacity);
        Label = label;
        Color = color;
    }

    public void Add(double value) => _samples.Add(value);

    public void Clear() => _samples.Clear();

    public double Peak()
    {
        double peak = 0;
        for (int i = 0; i < Count; i++)
        {
            if (this[i] > peak)
            {
                peak = this[i];
            }
        }
        return peak;
    }
}
