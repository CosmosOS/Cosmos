//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.MdbgEngine;
using Microsoft.Samples.Debugging.CorDebug.NativeApi;

namespace Microsoft.Samples.Tools.Mdbg
{
    /// <summary>
    /// Base class for implementing stop option policies, used to customize and determine what the
    /// debugger should do when it receives callbacks.
    /// </summary>
    public abstract class MDbgStopOptionPolicy
    {
        /// <summary>
        /// Sets the debugger behavior for this stop option policy.
        /// </summary>
        /// <param name="behavior">Debugger behavior - stop, log, or ignore.</param>
        /// <param name="arguments">Other arguments.</param>
        public abstract void SetBehavior(DebuggerBehavior behavior, string arguments);

        /// <summary>
        /// Acts on debugger callback.
        /// </summary>
        /// <param name="currentProcess">The current MDbgProcess. </param>
        /// <param name="args">The callback arguments.</param>
        public abstract void ActOnCallback(MDbgProcess currentProcess, CustomPostCallbackEventArgs args);

        /// <summary>
        /// Prints the stop option policy.
        /// </summary>
        public abstract void Print();

        /// <summary>
        /// The acronym associated with the stop option policy. 
        /// </summary>
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
            }
        }

        /// <summary>
        /// Possible behaviors of the debugger when it receives a callback.
        /// </summary>
        public enum DebuggerBehavior
        {
            /// <summary>
            /// The debugger should ignore this callback.
            /// </summary>
            Ignore,
            /// <summary>
            /// The debugger should log this callback.
            /// </summary>
            Log,
            /// <summary>
            /// The debugger should notify this callback.
            /// </summary>
            Notify,
            /// <summary>
            /// The debugger should stop on this callback.
            /// </summary>
            Stop
        };

        private string m_acronym;
    }

    /// <summary>
    /// A simple stop option policy that allows a user to specify a single behavior for a callback type.
    /// </summary>
    public class SimpleStopOptionPolicy : MDbgStopOptionPolicy
    {
        /// <summary>
        /// Default constructor. Sets the default behavior to DebuggerBehaviors.ignore.
        /// </summary>
        /// <param name="acronym">The acronym associated with this stop option policy.</param>
        /// <param name="fullName">The full name of the callback event, for printing.</param>
        public SimpleStopOptionPolicy(string acronym, string fullName)
        {
            Acronym = acronym;
            m_behavior = DebuggerBehavior.Ignore;
            m_fullName = fullName;
        }

        /// <summary>
        /// Acts on the current callback, based on the current debugger behavior for this stop
        /// option policy.
        /// </summary>
        /// <param name="currentProcess">Current MDbgProcess.</param>
        /// <param name="args">Callback arguments.</param>
        public override void ActOnCallback(MDbgProcess currentProcess, CustomPostCallbackEventArgs args)
        {
            CorEventArgs eventArgs = args.CallbackArgs as CorEventArgs;
            switch (m_behavior)
            {
                case DebuggerBehavior.Stop:
                    args.Controller.Stop(eventArgs.Thread, MDbgUtil.CreateStopReasonFromEventArgs(eventArgs, currentProcess));
                    break;
                case DebuggerBehavior.Log:
                    CommandBase.WriteOutput(eventArgs.ToString() + "\n");
                    break;
                case DebuggerBehavior.Notify:
                    CommandBase.WriteOutput(eventArgs.ToString() + "\n");
                    MDbgThread currentThread = currentProcess.Threads.GetThreadFromThreadId((args.CallbackArgs as CorThreadEventArgs).Thread.Id);

                    try
                    {
                        // Getting the current notification may not be implemented.
                        MDbgValue notification = currentThread.CurrentNotification;
                        if (notification != null)
                        {
                            CommandBase.WriteOutput(notification.GetStringValue(true));
                        }
                        else
                        {
                            CommandBase.WriteOutput("custom notification is null\n");
                        }
                    }
                    catch (NotImplementedException)
                    {
                        Trace.WriteLine("Custom Notifications Not Implemented");
                    }
                    break;

            }
        }

        /// <summary>
        /// Sets the debugger behavior for this stop option policy.
        /// </summary>
        /// <param name="behavior">stop, log, notify, or ignore</param>
        /// <param name="arguments">Must be null.</param>
        public override void SetBehavior(DebuggerBehavior behavior, string arguments)
        {
            if (arguments != null)
            {
                throw new MDbgShellException("This event type does not accept arguments.");
            }
            m_behavior = behavior;
        }

        /// <summary>
        /// Prints the stop option policy.
        /// </summary>
        public override void Print()
        {
            string output = "(" + Acronym + ") " + m_fullName + ": ";
            int length = output.Length;
            StringBuilder sb = new StringBuilder(output);
            for (int i = 0; i < 25 - length; i++)
            {
                //to ensure proper spacing
                sb.Append(" ");
            }
            CommandBase.WriteOutput(sb + Enum.GetName(typeof(DebuggerBehavior), m_behavior));
        }

        /// <summary>
        /// The full name of this stop option policy.
        /// </summary>
        protected string FullName
        {
            get
            {
                return m_fullName;
            }
            set
            {
                m_fullName = value;
            }
        }

        /// <summary>
        /// The current debugger behavior for this stop option policy.
        /// </summary>
        protected DebuggerBehavior Behavior
        {
            get
            {
                return m_behavior;
            }
            set
            {
                m_behavior = value;
            }
        }

        private string m_fullName;
        private DebuggerBehavior m_behavior;
    }

    /// <summary>
    /// Details debugger's behavior when the process it is attached to throws an exception. The user
    /// can specify different behaviors for different types of exceptions.
    /// </summary>
    public class ExceptionStopOptionPolicy : MDbgStopOptionPolicy
    {
        /// <summary>
        /// Default constructor. Sets the default behavior to ignore all exceptions, and sets
        /// the ExceptionEnhanced switch to false.
        /// </summary>
        public ExceptionStopOptionPolicy()
        {
            m_items = new List<ExceptionStopOptionPolicyItem>();
            m_default = DebuggerBehavior.Ignore;
            Acronym = "ex";
        }

        /// <summary>
        /// Acts on the debugger callback, based on the stop option policy settings and the 
        /// type of exception thrown.
        /// </summary>
        /// <param name="currentProcess">Current MDbgProcess.</param>
        /// <param name="args">Callback arguments.</param>
        public override void ActOnCallback(MDbgProcess currentProcess, CustomPostCallbackEventArgs args)
        {
            CorException2EventArgs ea = args.CallbackArgs as CorException2EventArgs;
            CorExceptionUnwind2EventArgs ua = args.CallbackArgs as CorExceptionUnwind2EventArgs;
            bool bException2 = (ea != null);

            if (m_exceptionEnhancedOn ||
               (bException2 && (ea.EventType == CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE)))
            {
                MDbgThread currentThread = null;
                currentThread = currentProcess.Threads.GetThreadFromThreadId((args.CallbackArgs as CorThreadEventArgs).Thread.Id);

                string exceptionType = null;
                DebuggerBehavior behavior;
                try
                {
                    // Getting the current exception may not be implemented.
                    exceptionType = currentThread.CurrentException.TypeName;
                    behavior = DetermineBehavior(exceptionType);
                }
                catch (NotImplementedException)
                {
                    behavior = this.m_default;
                }

                switch (behavior)
                {
                    case DebuggerBehavior.Stop:
                        if (bException2)
                        {
                            args.Controller.Stop(ea.Thread, new ExceptionThrownStopReason(ea.AppDomain,
                                ea.Thread, ea.Frame, ea.Offset, ea.EventType, ea.Flags, m_exceptionEnhancedOn));
                        }
                        else
                        {
                            args.Controller.Stop(ua.Thread, new ExceptionUnwindStopReason(ua.AppDomain,
                                ua.Thread, ua.EventType, ua.Flags));
                        }
                        break;
                    case DebuggerBehavior.Log:
                        string output = "Exception thrown: " + currentThread.CurrentException.TypeName +
                                        " at function " + currentThread.CurrentFrame.Function.FullName;
                        if (currentThread.CurrentSourcePosition != null)
                        {
                            output += " in source file " + currentThread.CurrentSourcePosition.Path +
                                      ":" + currentThread.CurrentSourcePosition.Line;
                        }
                        CommandBase.WriteOutput(output);
                        if (m_exceptionEnhancedOn)
                        {
                            if (bException2)
                            {
                                CommandBase.WriteOutput("Event type: " + ea.EventType);
                            }
                            else
                            {
                                CommandBase.WriteOutput("Event type: " + ua.EventType);
                            }
                        }
                        CommandBase.WriteOutput("");
                        break;
                }
            }
        }

        /// <summary>
        /// Modifies the exception settings, given an exception type and corresponding debugger behavior.
        /// </summary>
        /// <param name="behavior">stop, log, or ignore</param>
        /// <param name="arguments">A type of exception, or a regular expression.</param>
        public override void SetBehavior(DebuggerBehavior behavior, string arguments)
        {
            ExceptionStopOptionPolicyItem exceptionItem;
            string regex;
            int existingItemIndex = -1;
            bool redundantEntry = false;
            bool regexSubset = false;

            if (arguments == null)
            {
                // If no exception types are specified, then all existing settings are cleared and the
                // behavior becomes the default behavior for all exceptions.
                m_items.Clear();
                m_default = behavior;
                if (behavior == DebuggerBehavior.Ignore)
                {
                    // To preserve legacy behavior, ignoring all exceptions also turns the exception
                    // enhanced switch off.
                    m_exceptionEnhancedOn = false;
                }
            }
            else
            {
                // The arguments string can contain multiple exception types and regular expressions,
                // so the arguments are split into a string array. For example, if the arguments string
                // is "System.Exception System.A*", it is split into {"System.Exception", "System.A*"}.
                // The behavior for all of these arguments is then set to the given behavior. 
                string[] exceptionTypes = arguments.Split();
                string exceptionType;
                foreach (string type in exceptionTypes)
                {
                    exceptionType = type.Trim();
                    if (MDbgUtil.IsRegex(exceptionType))
                    {
                        // Input is a regular expression. Go through the existing exception items
                        // and check if any of them match the input regular expression. If they do,
                        // remove them, because the input regular expression is added after them
                        // and has precedence. For example, if input is "System.*" and an exception
                        // item already exists for "System.Exception", we remove the System.Exception
                        // item.
                        for (int i = 0; i < m_items.Count; i++)
                        {
                            exceptionItem = m_items[i];
                            regex = MDbgUtil.ConvertSimpleExpToRegExp(exceptionType);
                            if (Regex.IsMatch(exceptionItem.ExceptionType, regex))
                            {
                                m_items.Remove(exceptionItem);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        // Input is not a regular expression. Check if m_items already contains
                        // an entry for this exception type.
                        foreach (ExceptionStopOptionPolicyItem item in m_items)
                        {
                            if (item.ExceptionType == type)
                            {
                                existingItemIndex = m_items.IndexOf(item);
                                break;
                            }
                        }
                    }

                    // Check if input is redundant. An input is redundant if it does not change the existing
                    // behavior. There are two cases in which an input is redundant:
                    //
                    // - An exception item already exists that is a regular expression accepting the
                    // input exception type, and specifying the same debugger behavior. For example,
                    // if a "log System.*" exception item already exists, an input of "log System.Exception" 
                    // if redundant. However, an input of "ignore System.Exception" is NOT redundant,
                    // because it is added after the "log System.*" item and has precedence over it.
                    // 
                    // - The input behavior is the same as the default behavior. For example, if the 
                    // default behavior is ignore, an input of "ignore System.Exception" is redundant,
                    // UNLESS the input matches some existing exception item regular expression (as in
                    // the example above).

                    for (int i = m_items.Count - 1; i >= 0; i--)
                    {
                        exceptionItem = m_items[i];
                        if (m_items.IndexOf(exceptionItem) == existingItemIndex)
                        {
                            break;
                        }
                        if (MDbgUtil.IsRegex(exceptionItem.ExceptionType))
                        {
                            regex = MDbgUtil.ConvertSimpleExpToRegExp(exceptionItem.ExceptionType);
                            if (Regex.IsMatch(type, regex))
                            {
                                regexSubset = true;
                                if (behavior == exceptionItem.Behavior)
                                {
                                    redundantEntry = true;
                                }
                                break;
                            }
                        }
                    }
                    if (!regexSubset)
                    {
                        if (behavior == m_default)
                        {
                            redundantEntry = true;
                        }
                    }

                    // If the input modifies some existing exception item existing entry and the behavior 
                    // is redundant, remove the existing item. If no matching item exists and the input
                    // is redundant, do nothing.
                    if (existingItemIndex != -1)
                    {
                        if (redundantEntry)
                        {
                            m_items.RemoveAt(existingItemIndex);
                        }
                        else
                        {
                            m_items[existingItemIndex].Behavior = behavior;
                        }
                    }
                    else
                    {
                        if (!redundantEntry)
                        {
                            m_items.Add(new ExceptionStopOptionPolicyItem(type, behavior));
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Prints the current exception settings.
        /// </summary>
        public override void Print()
        {
            CommandBase.WriteOutput("(ex) <Exception Type> Exception:");
            if (m_items.Count == 0)
            {
                // behavior for specific exception types has not been specified - just print default behavior
                switch (m_default)
                {
                    case DebuggerBehavior.Stop: CommandBase.WriteOutput(" Stop on all exceptions");
                        break;
                    default: CommandBase.WriteOutput(" " + Enum.GetName(typeof(DebuggerBehavior), m_default)
                        + " all exceptions");
                        break;
                }
            }
            else
            {
                // print default behavior and all other settings specified, in order of increasing precedence
                CommandBase.WriteOutput("Default: " + Enum.GetName(typeof(DebuggerBehavior), m_default));

                foreach (ExceptionStopOptionPolicyItem item in m_items)
                {
                    CommandBase.WriteOutput(item.ExceptionType + ": " +
                        Enum.GetName(typeof(DebuggerBehavior), item.Behavior));
                }
            }
        }

        /// <summary>
        /// Given an exception thrown by a process, determines whether the debugger should 
        /// log, ignore, or stop on this exception, based on the type of exception and the
        /// existing exception settings. 
        /// </summary>
        /// <returns>stop, log, or ignore</returns>
        protected DebuggerBehavior DetermineBehavior(string exceptionType)
        {
            ExceptionStopOptionPolicyItem currentItem;
            string regex;

            // Check entries in m_items to see if this exception type or a regular expression
            // matching it already exists in m_items. We loop backwards through the array because
            // entries added later have precedence over those added earlier.
            for (int i = m_items.Count - 1; i >= 0; i--)
            {
                currentItem = m_items[i];
                if (MDbgUtil.IsRegex(currentItem.ExceptionType))
                {
                    // current entry in m_items is a regular expression
                    regex = MDbgUtil.ConvertSimpleExpToRegExp(currentItem.ExceptionType);
                    if (Regex.IsMatch(exceptionType, regex))
                    {
                        return currentItem.Behavior;
                    }
                }
                else
                {
                    if (currentItem.ExceptionType == exceptionType)
                    {
                        return currentItem.Behavior;
                    }
                }
            }

            return m_default;
        }

        /// <summary>
        /// Returns the number of ExceptionStopOptionPolicyItem objects in m_items.
        /// </summary>
        public int ItemsCount()
        {
            return m_items.Count;
        }

        /// <summary>
        /// A master switch to control whether the debugger should act on all exception callbacks.
        /// If it is set to true, the debugger acts on all exception callbacks. If it is set to 
        /// false, the debugger acts only on DEBUG_EXCEPTION_FIRST_CHANCE callbacks.
        /// </summary>
        public bool ExceptionEnhancedOn
        {
            get
            {
                return m_exceptionEnhancedOn;
            }
            set
            {
                m_exceptionEnhancedOn = value;
            }
        }

        /// <summary>
        /// Default behavior.
        /// </summary>
        public DebuggerBehavior Default
        {
            get
            {
                return m_default;
            }
        }

        private DebuggerBehavior m_default;
        private bool m_exceptionEnhancedOn;
        private List<ExceptionStopOptionPolicyItem> m_items;
    }

    /// <summary>
    ///  Item in ExceptionStopOptionPolicy. Details debugger behavior for specific types of exceptions.
    /// </summary>
    public class ExceptionStopOptionPolicyItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="exceptionType">An exception type or regular expression.</param>
        /// <param name="behavior">Debugger behavior - stop, log, or ignore</param>
        public ExceptionStopOptionPolicyItem(string exceptionType, MDbgStopOptionPolicy.DebuggerBehavior behavior)
        {
            m_exceptionType = exceptionType;
            m_behavior = behavior;
        }

        /// <value>
        ///   Exception type, or regular expression.
        /// </value>
        public string ExceptionType
        {
            get
            {
                return m_exceptionType;
            }
            set
            {
                m_exceptionType = value;
            }
        }

        /// <value>
        ///   Behavior of debugger when it encounters exception of type ExceptionType,
        ///   or matching the regular expression ExceptionType.
        /// </value>
        public MDbgStopOptionPolicy.DebuggerBehavior Behavior
        {
            get
            {
                return m_behavior;
            }
            set
            {
                m_behavior = value;
            }
        }

        private string m_exceptionType;
        private MDbgStopOptionPolicy.DebuggerBehavior m_behavior;
    }

    /// <summary>
    /// Used to control the ExceptionEnhanced switch in a given ExceptionStopOptionPolicy object. 
    /// This switch controls whether the debugger should act on all exception callbacks.
    /// If it is set to true, the debugger acts on all exception callbacks. If it is set to 
    /// false, the debugger acts only on DEBUG_EXCEPTION_FIRST_CHANCE callbacks.
    /// </summary>
    public class ExceptionEnhancedStopOptionPolicy : MDbgStopOptionPolicy
    {
        /// <summary>
        /// Default constructor. Sets the acronym to "ee".
        /// </summary>
        /// <param name="esop">The ExceptionStopOptionPolicy whose switch to control.</param>
        public ExceptionEnhancedStopOptionPolicy(ExceptionStopOptionPolicy esop)
        {
            m_esop = esop;
            Acronym = "ee";
        }

        /// <summary>
        /// Does nothing - an ExceptionEnhancedStopOptionPolicy object  is only meant to control the 
        /// ExceptionEnhanced switch in an ExceptionStopOptionPolicy object, not to directly stop the 
        /// debugger or to log a callback. 
        /// </summary>
        /// <param name="currentProcess">Current MDbgProcess.</param>
        /// <param name="args">Callback arguments.</param>
        public override void ActOnCallback(MDbgProcess currentProcess, CustomPostCallbackEventArgs args)
        {

        }

        /// <summary>
        /// Sets the ExceptionEnhanced switch of the associated ExceptionStopOptionPolicy object
        /// on or off. 
        /// </summary>
        /// <param name="behavior">DebuggerBehaviors.stop turns ON ExceptionEnhanced switch of the associated 
        /// ExceptionStopOptionPolicy object, and DebuggerBehaviors.ignore turns the switch OFF. The command 
        /// DebuggerBehaviors.log is not supported.</param>
        /// <param name="arguments">Must be null.</param>
        public override void SetBehavior(DebuggerBehavior behavior, string arguments)
        {
            if (arguments != null)
            {
                throw new MDbgShellException("This event type does not accept arguments.");
            }
            switch (behavior)
            {
                case DebuggerBehavior.Stop:
                    m_esop.ExceptionEnhancedOn = true;
                    break;
                case DebuggerBehavior.Ignore:
                    m_esop.ExceptionEnhancedOn = false;
                    break;
                case DebuggerBehavior.Log:
                    throw new MDbgShellException("ExceptionEnhanced can only be switched on and off, using the catch and ignore commands.");
            }

            // To preserve legacy behavior, if m_esop contains no MDbgExceptionPolicyItem objects
            // describing debugger behaviors for specific exception types, and the default behavior
            // is not log, then the command "catch ee" will set the default behavior of m_esop
            // to stop, and "ignore ee" will set it to ignore.

            if ((m_esop.ItemsCount() == 0) &&
                (behavior == DebuggerBehavior.Stop || behavior == DebuggerBehavior.Ignore) &&
                (m_esop.Default != DebuggerBehavior.Log))
            {
                m_esop.SetBehavior(behavior, null);
            }

        }

        /// <summary>
        /// Prints whether the ExceptionEnhanced switch of the associated ExceptionStopOptionPolicy object
        /// is turned on or off.
        /// </summary>
        public override void Print()
        {
            CommandBase.WriteOutput("(ee) ExceptionEnhanced:  " + m_esop.ExceptionEnhancedOn.ToString());
        }

        private ExceptionStopOptionPolicy m_esop;
    }
}
