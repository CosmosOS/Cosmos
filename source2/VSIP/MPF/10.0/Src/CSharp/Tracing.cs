/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Diagnostics;

namespace Microsoft.VisualStudio.Project
{
	internal class CCITracing
	{
		private CCITracing() { }

		[ConditionalAttribute("Enable_CCIDiagnostics")]
		static void InternalTraceCall(int levels)
		{
			System.Diagnostics.StackFrame stack;
			stack = new System.Diagnostics.StackFrame(levels);
			System.Reflection.MethodBase method = stack.GetMethod();
			if(method != null)
			{
				string name = method.Name + " \tin class " + method.DeclaringType.Name;
				System.Diagnostics.Trace.WriteLine("Call Trace: \t" + name);
			}
		}

		[ConditionalAttribute("CCI_TRACING")]
		static public void TraceCall()
		{
			// skip this one as well
			CCITracing.InternalTraceCall(2);
		}

		[ConditionalAttribute("CCI_TRACING")]
		static public void TraceCall(string strParameters)
		{
			CCITracing.InternalTraceCall(2);
			System.Diagnostics.Trace.WriteLine("\tParameters: \t" + strParameters);
		}

		[ConditionalAttribute("CCI_TRACING")]
		static public void Trace(System.Exception e)
		{
			CCITracing.InternalTraceCall(2);
			System.Diagnostics.Trace.WriteLine("ExceptionInfo: \t" + e.ToString());
		}

		[ConditionalAttribute("CCI_TRACING")]
		static public void Trace(string strOutput)
		{
			System.Diagnostics.Trace.WriteLine(strOutput);
		}

		[ConditionalAttribute("CCI_TRACING")]
		static public void TraceData(string strOutput)
		{
			System.Diagnostics.Trace.WriteLine("Data Trace: \t" + strOutput);
		}

		[ConditionalAttribute("Enable_CCIFileOutput")]
		[ConditionalAttribute("CCI_TRACING")]
		static public void AddTraceLog(string strFileName)
		{
			TextWriterTraceListener tw = new TextWriterTraceListener("c:\\mytrace.log");
			System.Diagnostics.Trace.Listeners.Add(tw);
		}
	}
}
