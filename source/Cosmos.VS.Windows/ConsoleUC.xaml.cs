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
using Cosmos.Debug.Common;
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
        }

        protected override void HandleChannelMessage(byte aChannel, byte aCommand, byte[] aData)
        {
            if (aChannel != ConsoleConsts.Channel)
            {
                return;
            }

            //using (var xFS = new FileStream(@"e:\OpenSource\Edison\Serial\Console.in", FileMode.OpenOrCreate))
            //{
            //    xFS.Position = xFS.Length;
            //    xFS.WriteByte(aCommand);
            //    if (aData.Length > 0)
            //    {
            //        xFS.Write(aData, 0, aData.Length);
            //    }
            //}

            if (aCommand == ConsoleConsts.Command_WriteText)
            {
                textBox.AppendText(Encoding.ASCII.GetString(aData) + "\r\n");
            }
            else
            {
                textBox.AppendText("Command '" + aCommand + "' not recognized!\r\n");
            }
        }

        //public override void Update(string aTag, byte[] aData)
        //{
        //    base.Update(aTag, aData);

        //    if (aData.Length > 0)
        //    {
        //        using (var xFS = new FileStream(@"e:\OpenSource\Edison\Serial\Console.in", FileMode.OpenOrCreate))
        //        {
        //            xFS.Position = xFS.Length;
        //            xFS.Write(aData, 0, aData.Length);
        //        }
        //    }
        //    var xTxt = aData.Aggregate("0x", (s, b) => s + b.ToString("X2"));

        //    if (Dispatcher.CheckAccess())
        //    {
        //        textBox.AppendText(xTxt + "\r\n");
        //    }
        //    else
        //    {
        //        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => textBox.AppendText(xTxt + "\r\n")));
        //    }
        //}
    }

    [Guid("681a4da7-ba11-4c26-80a9-b39734a95b1c")]
    public class ConsoleTW : ToolWindowPaneChannel
    {
        public ConsoleTW()
        {
            Caption = "Cosmos Console";
            BitmapResourceID = 301;
            BitmapIndex = 1;

            var xUserControl = new ConsoleUC();
            Content = xUserControl;
            mUserControl = xUserControl;
        }
    }
}
