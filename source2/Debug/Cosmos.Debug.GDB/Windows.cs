using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cosmos.Debug.GDB {
    public class Windows {
        static public FormMain mMainForm;
        //
        static public FormCallStack mCallStackForm;
        static public FormWatches mWatchesForm;
        static public FormLog mLogForm;
        static public FormBreakpoints mBreakpointsForm;
        static public FormRegisters mRegistersForm;
        static public FormDisassembly mDisassemblyForm;

        static public List<Form> mForms = new List<Form>();

        static public void CreateForms() {
            mForms.Add(mCallStackForm = new FormCallStack());
            mForms.Add(mWatchesForm = new FormWatches());
            mForms.Add(mLogForm = new FormLog());
            mForms.Add(mBreakpointsForm = new FormBreakpoints());
            mForms.Add(mRegistersForm = new FormRegisters());
            mForms.Add(mDisassemblyForm = new FormDisassembly());
        }

        static public void Show(Form aForm) {
            if (aForm != null) {
                aForm.Show();
                aForm.BringToFront();
            }
        }

        static public void RestorePositions() {
            foreach (var x in mForms) {
                RestoreWindow(x);
            }
        }

        static protected void RestoreWindow(Form aForm) {
            bool xShowForm = true;
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
                // On load the often end up behind other apps, so we do this to force them up on first show
                if (!x.IsVisibleNull()) {
                    // Technically we should do this if any size exists, generally
                    // they should be all present or all missing. so we just use
                    // this one attribute for now.
                    aForm.StartPosition = FormStartPosition.Manual;
                    xShowForm = x.Visible;
                }
                var xBreakpoints = aForm as FormBreakpoints;
                if (xBreakpoints != null)
                {
                    xBreakpoints.LoadSession();
                }
            }
            if (xShowForm) {
                Show(aForm);
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
            xRow.Visible = aForm.Visible;
            x.AddWindowRow(xRow);
        }

        static public void SavePositions() {
            Settings.DS.Window.Clear();
            foreach (var x in mForms) {
                SaveWindow(x);
            }
        }

        static public void UpdateAllWindows() {
            Windows.mDisassemblyForm.Disassemble("");
            Windows.mRegistersForm.Redo();
            Windows.mCallStackForm.Redo();
        }

        static protected List<Form> mVisibleWindows = new List<Form>();

        static protected void SaveWindowState(Form aForm) {
            if (aForm.Visible) {
                mVisibleWindows.Add(aForm);
            }
        }

        static public void Hide() {
            foreach (var x in mForms) {
                if (x.Visible) {
                    mVisibleWindows.Add(x);
                    x.Hide();
                }
            }
        }

        static public void Reshow() {
            foreach (var x in mVisibleWindows) {
                x.Show();
            }
            mVisibleWindows.Clear();
        }
    }
}
