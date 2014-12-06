using Cosmos.Debug.Common;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Cosmos.VS.Windows
{
    [Guid("39A8D0A0-26A3-4234-A660-0C4C8BF40FF3")]
    public class InternalTW : ToolWindowPane2
    {
        public InternalTW()
        {
            Caption = "Cosmos Internal";

            BitmapResourceID = 301;
            BitmapIndex = 1;

            mUserControl = new InternalUC();
            Content = mUserControl;
        }
    }

    public partial class InternalUC : DebuggerUC
    {
        public InternalUC()
        {
            InitializeComponent();

            butnPingVSIP.Click += new RoutedEventHandler(butnPingVSIP_Click);
            butnPingDS.Click += new RoutedEventHandler(butnPingDS_Click);
            butnClearLog.Click += new RoutedEventHandler(butnClearLog_Click);
        }

        private void Log(string aMsg)
        {
            Log(aMsg, false);
        }

        private void Log(string aMsg, bool aLogTime)
        {
            if (aLogTime)
            {
                aMsg = DateTime.Now.ToString("HH:mm:ss") + ": " + aMsg;
            }
            lboxLog.SelectedItem = lboxLog.Items.Add(aMsg);
        }

        private void butnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lboxLog.Items.Clear();
        }

        private void butnPingDS_Click(object sender, RoutedEventArgs e)
        {
            Global.PipeUp.SendCommand(Windows2Debugger.PingDebugStub);
        }

        private void butnPingVSIP_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This only works if there is an active debug session, see comment in code for this event for more details.");
            // Note: This will only work if the debugger is active,
            // ie Cosmos is booted. This is because the receiving
            // pipe is currently part of AD7Process which has a lifespan
            // tied to an active debug session.
            Global.PipeUp.SendCommand(Windows2Debugger.PingVSIP);
        }

        protected override void DoUpdate(string aTag)
        {
            string xText = Encoding.UTF8.GetString(mData);
            Log(xText, true);
        }
    }
}