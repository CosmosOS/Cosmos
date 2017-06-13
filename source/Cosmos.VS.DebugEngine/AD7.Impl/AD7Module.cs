using System;
using Cosmos.VS.DebugEngine.Engine.Impl;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    // this class represents a module loaded in the debuggee process to the debugger. 
    public class AD7Module : IDebugModule2, IDebugModule3 {
        //public readonly DebuggedModule DebuggedModule;

        public AD7Module() { //DebuggedModule debuggedModule) 
            //this.DebuggedModule = debuggedModule;
        }

        // Gets the MODULE_INFO that describes this module.
        // This is how the debugger obtains most of the information about the module.
        int IDebugModule2.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] infoArray)
        {
            try
            {
                MODULE_INFO info = new MODULE_INFO();

                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_NAME))
                {
                    //info.m_bstrName = System.IO.Path.GetFileName(this.DebuggedModule.Name);
                    info.m_bstrName = "DEADBEEF";
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_URL))
                {
                    //info.m_bstrUrl = this.DebuggedModule.Name;
                    info.m_bstrUrl = "DEADBEEF";
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URL;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS))
                {
                    //info.m_addrLoadAddress = (ulong)this.DebuggedModule.BaseAddress;
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS))
                {
                    // A debugger that actually supports showing the preferred base should crack the PE header and get 
                    // that field. This debugger does not do that, so assume the module loaded where it was suppose to.                   
                    //info.m_addrPreferredLoadAddress = (ulong)this.DebuggedModule.BaseAddress;
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_SIZE))
                {
                    //info.m_dwSize = this.DebuggedModule.Size;
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_SIZE;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_LOADORDER))
                {
                    //info.m_dwLoadOrder = this.DebuggedModule.GetLoadOrder();
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADORDER;
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION))
                {
                    //if (this.DebuggedModule.SymbolsLoaded)
                    {
                        //info.m_bstrUrlSymbolLocation = this.DebuggedModule.SymbolPath;
                        info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
                    }
                }
                if (dwFields.HasFlag(enum_MODULE_INFO_FIELDS.MIF_FLAGS))
                {
                    info.m_dwModuleFlags = 0;
                    //if (this.DebuggedModule.SymbolsLoaded)
                    {
                        info.m_dwModuleFlags |= (enum_MODULE_FLAGS.MODULE_FLAG_SYMBOLS);
                    }
                    info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
                }
                
                
                infoArray[0] = info;

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

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented
        int IDebugModule2.ReloadSymbols_Deprecated(string urlToSymbols, out string debugMessage) {
            debugMessage = null;
            System.Diagnostics.Debug.Fail("This function is not called by the debugger.");
            return VSConstants.E_NOTIMPL;
        }

        int IDebugModule3.ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage) {
            throw new NotImplementedException();
        }
        #endregion

        // IDebugModule3 represents a module that supports alternate locations of symbols and JustMyCode states.
        // The sample does not support alternate symbol locations or JustMyCode, but it does not display symbol load information 

        // Gets the MODULE_INFO that describes this module.
        // This is how the debugger obtains most of the information about the module.
        int IDebugModule3.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            return ((IDebugModule2)this).GetInfo(dwFields, pinfo);
        }

        // Returns a list of paths searched for symbols and the results of searching each path.
        int IDebugModule3.GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            // This engine only supports loading symbols at the location specified in the binary's symbol path location in the PE file and
            // does so only for the primary exe of the debuggee.
            // Therefore, it only displays if the symbols were loaded or not. If symbols were loaded, that path is added.
            pinfo[0] = new MODULE_SYMBOL_SEARCH_INFO();
            pinfo[0].dwValidFields = 1; // SSIF_VERBOSE_SEARCH_INFO;

            //if (this.DebuggedModule.SymbolsLoaded)
            {
                //string symbolPath = "Symbols Loaded - " + this.DebuggedModule.SymbolPath;
                //pinfo[0].bstrVerboseSearchInfo = symbolPath;
            }
            //else
            {
                string symbolsNotLoaded = "Symbols not loaded";
                pinfo[0].bstrVerboseSearchInfo = symbolsNotLoaded;
            }
            return VSConstants.S_OK;
        }

        // Used to support the JustMyCode features of the debugger.
        // the sample debug engine does not support JustMyCode and therefore all modules
        // are considered "My Code"
        int IDebugModule3.IsUserCode(out int pfUser)
        {
            pfUser = 1;
            return VSConstants.S_OK;           
        }

        // Loads and initializes symbols for the current module when the user explicitly asks for them to load.
        // The sample engine only supports loading symbols from the location pointed to by the PE file which will load
        // when the module is loaded.
        int IDebugModule3.LoadSymbols()
        {
            throw new NotImplementedException();
        }

        // Used to support the JustMyCode features of the debugger.
        // The sample engine does not support JustMyCode so this is not implemented
        int IDebugModule3.SetJustMyCodeState(int fIsUserCode)
        {
            throw new NotImplementedException();
        }

    }
}