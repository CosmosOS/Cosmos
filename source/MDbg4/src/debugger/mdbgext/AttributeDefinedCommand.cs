//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Resources;
using System.Diagnostics;

namespace Microsoft.Samples.Tools.Mdbg
{

    /// <summary>
    /// This attribute describes the command.
    /// </summary>
    [
     AttributeUsage(
        AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = false
        )
    ]
    public sealed class CommandDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Returns the command name.
        /// </summary>
        /// <value>Name of the command.</value>
        public string CommandName
        {
            get
            {
                return m_commandName;
            }
            set
            {
                m_commandName = value;
                m_minimumAbbrev = m_commandName.Length;
            }
        }

        /// <summary>
        /// Returns the minimum number of characters you must use to invoke this command.
        /// </summary>
        /// <value>The minimum number of characters.</value>
        public int MinimumAbbrev
        {
            get
            {
                return m_minimumAbbrev;
            }
            set
            {
                m_minimumAbbrev = value;
            }
        }

        /// <summary>
        /// Returns a brief help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        public string ShortHelp
        {
            get
            {
                if (m_resourceMgrKey != null)
                {
                    ResourceManager rm = (ResourceManager)m_resources[m_resourceMgrKey];
                    if (rm == null)
                    {
                        return m_shortHelp;
                    }
                    string key = (UseHelpFrom != null) ? UseHelpFrom : m_commandName;
                    return rm.GetString(key + "_ShortHelp", System.Globalization.CultureInfo.CurrentUICulture);
                }
                else
                {
                    return m_shortHelp;
                }
            }
            set
            {
                m_shortHelp = value;
            }
        }

        /// <summary>
        /// Returns a more detailed help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        public string LongHelp
        {
            get
            {
                if (m_resourceMgrKey != null)
                {
                    ResourceManager rm = (ResourceManager)m_resources[m_resourceMgrKey];
                    string key = UseHelpFrom != null ? UseHelpFrom : m_commandName;
                    return rm.GetString(key + "_LongHelp", System.Globalization.CultureInfo.CurrentUICulture);
                }
                else
                {
                    return m_longHelp == null ? "usage: \n" + m_shortHelp : m_longHelp;
                }
            }
            set
            {
                m_longHelp = value;
            }
        }

        /// <summary>
        /// Returns if the command is repeatable (hitting enter again will repeat these commands)
        /// </summary>
        /// <value>true if the command is repeatable</value>
        public bool IsRepeatable
        {
            get
            {
                return m_isRepeatable;
            }
            set
            {
                m_isRepeatable = value;
            }
        }

        /// <summary>
        /// Gets or sets the Resource Manager Key.
        /// </summary>
        /// <value>The Resource Manager Key.</value>
        public Type ResourceManagerKey
        {
            get
            {
                return m_resourceMgrKey;
            }
            set
            {
                Debug.Assert(value != null);

                m_resourceMgrKey = value;
            }
        }

        /// <summary>
        /// Gets or sets where to get the help from.
        /// </summary>
        /// <value>Where to get the help from.</value>
        public string UseHelpFrom
        {
            get
            {
                return m_useHelpFrom;
            }
            set
            {
                m_useHelpFrom = value;
            }
        }

        /// <summary>
        /// Registers the Resource Manager.
        /// </summary>
        /// <param name="key">Whet Type to use.</param>
        /// <param name="resourceManager">Which Resource Manager to register.</param>
        public static void RegisterResourceMgr(Type key, System.Resources.ResourceManager resourceManager)
        {
            Debug.Assert(resourceManager != null && key != null);
            if (key == null || resourceManager == null)
            {
                throw new ArgumentException();
            }

            if (m_resources == null)
            {
                m_resources = new Hashtable();
            }
            if (m_resources.ContainsKey(key))
            {
                throw new ArgumentException("key already registered");
            }

            m_resources.Add(key, resourceManager);
        }

        private static Hashtable m_resources;

