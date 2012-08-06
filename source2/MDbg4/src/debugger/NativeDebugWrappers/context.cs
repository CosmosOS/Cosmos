//---------------------------------------------------------------------
//  This file is part of the CLR Managed Debugger (mdbg) Sample.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Part of managed wrappers for native debugging APIs.
// Context.cs: defines INativeContext interfaces.
//---------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.Serialization;

using Microsoft.Samples.Debugging.Native;
using Microsoft.Samples.Debugging.Native.Private;



namespace Microsoft.Samples.Debugging.Native
{
    /// <summary>
    /// Serves as the global method for creating a platform-specific context
    /// </summary>
    public static class ContextAllocator
    {
        /// <summary>
        /// Generates a new context object for the platform in which Mdbg is running.
        /// </summary>
        /// <returns>Newly allocated platform specific context</returns>
        /// <exception cref="InvalidOperationException">Throws if running on an unsupported platform</exception>
        static public INativeContext GenerateContext()
        {
            int mdbgPid = Process.GetCurrentProcess().Id;
            ProcessorArchitecture arch = GetArchitectureFromPid(mdbgPid);
            INativeContext context = GenerateContext(arch);
            context.Flags = context.GetPSFlags(AgnosticContextFlags.ContextControl | AgnosticContextFlags.ContextInteger);
            return context;
        }

        /// <summary>
        /// Determines the platform architecture the OS is running a process in. Wow mode processes
        /// will report INTEL (32bit) otherwise all processes run in the native system architecture
        /// </summary>
        static public ProcessorArchitecture GetArchitectureFromPid(int pid)
        {
            NativeMethods.SYSTEM_INFO info;
            NativeMethods.GetSystemInfo(out info);
            bool isWow = false;

            if (info.wProcessorArchitecture == ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL)
            {
                // We know that we need an x86 context
                return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL;
            }
            else
            {
                // We cannot make this call if we know we're executing on a 32-bit OS, so we wait until
                // we know that we're running under a 64-bit OS where this method is defined.
                // 0x0400 = PROCESS_QUERY_INFORMATION
                using (SafeWin32Handle hProcess = NativeMethods.OpenProcess(0x0400, false, pid))
                {
                    NativeMethods.IsWow64Process(hProcess, ref isWow);
                }

                if (isWow)
                {
                    // Even though we're running on a 64-bit OS, we are currently in WOW mode.
                    return ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL;
                }
                // From this point on, we know that we're not running a 32-bit OS.  We can fully trust the
                // value in wProcessorArchitecture
                else
                {
                    return info.wProcessorArchitecture;
                }
            }
        }

        /// <summary>
        /// Generates a new context object for the current platform.  This does NOT account for Wow mode processes.
        /// This method should be primarily used for dump debugging.
        /// </summary>
        /// <param name="architecture">The architecture for which to create the context</param>
        /// <returns>Newly allocated platform specific context.</returns>
        /// <exception cref="InvalidOperationException">Throws if running on an unsupported platform</exception>
        static public INativeContext GenerateContext(ProcessorArchitecture architecture)
        {
            if (architecture == ProcessorArchitecture.PROCESSOR_ARCHITECTURE_INTEL)
            {
                // We know that we need an x86 context
                return new X86Context();
            }
            else if (architecture == ProcessorArchitecture.PROCESSOR_ARCHITECTURE_AMD64)
            {
                // We know that we need an amd64 context
                return new AMD64Context();
            }
            else if (architecture == ProcessorArchitecture.PROCESSOR_ARCHITECTURE_IA64)
            {
                // We know that we need an ia64 context
                return new IA64Context();
            }
            else
            {
                throw new ApplicationException("Error: This architecture is not supported");
            }
        }
    }

