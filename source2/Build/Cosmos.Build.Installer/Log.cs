using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Installer {
  public class Log {
    public void WriteLine(string aText) {
      if (LogLine != null) {
        LogLine(aText);
      }
    }

    public void NewSection(string aText) {
      if (LogSection != null) {
        LogSection(aText);
      }
    }

    public delegate void LogLineHandler(string aLine);
    public event LogLineHandler LogLine;

    public delegate void LogSectionHandler(string aLine);
    public event LogSectionHandler LogSection;
  }
}
