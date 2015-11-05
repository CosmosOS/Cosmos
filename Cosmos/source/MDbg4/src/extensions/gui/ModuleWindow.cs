//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#endregion

using System.Globalization;
using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Tools.Mdbg.Extension;

using Microsoft.Samples.Debugging.CorDebug;
using System.Diagnostics;

namespace gui
{
    partial class ModuleWindow : DebuggerToolWindow
    {
        public ModuleWindow(MainForm mainForm)
            : base(mainForm)
        {
            InitializeComponent();
        }

        // Populate the module window with the current list.
        // Called on UI thread.
        protected override void RefreshToolWindowInternal()
        {
            ListView.ListViewItemCollection items = this.listView1.Items;
            items.Clear();

            ListViewItem[] temp = null;

            // Go to worker thread to collect information
            
            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
            {
                Debug.Assert(proc != null);
                Debug.Assert(!proc.IsRunning);
                

                temp = new ListViewItem[proc.Modules.Count];
                int idx = 0;

                foreach (MDbgModule m in proc.Modules)
                {
                    StringBuilder sbFlags = new StringBuilder();

                    if (m.SymReader == null)
                    {
                        sbFlags.Append("[No symbols]");
                    }
                    else
                    {
                        sbFlags.Append("[Symbols]");
                    }

                    string fullname = m.CorModule.Name;
                    string directory = System.IO.Path.GetDirectoryName(fullname);
                    string name = System.IO.Path.GetFileName(fullname);

                    bool fIsDynamic = m.CorModule.IsDynamic;
                    if (fIsDynamic)
                    {
                        sbFlags.Append("[Dynamic] ");
                    }

                    CorDebugJITCompilerFlags flags = m.CorModule.JITCompilerFlags;

                    bool fNotOptimized = (flags & CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION) == CorDebugJITCompilerFlags.CORDEBUG_JIT_DISABLE_OPTIMIZATION;
                    if (fNotOptimized)
                    {
                        sbFlags.Append("[Not-optimized] ");
                    }
                    else
                    {
                        sbFlags.Append("[Optimized] ");
                    }

                    // Columns: Id, Name, Path, Flags
                    temp[idx++] = new ListViewItem(
                       new string[] { m.Number.ToString(), name, directory, sbFlags.ToString() }
                       );


                }
            }); // end worker
            

            if (temp != null)
            {
                foreach (ListViewItem x in temp)
                {
                    items.Add(x);
                }
            }
        }
    }
}