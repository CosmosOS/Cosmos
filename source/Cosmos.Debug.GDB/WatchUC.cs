using System;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class WatchUC : UserControl {
        public event Action<string> Delete;
		public string mCountOfInt;
		public RegNames? mRegister;

		public WatchUC() {
            InitializeComponent();
			mCountOfInt = "1";
        }

        private void lablDelete_Click(object sender, EventArgs e) {
			if (MessageBox.Show("Delete watch " + lablAddress.Text + "?", "Delete Watch", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
					== DialogResult.Yes) {
				if (mRegister.HasValue)
					Delete(lablAddress.Text);
				else
					Delete(Global.FromHexWithLeadingZeroX(lablAddress.Text).ToString());
            }
         }

		public void Send() {
			Global.GDB.SendCmd(string.Format("x/{0}x {1}", mCountOfInt, GetAddress()));
		}

		private string GetAddress() {
			if (mRegister.HasValue)
				return "0x" + Windows.mRegistersForm.GetRegisterValue(mRegister.Value).ToString("X");
			else
				return lablAddress.Text;
		}

		private void lablName_TextChanged(object sender, EventArgs e)
		{
			int height = 20 +  (lablValue.Lines.Length - 1) * 13;
			if(lablValue.Height != height)
				lablValue.Height = height;
		}
	}
}