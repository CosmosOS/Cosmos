namespace Cosmos.Build.Installer {
  static public class Log {
    static public void WriteLine(string aText) {
      LogLine?.Invoke(aText);
    }

    static public void NewSection(string aText) {
      LogSection?.Invoke(aText);
    }

    static public void SetError() {
      LogError?.Invoke();
    }

    public delegate void LogErrorHandler();
    static public event LogErrorHandler LogError;

    public delegate void LogLineHandler(string aLine);
    static public event LogLineHandler LogLine;

    public delegate void LogSectionHandler(string aLine);
    static public event LogSectionHandler LogSection;
  }
}
