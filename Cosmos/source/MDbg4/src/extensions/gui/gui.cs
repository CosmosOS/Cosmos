//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using Microsoft.Samples.Tools.Mdbg;
using Microsoft.Samples.Debugging.MdbgEngine;
using System.Threading;
using System.Windows.Forms;
using System.Security.Permissions;
using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

[assembly:CLSCompliant(true)]
[assembly:System.Runtime.InteropServices.ComVisible(false)]

namespace Microsoft.Samples.Tools.Mdbg.Extension
{
    public abstract class GuiExtension : CommandBase
    {
        public static void LoadExtension()
        {
            try 
            {
                MDbgAttributeDefinedCommand.AddCommandsFromType(Shell.Commands,typeof(GuiExtension));
            } 
            catch
            {
                // we'll ignore errors about multiple defined gui command in case gui is loaded
                // multiple times.
            }

            // and also load gui directly.
            Gui("");
        }

        #region Extra IL stepping commands        
        [
         CommandDescription(
                            CommandName = "il_next",
                            ShortHelp = "Step over the next IL instruction",
                            MinimumAbbrev = 4,
                            LongHelp = "Usage: il_next"
                            )
         ]
        public static void ILNextCmd(string args)
        {
            ILStepWorker(false, true, args);
        }
        
        [
         CommandDescription(
                            CommandName = "il_step",
                            ShortHelp = "Step into the next IL instruction",
                            MinimumAbbrev = 4,
                            LongHelp = "Usage: il_step"
                            )
        ]
        public static void ILStepCmd(string args)
        {
            ILStepWorker(true, true, args);
        }

        // Does the real work for an IL step.
        // fStepIn - true if step in; else false for step-over.
        // fVerbose - true if we should print out verbose diagnostic information.
        public static void ILStepWorker(bool stepIn, bool isVerbose, string args)
        {
            if ((args != null) && (args.Trim().Length > 0))
            {
                throw new MDbgShellException("no arguments expected.");
            }

            // Do an IL-level step by single-stepping native code until our IL IP changes.
            MDbgProcess proc = GuiExtension.Shell.Debugger.Processes.Active;
            CorFrame curFrame = proc.Threads.Active.CurrentFrame.CorFrame;

            uint ipNative;
            uint offsetStart;
            uint offsetNew;
            
            ulong oldStackStart;
            ulong oldStackEnd;
            ulong newStackStart;
            ulong newStackEnd;

            CorDebugMappingResult result;
            curFrame.GetIP(out offsetStart, out result);
            curFrame.GetStackRange(out oldStackStart, out oldStackEnd);
            curFrame.GetNativeIP(out ipNative);

            if (isVerbose)
            {
                WriteOutput(String.Format("  IL step starting at offset IL_{0:x}, N=0x{1:x}, frame=({2:x},{3:x})",
                    offsetStart, ipNative, oldStackStart, oldStackEnd));
            }

            // We'll just do native single-steps until our IL ip changes.
            while(true)
            {
                // Single step native code.
                if (stepIn)
                {
                    proc.StepInto(true).WaitOne();
                }else
                {
                    proc.StepOver(true).WaitOne();
                }

                // Since we continued the process, our old frame becomes invalid and we need to get a new one.
                curFrame = proc.Threads.Active.CurrentFrame.CorFrame;
                curFrame.GetStackRange(out newStackStart, out newStackEnd);
                if ((newStackStart != oldStackStart) || (newStackEnd != oldStackEnd))
                {
                    // We're in a new frame. Maybe from step-in or step-out.
                    if (isVerbose)
                    {
                        WriteOutput(String.Format("  IL step stopping at new frame =({0:x},{1:x})", newStackStart, newStackEnd));
                    }
                    break;
                }
                curFrame.GetIP(out offsetNew, out result);
                curFrame.GetNativeIP(out ipNative);
                if ((offsetNew == offsetStart) && (proc.StopReason.GetType() == typeof(StepCompleteStopReason)))
                {
                    if (isVerbose)
                    {
                        WriteOutput(String.Format("  IL step continuing. N=0x{0:x}", ipNative));
                    }
                    continue;
                }

                if (isVerbose)
                {
                    WriteOutput(String.Format("  IL step complete at IL_{0:x}", offsetNew));
                }
                break;
            }
            

        }        
        #endregion

        #region .Menu command

        // Return a stripped version of the menu item that we can compare against.
        // Removes any &, 
        static string StripMenuCommand(string name)
        {
            name = name.Trim().Replace("&", "").Replace(" ", "").Replace(".", "");
            return name;
        }

