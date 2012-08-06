//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Text;

using Microsoft.Samples.Debugging.CorPublish.NativeApi;

namespace Microsoft.Samples.Debugging.CorPublish
{
    public sealed class CorPublish
    {
        public CorPublish()
        {
            m_publish = new CorpubPublishClass();
        }

        public IEnumerable EnumProcesses()
        {
            ICorPublishProcessEnum pIEnum;
            m_publish.EnumProcesses(COR_PUB_ENUMPROCESS.COR_PUB_MANAGEDONLY,out pIEnum);
            return (pIEnum==null)?null:new CorPublishProcessEnumerator(pIEnum);
        }

        public CorPublishProcess GetProcess(int pid)
        {
            ICorPublishProcess proc;
            m_publish.GetProcess((uint)pid,out proc);
            return (proc==null)?null:new CorPublishProcess(proc);
        }
        

        private ICorPublish m_publish;
    }

    public sealed class CorPublishProcess
    {
        internal CorPublishProcess(ICorPublishProcess iprocess)
        {
            m_process = iprocess;
        }

        public IEnumerable EnumAppDomains()
        {
            ICorPublishAppDomainEnum pIEnum;
            m_process.EnumAppDomains(out pIEnum);
            return (pIEnum==null)?null:new CorPublishAppDomainEnumerator(pIEnum);
        }

        public string DisplayName
        {
            get
            {
                uint size;
                m_process.GetDisplayName(0, out size, null);
                StringBuilder szName = new StringBuilder((int)size);
                m_process.GetDisplayName((uint)szName.Capacity, out size, szName);
                return szName.ToString();
            }
        }
        
        public int ProcessId
        {
            get
            {
                uint pid;
                m_process.GetProcessID(out pid);
                return (int)pid;
            }
        }

        public bool IsManaged
        {
            get
            {
                int bManaged;
                m_process.IsManaged(out bManaged);
                return (bManaged!=0);
            }
        }

        private ICorPublishProcess m_process;
    }
    
    internal class CorPublishProcessEnumerator : 
        IEnumerable, IEnumerator, ICloneable
    {
        internal CorPublishProcessEnumerator(ICorPublishProcessEnum e)
        {
            m_enum = e;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorPublishEnum clone = null;
            m_enum.Clone (out clone);
            return new CorPublishProcessEnumerator((ICorPublishProcessEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator ()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext ()
        {
            ICorPublishProcess a;
            uint c = 0;
            int r = m_enum.Next ((uint) 1,out a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_proc = new CorPublishProcess(a);
            else
                m_proc = null;
            return m_proc != null;
        }

        public void Reset ()
        {
            m_enum.Reset();
            m_proc = null;
        }

        public Object Current
        {
            get
            {
                return m_proc;
            }
        }

        private ICorPublishProcessEnum m_enum;
        private CorPublishProcess m_proc;
    }

    public sealed class CorPublishAppDomain
    {
        internal CorPublishAppDomain(ICorPublishAppDomain appDomain)
        {
            m_appDomain = appDomain;
        }

        public int Id
        {
            get
            {
                uint id;
                m_appDomain.GetID(out id);
                return (int)id;
            }
        }

        public string Name
        {
            get
            {
                uint size;
                m_appDomain.GetName(0,out size, null);
                StringBuilder szName = new StringBuilder((int)size);
                m_appDomain.GetName((uint)szName.Capacity,out size, szName);
                return szName.ToString();
            }
        }

        private ICorPublishAppDomain m_appDomain;
    }


    internal class CorPublishAppDomainEnumerator : 
        IEnumerable, IEnumerator, ICloneable
    {
        internal CorPublishAppDomainEnumerator(ICorPublishAppDomainEnum appDomainEnumerator)
        {
            m_enum = appDomainEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone ()
        {
            ICorPublishEnum clone = null;
            m_enum.Clone (out clone);
            return new CorPublishAppDomainEnumerator((ICorPublishAppDomainEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator ()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext ()
        {
            ICorPublishAppDomain a;
            uint c = 0;
            int r = m_enum.Next ((uint) 1, out a, out c);
            if (r==0 && c==1) // S_OK && we got 1 new element
                m_appDomain = new CorPublishAppDomain(a);
            else
                m_appDomain = null;
            return m_appDomain != null;
        }

        public void Reset ()
        {
            m_enum.Reset();
            m_appDomain = null;
        }

        public Object Current
        {
            get
            {
                return m_appDomain;
            }
        }

        private ICorPublishAppDomainEnum m_enum;
        private CorPublishAppDomain m_appDomain;
    }
}
