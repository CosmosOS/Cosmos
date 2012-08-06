//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;

using Microsoft.Samples.Debugging.MdbgEngine;

namespace Microsoft.Samples.Tools.Mdbg
{
    class MDbgSourceFileMgr : IMDbgSourceFileMgr
    {
        public IMDbgSourceFile GetSourceFile(string path)
        {
            String s = String.Intern(path);
            MDbgSourceFile source = (MDbgSourceFile) m_sourceCache[s];

            if ( source == null )
            {
                source = new MDbgSourceFile(s);
                m_sourceCache.Add(s,source);
            }
            return source;
        }

        public void ClearDocumentCache()
        {
            m_sourceCache.Clear();
        }

        private Hashtable m_sourceCache = new Hashtable();
    }

    class MDbgSourceFile : IMDbgSourceFile
    {
        public MDbgSourceFile(string path)
        {
            m_path = path;
            try
            {
                Initialize();
            }
            catch(FileNotFoundException)
            {
                throw new MDbgShellException("Could not find source: "+m_path);
            }
        }
        
        public string Path
        { 
            get
            {
                return m_path;
            }
        }
        
        public string this[ int lineNumber ]
        {
            get
            {
                if( m_lines == null )
                {
                    Initialize();
                }
                if( (lineNumber<1) || (lineNumber>m_lines.Count) )
                    throw new MDbgShellException(string.Format("Could not retrieve line {0} from file {1}.",
                                                               lineNumber, this.Path));

                return (string) m_lines[lineNumber-1];
            }
        }
        
        public int Count
        {
            get
            {
                if( m_lines == null )
                {
                    Initialize();
                }
                return m_lines.Count;
            }
        }

        protected  void Initialize()
        {
            StreamReader sr = null;
            try
            {                
                // Encoding.Default doesn’t port between machines, but it's used just in case the source isn’t Unicode
                sr = new StreamReader(m_path, System.Text.Encoding.Default, true);
                m_lines = new ArrayList();
                
                string s = sr.ReadLine();
                while(s!=null)
                {
                    m_lines.Add(s);
                    s = sr.ReadLine();
                }
            }
            finally
            {
                if( sr!=null )
                    sr.Close(); // free resources in advance
            }
        }

        private ArrayList m_lines;
        private string m_path;

    }
}
