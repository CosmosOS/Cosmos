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
using System.Diagnostics;

namespace gui
{
    // Tool window to evaluate simple expressions.
    partial class QuickViewWindow : DebuggerToolWindow
    {
        public QuickViewWindow(MainForm mainForm)
            : base(mainForm)
        {
            InitializeComponent();

            // Hook handler for lazily expanding
            treeView1.BeforeExpand += new TreeViewCancelEventHandler(treeView1_BeforeExpand);
        }

        protected override void RefreshToolWindowInternal()
        {
            // Nothing to do in refresh since we only respond to 
            // the user entering a value.
        }

        // Trap "Enter" key on dialog input.
        // called on UI thread.
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
                string arg = this.textBox1.Text;
                MDbgValue val = Resolve(arg);
                Util.Print(MainForm, val, this.treeView1);                
            }
        }

        // Resolve the expression to a value.
        // Returns "Null" if we can't resolve the arg.
        // called on UI thread.
        MDbgValue Resolve(string arg)
        {
            MDbgValue var = null;
            MainForm.ExecuteOnWorkerThreadIfStoppedAndBlock(delegate(MDbgProcess proc)
            {
                MDbgFrame frame = GetCurrentFrame(proc);
                if (frame == null)
                {
                    return;
                }

                var = proc.ResolveVariable(arg, frame);
            });
            return var;
        }

        // When tree mode, add stuff to it.
        void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            Util.TryExpandNode(MainForm, e.Node);
        }        

    }      // end QuickViewWindow
}