        private string m_commandName;
        private int m_minimumAbbrev;
        private string m_shortHelp;
        private string m_longHelp;
        private bool m_isRepeatable = true;
        private Type m_resourceMgrKey = null;
        private string m_useHelpFrom = null;
    }

    /// <summary>
    /// This class defines MDbg Attribute Defined Commands.
    /// </summary>
    public sealed class MDbgAttributeDefinedCommand : IMDbgCommand, IEquatable<IMDbgCommand>
    {

        private MDbgAttributeDefinedCommand(MethodInfo methodInfo, CommandDescriptionAttribute descriptionAttribute)
        {
            m_mi = methodInfo;
            m_cmdDescr = descriptionAttribute;
            if (!g_extensions.Contains(LoadedFrom))
            {
                g_extensions.Add(LoadedFrom, g_freeAssemblySeqNumber++);
            }
        }

        /// <summary>
        /// Adds commands from type.
        /// </summary>
        /// <param name="commandSet">Command Set to add.</param>
        /// <param name="type">Type to add commands for.</param>
        public static void AddCommandsFromType(IMDbgCommandCollection commandSet, Type type)
        {
            foreach (MethodInfo mi in type.GetMethods())
            {
                object[] attribs = mi.GetCustomAttributes(typeof(CommandDescriptionAttribute), false);
                if (attribs != null)
                {
                    foreach (object o in attribs)
                    {
                        if (o is CommandDescriptionAttribute)
                        {
                            MDbgAttributeDefinedCommand cmd =
                                new MDbgAttributeDefinedCommand(mi, (CommandDescriptionAttribute)o);
                            Debug.Assert(cmd != null);
                            commandSet.Add(cmd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes commands from type
        /// </summary>
        /// <param name="commandSet">Command Set to remove.</param>
        /// <param name="type">Type to remove commands for.</param>
        public static void RemoveCommandsFromType(IMDbgCommandCollection commandSet, Type type)
        {
            foreach (MethodInfo mi in type.GetMethods())
            {
                object[] attribs = mi.GetCustomAttributes(typeof(CommandDescriptionAttribute), false);
                if (attribs != null)
                {
                    foreach (object o in attribs)
                    {
                        if (o is CommandDescriptionAttribute)
                        {
                            MDbgAttributeDefinedCommand cmd =
                                new MDbgAttributeDefinedCommand(mi, (CommandDescriptionAttribute)o);
                            Debug.Assert(cmd != null);
                            commandSet.Remove(cmd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the command name.
        /// </summary>
        /// <value>Name of the command.</value>
        public string CommandName
        {
            get
            {
                return m_cmdDescr.CommandName;
            }
        }

        /// <summary>
        /// Returns the minimum number of characters you must use to invoke this command.
        /// </summary>
        /// <value>The minimum number of characters.</value>
        public int MinimumAbbrev
        {
            get
            {
                return m_cmdDescr.MinimumAbbrev;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="arguments">Arguments to pass to the command.</param>
        public void Execute(string arguments)
        {
            m_mi.Invoke(null, new object[] { arguments });
        }

        /// <summary>
        /// Returns a brief help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        public string ShortHelp
        {
            get
            {
                return m_cmdDescr.ShortHelp;
            }
        }

        /// <summary>
        /// Returns a more detailed help message for the command.
        /// </summary>
        /// <value>The help message.</value>
        public string LongHelp
        {
            get
            {
                return m_cmdDescr.LongHelp;
            }
        }

        /// <summary>
        /// Assembly the command was loaded from
        /// </summary>
        /// <value>The Assembly.</value>
        public Assembly LoadedFrom
        {
            get
            {
                return m_mi.DeclaringType.Assembly;
            }
        }

        /// <summary>
        /// Returns if the command is repeatable (hitting enter again will repeat these commands)
        /// </summary>
        /// <value>true if the command is repeatable</value>
        public bool IsRepeatable
        {
            get
            {
                return m_cmdDescr.IsRepeatable;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            // we'll sort the commands first by extensions by which they were loaded
            // and then by it's name.
            if (!(obj is IMDbgCommand))
            {
                return 1;                                   // unknown types always at the start of the list
            }

            IMDbgCommand other = obj as IMDbgCommand;
            if (this.LoadedFrom != other.LoadedFrom)
            {
                return (int)g_extensions[this.LoadedFrom] - (int)g_extensions[other.LoadedFrom];
            }

            // commands are from same extension
            return String.Compare(CommandName, (obj as IMDbgCommand).CommandName);
        }

        public bool Equals(IMDbgCommand cmd)
        {
            // We could be a little more thorough in our equivalence checks, but LoadedFrom and CommandName 
            // are the most telling
            if (this.CommandName != cmd.CommandName || this.LoadedFrom != cmd.LoadedFrom)
            {
                return false;
            }
            return true;
        }


        private readonly MethodInfo m_mi;
        private readonly CommandDescriptionAttribute m_cmdDescr;

        private static int g_freeAssemblySeqNumber = 0;
        private static readonly ListDictionary g_extensions = new ListDictionary();
    }
}
