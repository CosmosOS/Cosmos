using System;

using PostSharp.Laos;

namespace Cosmos.Kernel.Debug {
	[Serializable]
	public class TraceAttribute: OnMethodBoundaryAspect {
		private string mMethodName;
		public override void CompileTimeInitialize(System.Reflection.MethodBase method) {
			base.CompileTimeInitialize(method);
			mMethodName = method.DeclaringType.FullName + "." + method.ToString();

		}
		public override void OnEntry(MethodExecutionEventArgs eventArgs) {
			base.OnEntry(eventArgs);
			System.Diagnostics.Debug.WriteLine("Enter method '" + mMethodName);
		}
	}
}