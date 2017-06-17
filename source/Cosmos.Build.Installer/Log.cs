namespace Cosmos.Build.Installer
{
  public class Log
  {
    public void WriteLine(string aText)
    {
      LogLine?.Invoke(aText);
    }

    public void NewSection(string aText)
    {
      LogSection?.Invoke(aText);
    }

    public void SetError()
    {
      LogError?.Invoke();
    }

    public delegate void LogErrorHandler();
    public event LogErrorHandler LogError;

    public delegate void LogLineHandler(string aLine);
    public event LogLineHandler LogLine;

    public delegate void LogSectionHandler(string aLine);
    public event LogSectionHandler LogSection;
  }
}