    /// <summary>
    /// Exposes raw contents of the Context in IContext. This locks the buffer. Dispose this object to unlock the buffer
    /// </summary>
    /// <remarks>The implementation behind the interface has a variety of ways to ensure the memory is safe to write to.
    /// The buffer may be in the native heap; or it may be to a pinned object in the managed heap
    /// This is primarily intended for writing to the context (by passing the buffer out to a pinvoke),
    /// but can also be a way to read the raw bytes.</remarks>
    [CLSCompliant(true)]
    public interface IContextDirectAccessor : IDisposable
    {
        /// <summary>
        /// The size of the buffer. This should be the same as INativeContext.Size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// A pointer to the raw buffer. The memory is pinned until this object is disposed. Check the context Flags 
        /// to know which raw bytes are valid to be read. 
        /// </summary>
        IntPtr RawBuffer { get; }
    }

    public class ContextAccessor : IContextDirectAccessor, IDisposable
    {
        private IntPtr m_rawBuffer;
        private int m_size;

        public ContextAccessor(IntPtr buffer, int size)
        {
            this.m_size = size;
            this.m_rawBuffer = buffer;
        }

        /// <summary>
        /// The size of the buffer. This should be the same as Context.Size.
        /// </summary>
        public int Size
        {
            get
            {
                return this.m_size;
            }
        }

