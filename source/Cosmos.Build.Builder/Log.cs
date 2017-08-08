namespace Cosmos.Build.Builder
{
  public static class Log
  {
    public static void WriteLine(string aText)
    {
      LogLine?.Invoke(aText);
    }

    public static void NewSection(string aText)
    {
      LogSection?.Invoke(aText);
    }

    public static void SetError()
    {
      LogError?.Invoke();
    }

    public delegate void LogErrorHandler();

    public static event LogErrorHandler LogError;

    public delegate void LogLineHandler(string aLine);

    public static event LogLineHandler LogLine;

    public delegate void LogSectionHandler(string aLine);

    public static event LogSectionHandler LogSection;
  }
}