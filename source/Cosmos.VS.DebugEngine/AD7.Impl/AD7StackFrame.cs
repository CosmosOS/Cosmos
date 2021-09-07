using System;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using Cosmos.VS.DebugEngine.AD7.Definitions;
using Cosmos.VS.DebugEngine.Engine.Impl;
using IL2CPU.Debug.Symbols;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
  // Represents a logical stack frame on the thread stack.
  // Also implements the IDebugExpressionContext interface, which allows expression evaluation and watch windows.
  public class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2
  {
    readonly AD7Engine mEngine;
    readonly AD7Thread mThread;
    //readonly X86ThreadContext m_threadContext;

    private string mDocName;
    private string mFunctionName;
    private uint mLineNum;
    private bool mHasSource;

    // Must have empty holders, some code looks at length and can run
    // before we set them.
    internal LOCAL_ARGUMENT_INFO[] mLocalInfos = Array.Empty<LOCAL_ARGUMENT_INFO>();
    internal LOCAL_ARGUMENT_INFO[] mArgumentInfos = Array.Empty<LOCAL_ARGUMENT_INFO>();

    // An array of this frame's parameters
    private DebugLocalInfo[] mParams;

    // An array of this frame's locals
    private DebugLocalInfo[] mLocals;
    private AD7Process mProcess;

    public AD7StackFrame(AD7Engine aEngine, AD7Thread aThread, AD7Process aProcess)
    {
      mEngine = aEngine;
      mThread = aThread;
      mProcess = aProcess;
      var xProcess = mEngine.mProcess;
      if (mHasSource = xProcess.mCurrentAddress.HasValue)
      {
        var xAddress = xProcess.mCurrentAddress.Value;
        var xSourceInfos = xProcess.mDebugInfoDb.GetSourceInfos(xAddress);
        if (!xSourceInfos.ContainsKey(xAddress))
        {
          //Attempt to find the ASM address of the first ASM line of the C# line that contains
          //the current ASM address line

          // Because of Asm breakpoints the address we have might be in the middle of a C# line.
          // So we find the closest address to ours that is less or equal to ours.
          var xQry = from x in xSourceInfos
                     where x.Key <= xAddress
                     orderby x.Key descending
                     select x.Key;
          if (xQry.Count() > 0)
          {
            xAddress = xQry.First();
          }
        }
        if (mHasSource = xSourceInfos.ContainsKey(xAddress))
        {
          var xSourceInfo = xSourceInfos[xAddress];
          mDocName = xSourceInfo.SourceFile;
          mFunctionName = xSourceInfo.MethodName;
          mLineNum = (uint)xSourceInfo.LineStart;

          // Multiple labels that point to a single address can happen because of exception handling exits etc.
          // Because of this given an address, we might find more than one label that matches the address.
          // Currently, the label we are looking for will always be the first one so we choose that one.
          // In the future this might "break", so be careful about this. In the future we may need to classify
          // labels in the output and mark them somehow.
          var xLabelsForAddr = xProcess.mDebugInfoDb.GetLabels(xAddress);
          if (xLabelsForAddr.Length > 0)
          {
            MethodIlOp xSymbolInfo;
            string xLabel = xLabelsForAddr[0]; // Necessary for LINQ
            xSymbolInfo = aProcess.mDebugInfoDb.TryGetFirstMethodIlOpByLabelName(xLabel);
            if (xSymbolInfo != null)
            {
              var xMethod = mProcess.mDebugInfoDb.GetMethod(xSymbolInfo.MethodID.Value);
              var xAllInfos = aProcess.mDebugInfoDb.GetAllLocalsAndArgumentsInfosByMethodLabelName(xMethod.LabelCall);
              mLocalInfos = xAllInfos.Where(q => !q.IsArgument).ToArray();
              mArgumentInfos = xAllInfos.Where(q => q.IsArgument).ToArray();
              if (mArgumentInfos.Length > 0)
              {
                mParams = new DebugLocalInfo[mArgumentInfos.Length];
                for (int i = 0; i < mArgumentInfos.Length; i++)
                {
                  mParams[i] = new DebugLocalInfo
                  {
                    Name = mArgumentInfos[i].NAME,
                    Index = i,
                    IsLocal = false
                  };
                }
                mParams = mParams.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToArray();
              }

              if (mLocalInfos.Length > 0)
              {
                mLocals = new DebugLocalInfo[mLocalInfos.Length];
                for (int i = 0; i < mLocalInfos.Length; i++)
                {
                  mLocals[i] = new DebugLocalInfo
                  {
                    Name = mLocalInfos[i].NAME,
                    Index = i,
                    IsLocal = true
                  };
                }
                mLocals = mLocals.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToArray();
              }
            }
          }
          else
          {
            
            AD7Util.MessageBox("No Symbol found for address 0x" + xAddress.ToString("X8").ToUpper());
          }
          xProcess.DebugMsg(String.Format("StackFrame: Returning: {0}#{1}[{2}]", mDocName, mFunctionName, mLineNum));
        }
      }
      if (!mHasSource)
      {
        xProcess.DebugMsg("StackFrame: No Source available");
      }

      // If source information is available, create the collections of locals and parameters and populate them with
      // values from the debuggee.
      //if (m_hasSource) {
      //if (mArgumentInfos.Length > 0) {
      //m_parameters = new VariableInformation[m_numParameters];
      //m_engine.DebuggedProcess.GetFunctionArgumentsByIP(m_threadContext.eip, m_threadContext.ebp, m_parameters);
      //}

      //if (mLocalInfos.Length > 0) {
      //m_locals = new VariableInformation[m_numLocals];
      //m_engine.DebuggedProcess.GetFunctionLocalsByIP(m_threadContext.eip, m_threadContext.ebp, m_locals);
      //}
      //}
    }

    #region Non-interface methods

    // Construct a FRAMEINFO for this stack frame with the requested information.
    public void SetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, out FRAMEINFO frameInfo)
    {
      System.Diagnostics.Debug.WriteLine("In AD7StackFrame.SetFrameInfo");
      System.Diagnostics.Debug.WriteLine("\tdwFieldSpec = " + dwFieldSpec.ToString());
      frameInfo = new FRAMEINFO();

      //uint ip = m_threadContext.eip;
      //DebuggedModule module = null;// m_engine.DebuggedProcess.ResolveAddress(ip);

      // The debugger is asking for the formatted name of the function which is displayed in the callstack window.
      // There are several optional parts to this name including the module, argument types and values, and line numbers.
      // The optional information is requested by setting flags in the dwFieldSpec parameter.
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME))
      {
        // If there is source information, construct a string that contains the module name, function name, and optionally argument names and values.
        if (mHasSource)
        {
          frameInfo.m_bstrFuncName = "";

          if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE))
          {
            //                        m_
            //frameInfo.m_bstrFuncName = System.IO.Path.GetFileName(module.Name) + "!";
            frameInfo.m_bstrFuncName = "module!";
          }

          frameInfo.m_bstrFuncName += mFunctionName;

          if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS) && mArgumentInfos.Length > 0)
          {
            frameInfo.m_bstrFuncName += "(";
            for (int i = 0; i < mParams.Length; i++)
            {
              if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_TYPES) != 0)
              {
                //frameInfo.m_bstrFuncName += m_parameters[i]. + " ";
                frameInfo.m_bstrFuncName += "ParamType ";
              }

              if ((dwFieldSpec & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_NAMES) != 0)
              {
                frameInfo.m_bstrFuncName += mParams[i].Name;
              }

              //    if ((dwFieldSpec & (uint)enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS_VALUES) != 0)  {
              //        frameInfo.m_bstrFuncName += "=" + m_parameters[i].m_value;
              //    }

              if (i < mParams.Length - 1)
              {
                frameInfo.m_bstrFuncName += ", ";
              }
            }
            frameInfo.m_bstrFuncName += ")";
          }

          if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES))
          {
            frameInfo.m_bstrFuncName += " Line:" + mLineNum.ToString();
          }
        }
        else
        {
          // No source information, so only return the module name and the instruction pointer.
          if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE))
          {
            //frameInfo.m_bstrFuncName = EngineUtils.GetAddressDescription(module, ip);
          }
          else
          {
            //frameInfo.m_bstrFuncName = EngineUtils.GetAddressDescription(null, ip);
          }
        }
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
      }

      // The debugger is requesting the name of the module for this stack frame.
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_MODULE))
      {
        frameInfo.m_bstrModule = "module";
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
      }

      // The debugger is requesting the range of memory addresses for this frame.
      // For the sample engine, this is the contents of the frame pointer.
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_STACKRANGE))
      {
        //frameInfo.m_addrMin = m_threadContext.ebp;
        //frameInfo.m_addrMax = m_threadContext.ebp;
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STACKRANGE;
      }

      // The debugger is requesting the IDebugStackFrame2 value for this frame info.
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FRAME))
      {
        frameInfo.m_pFrame = this;
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
      }

      // Does this stack frame of symbols loaded?
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO))
      {
        frameInfo.m_fHasDebugInfo = mHasSource ? 1 : 0;
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
      }

      // Is this frame stale?
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_STALECODE))
      {
        frameInfo.m_fStaleCode = 0;
        frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
      }

      // The debugger would like a pointer to the IDebugModule2 that contains this stack frame.
      if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP))
      {
        if (mEngine.mModule != null)
        {
          frameInfo.m_pModule = mEngine.mModule;
          frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
        }
      }
    }

    // Construct an instance of IEnumDebugPropertyInfo2 for the combined locals and parameters.
    private void CreateLocalsPlusArgsProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
    {
      elementsReturned = 0;

      int localsLength = 0;

      if (mLocals != null)
      {
        localsLength = mLocals.Length;
        elementsReturned += (uint)localsLength;
      }

      if (mParams != null)
      {
        elementsReturned += (uint)mParams.Length;
      }
      var propInfo = new DEBUG_PROPERTY_INFO[elementsReturned];

      if (mLocals != null)
      {
        for (int i = 0; i < mLocals.Length; i++)
        {
          var property = new AD7Property(mLocals[i], mProcess, this);
          propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
        }
      }

      if (mParams != null)
      {
        for (int i = 0; i < mParams.Length; i++)
        {
          var property = new AD7Property(mParams[i], mProcess, this);
          propInfo[localsLength + i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
        }
      }

      propInfo = propInfo.OrderBy(i => i.bstrName).ToArray();
      enumObject = new AD7PropertyInfoEnum(propInfo);
    }

    // Construct an instance of IEnumDebugPropertyInfo2 for the locals collection only.
    private void CreateLocalProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
    {
      elementsReturned = (uint)mLocals.Length;
      var propInfo = new DEBUG_PROPERTY_INFO[mLocals.Length];

      for (int i = 0; i < propInfo.Length; i++)
      {
        AD7Property property = new AD7Property(mLocals[i], mProcess, this);
        propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
      }

      propInfo = propInfo.OrderBy(i => i.bstrName).ToArray();
      enumObject = new AD7PropertyInfoEnum(propInfo);
    }

    // Construct an instance of IEnumDebugPropertyInfo2 for the parameters collection only.
    private void CreateParameterProperties(out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
    {
      elementsReturned = (uint)mParams.Length;
      var propInfo = new DEBUG_PROPERTY_INFO[mParams.Length];

      for (int i = 0; i < propInfo.Length; i++)
      {
        var property = new AD7Property(mParams[i], mProcess, this);
        propInfo[i] = property.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_STANDARD);
      }

      propInfo = propInfo.OrderBy(i => i.bstrName).ToArray();
      enumObject = new AD7PropertyInfoEnum(propInfo);
    }

    #endregion

    #region IDebugStackFrame2 Members

    // Creates an enumerator for properties associated with the stack frame, such as local variables.
    // The sample engine only supports returning locals and parameters. Other possible values include
    // class fields (this pointer), registers, exceptions...
    int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint elementsReturned, out IEnumDebugPropertyInfo2 enumObject)
    {
      int hr;

      elementsReturned = 0;
      enumObject = null;

      try
      {
        if (guidFilter == AD7Guids.guidFilterLocalsPlusArgs ||
                guidFilter == AD7Guids.guidFilterAllLocalsPlusArgs)
        {
          CreateLocalsPlusArgsProperties(out elementsReturned, out enumObject);
          hr = VSConstants.S_OK;
        }
        else if (guidFilter == AD7Guids.guidFilterLocals)
        {
          CreateLocalProperties(out elementsReturned, out enumObject);
          hr = VSConstants.S_OK;
        }
        else if (guidFilter == AD7Guids.guidFilterArgs)
        {
          CreateParameterProperties(out elementsReturned, out enumObject);
          hr = VSConstants.S_OK;
        }
        else
        {
          hr = VSConstants.E_NOTIMPL;
        }
      }
      //catch (ComponentException e)
      //{
      //    return e.HResult;
      //}
      catch (Exception e)
      {
        return EngineUtils.UnexpectedException(e);
      }

      return hr;
    }

    // Gets the code context for this stack frame. The code context represents the current instruction pointer in this stack frame.
    int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 memoryAddress)
    {
      memoryAddress = null;

      try
      {
        //memoryAddress = new AD7MemoryAddress(m_engine, m_threadContext.eip);
        return VSConstants.E_NOTIMPL;
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

    // Gets a description of the properties of a stack frame.
    // Calling the IDebugProperty2::EnumChildren method with appropriate filters can retrieve the local variables, method parameters, registers, and "this"
    // pointer associated with the stack frame. The debugger calls EnumProperties to obtain these values in the sample.
    int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 property)
    {
      throw new NotImplementedException();
    }

    // Gets the document context for this stack frame. The debugger will call this when the current stack frame is changed
    // and will use it to open the correct source document for this stack frame.
    int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 docContext)
    {
      docContext = null;
      try
      {
        if (mHasSource)
        {
          // Assume all lines begin and end at the beginning of the line.
          TEXT_POSITION begTp = new TEXT_POSITION();
          begTp.dwColumn = 0;
          begTp.dwLine = mLineNum - 1;
          TEXT_POSITION endTp = new TEXT_POSITION();
          endTp.dwColumn = 0;
          endTp.dwLine = mLineNum - 1;

          docContext = new AD7DocumentContext(mDocName, begTp, endTp, null);
          return VSConstants.S_OK;
        }
      }
      //catch (ComponentException e)
      //{
      //    return e.HResult;
      //}
      catch (Exception e)
      {
        return EngineUtils.UnexpectedException(e);
      }

      return VSConstants.S_FALSE;
    }

    // Gets an evaluation context for expression evaluation within the current context of a stack frame and thread.
    // Generally, an expression evaluation context can be thought of as a scope for performing expression evaluation.
    // Call the IDebugExpressionContext2::ParseText method to parse an expression and then call the resulting IDebugExpression2::EvaluateSync
    // or IDebugExpression2::EvaluateAsync methods to evaluate the parsed expression.
    int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
    {
      ppExprCxt = this;
      return VSConstants.S_OK;
    }

    // Gets a description of the stack frame.
    int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
    {
      try
      {
        SetFrameInfo(dwFieldSpec, out pFrameInfo[0]);

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

    // Gets the language associated with this stack frame.
    // In this sample, all the supported stack frames are C++
    int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
    {
      pbstrLanguage = "CSharp";
      pguidLanguage = AD7Guids.guidLanguageCSharp;
      return VSConstants.S_OK;
    }

    // Gets the name of the stack frame.
    // The name of a stack frame is typically the name of the method being executed.
    int IDebugStackFrame2.GetName(out string name)
    {
      name = null;

      try
      {
        name = mFunctionName;
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

    // Gets a machine-dependent representation of the range of physical addresses associated with a stack frame.
    int IDebugStackFrame2.GetPhysicalStackRange(out ulong addrMin, out ulong addrMax)
    {
      addrMin = 0;// m_threadContext.ebp;
      addrMax = 0;// m_threadContext.ebp;

      return VSConstants.S_OK;
    }

    // Gets the thread associated with a stack frame.
    int IDebugStackFrame2.GetThread(out IDebugThread2 thread)
    {
      thread = mThread;
      return VSConstants.S_OK;
    }

    #endregion

    // Retrieves the name of the evaluation context.
    // The name is the description of this evaluation context. It is typically something that can be parsed by an expression evaluator
    // that refers to this exact evaluation context. For example, in C++ the name is as follows:
    // "{ function-name, source-file-name, module-file-name }"
    int IDebugExpressionContext2.GetName(out string pbstrName)
    {
      throw new NotImplementedException();
    }

    // Parses a text-based expression for evaluation.
    // The engine sample only supports locals and parameters so the only task here is to check the names in those collections.
    int IDebugExpressionContext2.ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr,
                                            out string pbstrError,
                                            out uint pichError)
    {
      //System.Windows.Forms.AD7Util.MessageBox("pszCode: " + pszCode);
      pbstrError = "";
      pichError = 0;
      ppExpr = null;

      try
      {
        if (mParams != null)
        {
          foreach (DebugLocalInfo currVariable in mParams)
          {
            if (String.CompareOrdinal(currVariable.Name, pszCode) == 0)
            {
              ppExpr = new AD7Expression(currVariable, mProcess, this);
              return VSConstants.S_OK;
            }
          }
        }

        if (mLocals != null)
        {
          foreach (DebugLocalInfo currVariable in mLocals)
          {
            if (String.CompareOrdinal(currVariable.Name, pszCode) == 0)
            {
              ppExpr = new AD7Expression(currVariable, mProcess, this);
              return VSConstants.S_OK;
            }
          }
        }

        pbstrError = "Invalid Expression";
        pichError = (uint)pbstrError.Length;
        return VSConstants.S_FALSE;
      }
      catch (Exception e)
      {
        return EngineUtils.UnexpectedException(e);
      }
    }

  }
}