        /// <summary>
        /// A pointer to the raw buffer. The memory is pinned until this object is disposed. Check the context Flags 
        /// to know which raw bytes are valid to be read. 
        /// </summary>
        public IntPtr RawBuffer
        {
            get
            {
                return this.m_rawBuffer;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void Dispose(bool supressPendingFinalizer)
        {
            this.m_rawBuffer = IntPtr.Zero;
            this.m_size = 0;

            if (supressPendingFinalizer)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface to a context. This provides platform agnostic wrapper to a platform specific OS Context.
    /// </summary>
    public interface INativeContext :
        IEquatable<INativeContext>,
        IDisposable
    {
        #region Writing
        /// <summary>
        /// Used to lock the buffer and get a raw pointer to it. 
        /// This is the only way to change the entire context at once. 
        /// This is useful for pinvoking to native functions.
        /// </summary>
        /// <returns>context writer object</returns>
        /// <remarks>
        /// Expected usage would be (in C# syntax):
        /// <example>
        ///    IContext c = NativeContextAllocator.Alloc();
        ///    using(IContextWriter w = c.OpenForDirectAccess) { // context buffer is now locked
        ///       SomeNativeFunctionToGetThreadContext(w.RawBuffer, w.Size);
        ///    } // w is disposed, this unlocks the context buffer.
        /// </example>
        /// </remarks>
        IContextDirectAccessor OpenForDirectAccess();
        #endregion

        #region Geometry and writing
        /// <summary>
        /// Get Size in bytes. Size could change depending on the flags.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Get the flags associated with the context. 
        /// </summary>
        /// <remarks>Flags are platform specific and generally indicate which parts of the context are valid.
        /// Flags will affect which registers are available (EnumerateRegisters), potentially the Size of the context,
        /// and how contexts are compared.
        /// Expanding the active flags means newly included registers have an uninitialized value.
        /// A context could be completely constructed late-bound by setting the Flags and then calling
        /// SetRegisterByName on each regsister
        /// </remarks>
        ContextFlags Flags { get; set; }


        /// <summary>
        /// This will return the context specific flags for the given AgnosticContextFlags
        /// <param name="flags">the value (found in the enum) of the platform specific flags desired</param>
        /// </summary>
        ContextFlags GetPSFlags(AgnosticContextFlags flags);

        /// <summary>
        /// This will clear the context buffer.
        /// </summary>
        void ClearContext();

        #endregion

        #region Standard operations
        /// <summary>
        /// Get or Set the instruction pointer
        /// </summary>
        IntPtr InstructionPointer { get; set; }

        /// <summary>
        /// Get the stack pointer
        /// </summary>
        IntPtr StackPointer { get; }

        /// <summary>
        /// Enable or disable the single-step flag in the context. 
        /// </summary>
        /// <param name="enable">true to enable single-stepping, false to disable it</param>
        /// <exception cref="System.InvalidOperationException">Throws if the architecture doesn't support single-stepping.</exception>
        void SetSingleStepFlag(bool enable);

        /// <summary>
        /// Is the single step flag enabled?
        /// </summary>
        bool IsSingleStepFlagEnabled { get; }

        /// <summary>
        /// Create a new deep copy of this context. 
        /// The copies are independent and can be modified without interfering with each other.
        /// </summary>
        /// <returns>copy of this context</returns>
        /// <remarks>Contexts can be large, so copying excessively would be expensive.</remarks>
        /// <example> 
        /// INativeContext c1 = ...
        /// INativeContext c2 = c1.Clone();
        ///   
        /// Assert(c1 != c2); // true, Clone gives different instances
        /// Assert(c1.Equals(c2)); // true
        /// Assert(c2.Equals(c1)); // true
        /// </example>
        INativeContext Clone();

        // <summary>
        // Implement IEquatable<T> to do value comparison of two contexts. 
        // </summary>
        // <param name="other">non-null context to compare too</param>
        // <returns>true if equal, else false</returns>
        // <remarks>Comparison can't just do a bitwise comparison of the buffer. It needs to be aware of the <see cref="Flags"/> 
        // property for each context, because if a portion of the context is missing, it could be random garbage. 
        // Comparison does not modify either context object.</remarks>

        // bool IEquatable<T>.Equals(object other)  // inheritted from IEquatable<T>

        #endregion Standard operations



        #region Self Describing
        /// <summary>
        /// Get a simple string description of the CPU the context is for. A implementation may also provide a ToString()
        /// override to give more detail (eg, which flags are active)
        /// </summary>
        Platform Platform { get; }

        /// <summary>
        /// Get the ImageFileMachine code.  This is used for getting an interop callstack for a machine.
        /// Used by TraverseStack
        /// </summary>
        int ImageFileMachine { get; }

        /// <summary>
        /// Enumerate registers names (and their types) for late-bound access. Available registers depend on the flags.
        /// </summary>
        /// <returns>an enumeration of (name,type) pairs</returns>
        /// <remarks>An implementation does not need to include all registers on the context.
        /// The returned strings can be used with other by-name functions like <see cref="FindRegisterByName"/>
        /// and <see cref="SetRegisterByName"/>.</remarks>
        System.Collections.Generic.IEnumerable<String> EnumerateRegisters();

        /// <summary>
        /// Get a register by name
        /// </summary>
        /// <param name="name">Name of the registers. Lookup is case insensitive</param>
        /// <returns>value of register. Registers can be arbitrary types (uint32, double, long, etc), so this
        /// returns an object. Throws if name is not currently valid</returns>
        object FindRegisterByName(string name);

        /// <summary>
        /// Sets a register by name.
        /// </summary>
        /// <param name="name">Case-insensitive name of register to set. </param>
        /// <param name="value">value of register to set. Type of value must be convertable to type of the register</param>
        /// <exception cref="System.InvalidOperationException">Throws if no matching name or if register is not valid for the given Flags.</exception>
        void SetRegisterByName(string name, object value);

        #endregion Self Describing


    } // IContext


    /// <summary>
    /// Describes the ProcessorArchitecture in a SYSTEM_INFO field.
    /// This can also be reported by a dump file.
    /// </summary>
    public enum ProcessorArchitecture : ushort
    {
        PROCESSOR_ARCHITECTURE_INTEL = 0,
        PROCESSOR_ARCHITECTURE_MIPS = 1,
        PROCESSOR_ARCHITECTURE_ALPHA = 2,
        PROCESSOR_ARCHITECTURE_PPC = 3,
        PROCESSOR_ARCHITECTURE_SHX = 4,
        PROCESSOR_ARCHITECTURE_ARM = 5,
        PROCESSOR_ARCHITECTURE_IA64 = 6,
        PROCESSOR_ARCHITECTURE_ALPHA64 = 7,
        PROCESSOR_ARCHITECTURE_MSIL = 8,
        PROCESSOR_ARCHITECTURE_AMD64 = 9,
        PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10,
    }
}