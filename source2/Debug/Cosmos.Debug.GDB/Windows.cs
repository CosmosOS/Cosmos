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
                aForm.BringToFront();
            }
        }

        static public void RestorePositions() {
            RestoreWindow(mCallStackForm);
        }

        static protected void RestoreWindow(Form aForm) {
            //http://social.msdn.microsoft.com/forums/en-US/winforms/thread/72b2edaf-0719-4d22-885e-48d643dc626b
            var x = Settings.DS.Window.FindByName(aForm.GetType().Name);
            if (x != null) {
                if (!x.IsLeftNull()) {
                    aForm.Left = x.Left;
                }
                if (!x.IsTopNull()) {
                    aForm.Top = x.Top;
                }
                if (!x.IsWidthNull()) {
                    aForm.Width = x.Width;
                }
                if (!x.IsHeightNull()) {
                    aForm.Height = x.Height;
                }
            }
        }

        static protected void SaveWindow(Form aForm) {
            var x = Settings.DS.Window;
            var xRow = x.NewWindowRow();
            xRow.Name = aForm.GetType().Name;
            xRow.Left = aForm.Left;
            xRow.Top = aForm.Top;
            xRow.Width = aForm.Width;
            xRow.Height = aForm.Height;
            x.AddWindowRow(xRow);
        }

        static public void SavePositions() {
            Settings.DS.Window.Clear();
            SaveWindow(mCallStackForm);
        }

        static public void UpdateAllWindows() {
            Windows.mMainForm.Disassemble("");
            Windows.mMainForm.GetRegisters();
            Windows.mCallStackForm.Update();
        }

    }
}
