using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Cosmos.Kernel.LogTail.Handlers {
	public partial class HeapLogHandler: LogHandler {
		public override string Title {
			get {
				return "Heap Usage";
			}
		}

		public HeapLogHandler() {
			InitializeComponent();
		}

		public override void Clear() {
			if (InvokeRequired) {
				this.BeginInvoke(new Action(Clear));
				return;
			}
		}

		private uint mHeapUsage = 0;
		public uint HeapUsage {
			get {
				return mHeapUsage;
			}
			set {
				if (value != mHeapUsage) {
					mHeapUsage = value;
					lblValue.Text = GetValueString(value);
				}
			}
		}

		private static string GetValueString(uint aValue) {
			string[] xTest = new string[] { "B", "KB", "MB", "GB" };
			int xIteration = 0;
			Single xValue = aValue;
			while (xValue >= 1024) {
				xValue /= 1024f;
				xIteration++;
			}
			return xValue.ToString("#0.000") + " " + xTest[xIteration];
		}

		private delegate void Handler(LogMessage message);

		public override void HandleMessage(LogMessage message) {
			if (InvokeRequired) {
				this.BeginInvoke(new Handler(HandleMessage), message);
				return;
			}
			if (message.Name == "MM_Alloc") {
				HeapUsage += UInt32.Parse(message["Length"].Substring(2), NumberStyles.HexNumber);
				return;
			}
			if (message.Name == "MM_Init") {
				lblAvailValue.Text = GetValueString(UInt32.Parse(message["Length"].Substring(2), NumberStyles.HexNumber));
				return;
			}
		}
	}
}
