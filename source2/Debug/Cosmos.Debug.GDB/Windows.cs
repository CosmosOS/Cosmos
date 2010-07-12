using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public class Windows {
        static public FormMain mMainForm;
        static public FormCallStack mCallStackForm;
        static public FormWatches mWatchesForm;
        static public FormLog mLogForm;

        static public void CreateForms() {
            mCallStackForm = new FormCallStack();
            mCallStackForm.Show();

            mWatchesForm = new FormWatches();
            mWatchesForm.Show();

            mLogForm = new FormLog();
            mLogForm.Show();
        }

        static public void Show(Form aForm) {
            if (aForm != null) {
                aForm.Show();
            }
        }

        static public void UpdateAllWindows() {
            Windows.mMainForm.Disassemble("");
            Windows.mMainForm.GetRegisters();
            Windows.mCallStackForm.Update();
        }

    }
}
