using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cosmos.VS.Windows
{
    /// <summary>
    /// Interaction logic for ConsoleUC.xaml
    /// </summary>
    public partial class ConsoleUC
    {
        public ConsoleUC()
        {
            InitializeComponent();
        }

        //private StreamWriter mOut = StreamWriter.Null;
        private StreamWriter mOut = new StreamWriter(@"c:\data\output.txt", false)
                                    {
                                        AutoFlush = true
                                    };

        protected override void HandleChannelMessage(byte aChannel, byte aCommand, byte[] aData)
        {
            if (aChannel != ConsoleConsts.Channel)
            {
                return;
            }

            if (aCommand == ConsoleConsts.Command_WriteText)
            {
                mOut.Write(Encoding.ASCII.GetString(aData).Replace("\t", "    "));
                //textBox.Text += Encoding.ASCII.GetString(aData).Replace("\t", "    ");
            }
            else
            {
                mOut.WriteLine("Command '{0}' not recognized. Data = '{1}'", aCommand, Encoding.ASCII.GetString(aData).Replace("\t", "    "));
                //textBox.Text += ("Command '" + aCommand + "' not recognized!\r\n");
            }
            //textBox.ScrollToEnd();
        }
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