        // Command to invoke a GUI menu element from the command line.
        [
            CommandDescription(
            CommandName = ".menu",
            MinimumAbbrev = 2,
            ShortHelp = "Execute gui menu command",
            LongHelp = @"Executes a gui menu command. Command is specified via menu names, and uses '|' to 
specifiy sub menues. Eg, '.menu Tools|Callstack'"
            )
        ]
        public static void ExecuteGuiCommand(string args)
        {
            if (m_mainForm == null || m_mainForm.IsDisposed)
            {
                throw new MDbgShellException("Gui is closed");                
            }
            if (args == null || args.Length == 0)
            {
                throw new MDbgShellException("Illegal usage. Expecting args");                
            }
            string[] parts = args.Split('|');
            if (parts.Length <= 0)
            {
                throw new MDbgShellException("Illegal usage. Expecting args");                
            }

            MenuItem m = null;
            Menu.MenuItemCollection c = m_mainForm.Menu.MenuItems;

            string lastPart = "<top>";
            foreach(String rawPart in parts)
            {
                string part = StripMenuCommand(rawPart);
                // 'MenuItemCollection.Find' doesn't search on Text property. So we search manually.
                m = null;
                foreach (MenuItem m2 in c)
                {
                    // Do case-insensitive string compare.
                    string menuText = StripMenuCommand(m2.Text);
                    if (String.Compare(menuText, part, true) == 0)
                    {
                        m = m2;
                        break;
                    }
                }
                if (m == null)
                {
                    // No match was found. To be helpful, print out possible matches.
                    if (c.Count == 0)
                    {
                        WriteOutput(MDbgOutputConstants.Ignore, "Menu '" + lastPart + "' has no sub menus.");
                    }
                    else {
                        WriteOutput(MDbgOutputConstants.Ignore, "Menu '" + lastPart +"' only has the following sub menus:");
                        foreach (MenuItem m2 in c)
                        {
                            string name  =  StripMenuCommand(m2.Text);
                            if (name != "-") // skip separators.
                            {
                                WriteOutput(MDbgOutputConstants.Ignore, "   " + name);
                            }
                        }
                    }

                    throw new MDbgShellException("Can't find item '" + part + "' in '" + args + "'");                    
                }

                lastPart = part;
                c = m.MenuItems;
            }
            System.Diagnostics.Debug.Assert(m != null);

            string prefix = "Invoking menu command:";
            WriteOutput(MDbgOutputConstants.StdOutput, prefix + args, prefix.Length, args.Length);
            InvokeMenuItemHelper(m, args);
        }
        
        // Called on worker thread to invoke a menu command.
        static void InvokeMenuItemHelper(MenuItem m, string args)
        {
            // Need to make cross-thread call to invoke. Don't block since the GUI thread won't pump messages.            
            m_mainForm.BeginInvoke(new MethodInvoker(delegate()
            {
                
                {
                    m.PerformClick();
                    WriteOutput("Done invoking menu command:" + args);
                }
                /*  */
            }));            
        }
        #endregion .Menu command

        [
         CommandDescription(
           CommandName = "gui",
           ShortHelp = "gui [close] - starts/closes a gui interface",
           LongHelp =
             "Usage: gui [close]"
         )
        ]
        public static void Gui(string args)
        {
            // just do the check that gui is not already loaded,
            // strange things are happening else:
            ArgParser ap = new ArgParser(args);
            if( ap.Exists(0) )
            {
                if( ap.AsString(0) == "close" )
                {
                    if( m_mainForm!=null )
                    {
                        m_mainForm.CloseGui();
                        Application.Exit(); // this line will cause the message pump on other thread to quit.
                        return;
                    }
                    else
                        throw new MDbgShellException("GUI not started.");
                }
                else
                    throw new MDbgShellException("invalid argument");
            }
            
            if(Shell.IO == m_mainForm)
            {
                WriteOutput("GUI already started. Cannot start second instance.");
                return;
            }

            WriteOutput("starting gui");

            m_mainForm = new MainForm(Shell);

            Thread t = new Thread(new ThreadStart(RunMessageLoop));

            // Only MTA Threads can access CorDebug interfaces because mscordbi doesn't provide marshalling to make them accessable from STA Threads
            // However, UI thread must be STA. So we'll make cross-thread calls to a worker thread for all ICorDebug access.
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;

            t.Start();
            m_mainForm.InitComplete.WaitOne(); // wait till form is fully displayed.
            
            WriteOutput(MainForm.MetaInfoOutputConstant, "GUI: Simple Extension for Managed debugger (MDbg) started");
            WriteOutput(MainForm.MetaInfoOutputConstant, "for information on how to use the extension select in menu bar help|about");
            WriteOutput(MainForm.MetaInfoOutputConstant, "!!This is just a sample. It is not intended for Production purposes!!\n");
        }

        public static void RunMessageLoop()
        {
            // if we have following code here and close the dialog the we cannot break into debugger.
            //m_mainForm.Show();
            //Application.Run(m_mainForm);//new ApplicationContext(m_mainForm));
            m_mainForm.ShowDialog();
        }
        private static MainForm m_mainForm;
    }
}

        
