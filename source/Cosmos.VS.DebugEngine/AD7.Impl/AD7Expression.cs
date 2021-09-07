using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    // This class represents a succesfully parsed expression to the debugger. 
    // It is returned as a result of a successful call to IDebugExpressionContext2.ParseText
    // It allows the debugger to obtain the values of an expression in the debuggee. 
    // For the purposes of this sample, this means obtaining the values of locals and parameters from a stack frame.
    public class AD7Expression : IDebugExpression2
    {
        private DebugLocalInfo m_var;
        private AD7Process Process;
        private AD7StackFrame StackFrame;

        public AD7Expression(DebugLocalInfo pVar, AD7Process pProcess, AD7StackFrame pStackFrame)
        {
            m_var = pVar;
            Process = pProcess;
            StackFrame = pStackFrame;
        }

        // This method cancels asynchronous expression evaluation as started by a call to the IDebugExpression2::EvaluateAsync method.
        int IDebugExpression2.Abort()
        {
            throw new NotImplementedException();
        }

        // This method evaluates the expression asynchronously.
        // This method should return immediately after it has started the expression evaluation. 
        // When the expression is successfully evaluated, an IDebugExpressionEvaluationCompleteEvent2 
        // must be sent to the IDebugEventCallback2 event callback
        //
        // This is primarily used for the immediate window which this engine does not currently support.
        [SuppressMessage("AsyncUsage.CSharp.Naming", "AvoidAsyncSuffix:Avoid Async suffix", Scope = "member")]
        int IDebugExpression2.EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            throw new NotImplementedException();
        }

        // This method evaluates the expression synchronously.
        int IDebugExpression2.EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback, out IDebugProperty2 ppResult)
        {
            ppResult = new AD7Property(m_var, Process, StackFrame);
            return VSConstants.S_OK;
        }

    }
}
