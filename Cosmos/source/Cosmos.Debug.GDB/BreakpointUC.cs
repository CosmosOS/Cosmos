using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public partial class BreakpointUC : UserControl {
        public event Action<int> Delete;

        public BreakpointUC() {
            InitializeComponent();
        }

        private void lablDelete_Click(object sender, EventArgs e) {
            if (MessageBox.Show("Delete breakpoint " + lablName.Text + "?", "Delete Breakpoint", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                Delete(int.Parse(lablNum.Text));
            }
         }
    }
}
