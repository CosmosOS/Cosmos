using System;
using Cosmos.VS.DebugEngine.Engine.Impl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    // And implementation of IDebugCodeContext2 and IDebugMemoryContext2. 
    // IDebugMemoryContext2 represents a position in the address space of the machine running the program being debugged.
    // IDebugCodeContext2 represents the starting position of a code instruction. 
    // For most run-time architectures today, a code context can be thought of as an address in a program's execution stream.
    public class AD7MemoryAddress : IDebugCodeContext2
    {
        readonly AD7Engine m_engine;
        readonly uint m_address;
        IDebugDocumentContext2 m_documentContext;
        
        public AD7MemoryAddress(AD7Engine engine, uint address)
        {
            m_engine = engine;
            m_address = address;
        }

        public void SetDocumentContext(IDebugDocumentContext2 docContext)
        {
            m_documentContext = docContext;
        }

        // Adds a specified value to the current context's address to create a new context.
        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(m_engine, (uint)dwCount + m_address);
            return VSConstants.S_OK;
        }

        // Compares the memory context to each context in the given array in the manner indicated by compare flags, 
        // returning an index of the first context that matches.
        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            pdwMemoryContext = UInt32.MaxValue;

            try
            {
                for (uint c = 0; c < dwMemoryContextSetLen; c++)
                {
                    if (!(rgpMemoryContextSet[c] is AD7MemoryAddress compareTo))
                    {
                        continue;
                    }

                    if (!ReferenceEquals(m_engine, compareTo.m_engine))
                    {
                        continue;
                    }

                    bool result;

                    switch (Compare)
                    {
                        case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                            result = (m_address == compareTo.m_address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                            result = (m_address < compareTo.m_address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                            result = (m_address > compareTo.m_address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                            result = (m_address <= compareTo.m_address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                            result = (m_address >= compareTo.m_address);
                            break;

                        // The sample debug engine doesn't understand scopes or functions
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                            result = (m_address == compareTo.m_address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                            result = (m_address == compareTo.m_address);
                            if (result == false)
                            {
                                //DebuggedModule module = m_engine.DebuggedProcess.ResolveAddress(m_address);

                                //if (module != null)
                                //{
                                //    result = (compareTo.m_address >= module.BaseAddress) &&
                                //        (compareTo.m_address < module.BaseAddress + module.Size);
                                //}
                            }
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                            result = true;
                            break;

                        default:
                            // A new comparison was invented that we don't support
                            return VSConstants.E_NOTIMPL;
                    }

                    if (result)
                    {
                        pdwMemoryContext = c;
                        return VSConstants.S_OK;
                    }
                }

                return VSConstants.S_FALSE;
            }
            //catch (ComponentException e)
            //{
            //    return e.HResult;
            //}
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        // Gets information that describes this context.
        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {           
            try
            {
                pinfo[0].dwFields = 0;

                if (dwFields.HasFlag(enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS))
                {
                    pinfo[0].bstrAddress = m_address.ToString();
                    pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
                }

                // Fields not supported by the sample
                //if ((dwFields & (uint)enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0){}
                //if ((dwFields & (uint)enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0){}
                //if ((dwFields & (uint)enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0){}
                //if ((dwFields & (uint)enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0) {}
                //if ((dwFields & (uint)enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0) {}

                return VSConstants.S_OK;
            }
            //catch (ComponentException e)
            //{
            //    return e.HResult;
            //}
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        // Gets the user-displayable name for this context
        // This is not supported by the sample engine.
        public int GetName(out string pbstrName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // Subtracts a specified value from the current context's address to create a new context.
        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(m_engine, (uint)dwCount - m_address);
            return VSConstants.S_OK;
        }

        // Gets the document context for this code-context
        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = m_documentContext;
            return VSConstants.S_OK;
        }

        // Gets the language information for this code context.
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (m_documentContext != null)
            {
                m_documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.S_FALSE;
            }
        }

    }
}
