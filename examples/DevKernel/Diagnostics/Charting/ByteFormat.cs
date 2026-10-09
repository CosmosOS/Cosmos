namespace DevKernel.Diagnostics.Charting;

// Byte counts rendered in the largest unit that keeps them under four digits, so
// columns stay narrow and comparable on screen.
public static class ByteFormat
{
    private const long Kilo = 1024;
    private const long Mega = Kilo * 1024;
    private const long Giga = Mega * 1024;

    public static string Short(long bytes)
    {
        if (bytes < 0)
        {
            return "-" + Short(-bytes);
        }

        if (bytes < Kilo)
        {
            return bytes + " B";
        }

        if (bytes < Mega)
        {
            return WithTenth(bytes, Kilo) + " KB";
        }

        if (bytes < Giga)
        {
            return WithTenth(bytes, Mega) + " MB";
        }

        return WithTenth(bytes, Giga) + " GB";
    }

    public static string Short(double bytes) => Short((long)bytes);

    private static string WithTenth(long bytes, long unit)
    {
        long tenths = bytes * 10 / unit;
        return tenths / 10 + "." + tenths % 10;
    }
}
