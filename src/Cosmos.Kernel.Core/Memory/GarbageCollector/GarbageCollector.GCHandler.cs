// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// GC handle table: allocation, freeing, and weak handle cleanup for Weak, Normal, and Pinned handles.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    /// <summary>
    /// Allocates a new GC handle for the specified object.
    /// </summary>
    /// <param name="obj">Pointer to the managed object to track.</param>
    /// <param name="handleType">The type of handle to allocate.</param>
    /// <param name="extraInfo">Additional info associated with the handle.</param>
    /// <returns>An opaque handle value, or <see cref="IntPtr.Zero"/> if the store is full.</returns>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    internal static IntPtr AllocateHandler(GCObject* obj, GCHandleType handleType, nint extraInfo)
    {
        return (nint)s_gCHandleManager.AllocateHandler(obj, handleType, extraInfo);
    }

    /// <summary>
    /// Gets the primary object from a dependent handle.
    /// </summary>
    /// <param name="handle">The handle pointer.</param>
    /// <returns>Pointer to the primary object, or <c>null</c> if the handle is empty.</returns>
    internal static GCObject* HandleGetPrimary(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        var entry = (GCHandle*)(nint)handle;
        return entry->Object;
    }

    /// <summary>
    /// Gets the secondary (dependent) object from a dependent handle.
    /// </summary>
    /// <param name="handle">The handle pointer.</param>
    /// <returns>Pointer to the secondary object, or <c>null</c> if none.</returns>
    internal static GCObject* HandleGetSecondary(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        var entry = (GCHandle*)(nint)handle;
        return (GCObject*)entry->ExtraInfo;
    }

    /// <summary>
    /// Sets the primary object on a dependent handle.
    /// </summary>
    /// <param name="handle">The handle pointer.</param>
    /// <param name="obj">The new primary object pointer.</param>
    internal static void HandleSetPrimary(IntPtr handle, GCObject* obj)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var entry = (GCHandle*)handle;
        entry->Object = obj;
    }

    /// <summary>
    /// Sets the secondary (dependent) object on a dependent handle.
    /// </summary>
    /// <param name="handle">The handle pointer.</param>
    /// <param name="secondary">The new secondary object pointer.</param>
    internal static void HandleSetSecondary(IntPtr handle, GCObject* secondary)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var entry = (GCHandle*)handle;
        entry->ExtraInfo = (nint)secondary;
    }

    /// <summary>
    /// Frees a previously allocated GC handle, releasing the slot for reuse.
    /// </summary>
    /// <param name="handle">The handle to free. No-op if <see cref="IntPtr.Zero"/>.</param>
    internal static void FreeHandle(IntPtr handle)
    {
        s_gCHandleManager.FreeHandle((GCHandle*)handle);
    }
}
