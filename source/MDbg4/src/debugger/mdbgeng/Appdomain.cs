//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

using Microsoft.Samples.Debugging.CorDebug;
using Microsoft.Samples.Debugging.CorMetadata;


namespace Microsoft.Samples.Debugging.MdbgEngine
{
    /// <summary>
    /// Contains a collection of appdomains created in debugged program.
    /// </summary>
    public sealed class MDbgAppDomainCollection : MarshalByRefObject, IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            MDbgAppDomain[] ret = new MDbgAppDomain[m_items.Count];
            m_items.Values.CopyTo(ret,0);
            Array.Sort(ret);
            return ret.GetEnumerator();
        }
        
        /// <value>Number of appdomains in the collection</value>
        public int Count
        {
            get
            {
                return m_items.Count;
            }
        }

        /// <summary>
        /// Returns an appdomain from its name. 
        /// </summary>
        /// <value>
        ///     Returns null if there is no appdomain with passed-in name.
        /// </value>
        /// <exception cref="MDbgAmbiguousModuleNameException">
        /// Thrown when there are multiple appdomains with same name.
        /// </exception>
        public MDbgAppDomain this[string appDomainName]
        {
            get
            {
                MDbgAppDomain appDomain = null;
                foreach(MDbgAppDomain ad in m_items.Values)
                {
                    if(String.Compare(ad.CorAppDomain.Name,appDomainName)==0)
                    {
                        if(appDomain==null)
                            appDomain = ad;
                        else
                            throw new MDbgAmbiguousModuleNameException();
                    }
                }
                return appDomain;
            }
        }

        /// <summary>
        /// Allows for indexing into the Collection by appDomainNumber
        /// </summary>
        /// <param name="appDomainNumber">Which appDomainNumber to access.</param>
        /// <returns>The MDbgAppDomain with the given number.</returns>
        public MDbgAppDomain this[int appDomainNumber]
        {
            get
            {
                MDbgAppDomain appDomain = null;
                foreach(MDbgAppDomain ad in m_items.Values)
                {
                    if( ad.Number == appDomainNumber )
                    {
                        appDomain = ad;
                        break;
                    }
                }
                return appDomain;
            }
        }

        /// <summary>
        /// Locates MDbgAppDomain object from CorAppDomain object.
        /// </summary>
        /// <param name="appDomain">appDomain object from CorXXX layer.</param>
        /// <returns>MdbgAppDomain object</returns>
        public MDbgAppDomain Lookup(CorAppDomain appDomain)
        {
            return (MDbgAppDomain)m_items[appDomain];
        }


        internal MDbgAppDomainCollection(MDbgProcess process)
        {
            Debug.Assert(process!=null);
            m_process = process;
        }

        internal MDbgAppDomain Register(CorAppDomain appDomain)
        {
            MDbgAppDomain mdbgAppDomain;
            
            // appdomains may get registered mutliple times if we get a fake-attach event right before a real event.
            if (!m_items.Contains(appDomain))
            {
                mdbgAppDomain = new MDbgAppDomain(m_process, appDomain, m_freeAppDomainNumber++);
                m_items.Add(appDomain, mdbgAppDomain);
                return mdbgAppDomain;
            }
            return (MDbgAppDomain) m_items[appDomain];
        }

        internal void Unregister(CorAppDomain appDomain)
        {
            Debug.Assert(m_items.ContainsKey(appDomain));
            m_items.Remove(appDomain);
        }

        private Hashtable m_items = new Hashtable();
        private MDbgProcess m_process;
        private int m_freeAppDomainNumber=0;
    }

    /// <summary>
    /// Class represents an application domain in the debugged program.
    /// </summary>
    public sealed class MDbgAppDomain : MarshalByRefObject, IComparable
    {
        /// <value>
        ///     Process in which current appdomain exists.
        /// </value>
        public MDbgProcess Process
        {
            get
            {
                return m_process;
            }
        }
        
        /// <value>
        ///     CorXXX object for the AppDomain.
        /// </value>
        public CorAppDomain CorAppDomain
        {
            get
            {
                return m_appDomain;
            }
        }

        /// <summary>
        /// AppDomain logical number. 
        /// </summary>
        /// Appdomains in debugged process are assigned
        /// unique logical number. First created appdomain in the process gets logical
        /// number 0, second receives number 1, etc...
        /// Logical numbers are never reused in a process.
        /// <value>
        ///     Logical number for the appdomain.
        /// </value>
        public int Number
        {
            get
            {
                return m_number;
            }
        }

        internal MDbgAppDomain(MDbgProcess process,CorAppDomain appDomain,int number)
        {
            Debug.Assert(process!=null);
            Debug.Assert(appDomain!=null);
            m_process = process;
            m_appDomain = appDomain;
            m_number = number;
        }

        int IComparable.CompareTo(object obj)
        {
            return this.Number - (obj as MDbgAppDomain).Number;
        }

        private int m_number;
        private MDbgProcess m_process;
        private CorAppDomain m_appDomain;
    }
}
