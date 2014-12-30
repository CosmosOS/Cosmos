using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell;

namespace Cosmos.VS.Windows
{
  /// <summary>
  /// Interaction logic for ConsoleUC.xaml
  /// </summary>
  public partial class ConsoleUC
  {
    private DispatcherTimer mTimer;

    public ConsoleUC()
    {
      InitializeComponent();

      mTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Normal, OnTimer, Dispatcher);
    }

    private void OnTimer(object sender, EventArgs e)
    {
      mTimer.Stop();
      try
      {
        var xBuff = new byte[16384];
        if (!Global.ConsoleTextChannel.IsConnected)
        {
          return;
        }
        var xReadBytes = Global.ConsoleTextChannel.Read(xBuff, 0, xBuff.Length);
        if (xReadBytes > 0)
        {
          using (var xFS = new FileStream(@"e:\OpenSource\Edison\Serial\Console.in", FileMode.OpenOrCreate))
          {
            xFS.Position = xFS.Length;
            xFS.Write(xBuff, 0, xReadBytes);
          }
          textBox.AppendText(Encoding.UTF8.GetString(xBuff, 0, xReadBytes));
        }
      }
      finally
      {
        mTimer.Start();
      }
    }
  }

  [Guid("681a4da7-ba11-4c26-80a9-b39734a95b1c")]
  public class ConsoleTW : ToolWindowPane2
  {
    public ConsoleTW()
    {
      Caption = "Cosmos Console";
      BitmapResourceID = 301;
      BitmapIndex = 1;

      var xUserControl = new ConsoleUC();
      Content = xUserControl;
      this.OnClose();
    }
  }
}
