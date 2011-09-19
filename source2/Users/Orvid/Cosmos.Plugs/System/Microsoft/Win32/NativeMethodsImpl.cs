namespace Cosmos.Plugs
{
    //[Cosmos.IL2CPU.Plugs.Plug(Target = typeof(Microsoft.Win32.NativeMethods), TargetFramework = Cosmos.IL2CPU.Plugs.FrameworkVersion.v4_0)]
    //public static class Microsoft_Win32_NativeMethodsImpl
    //{

    //    public static System.Boolean GetExitCodeProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Int32* exitCode)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetExitCodeProcess' has not been implemented!");
    //    }

    //    public static System.Boolean GetProcessTimes(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.Int64* creation, System.Int64* exit, System.Int64* kernel, System.Int64* user)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetProcessTimes' has not been implemented!");
    //    }

    //    public static System.Boolean CreateProcess(System.String lpApplicationName, System.Text.StringBuilder lpCommandLine, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpProcessAttributes, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpThreadAttributes, System.Boolean bInheritHandles, System.Int32 dwCreationFlags, System.IntPtr lpEnvironment, System.String lpCurrentDirectory, Microsoft.Win32.NativeMethods+STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods+PROCESS_INFORMATION lpProcessInformation)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateProcess' has not been implemented!");
    //    }

    //    public static System.Int32 GetCurrentProcessId()
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetCurrentProcessId' has not been implemented!");
    //    }

    //    public static System.IntPtr GetCurrentProcess()
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetCurrentProcess' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeFileMappingHandle CreateFileMapping(System.IntPtr hFile, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpFileMappingAttributes, System.Int32 flProtect, System.Int32 dwMaximumSizeHigh, System.Int32 dwMaximumSizeLow, System.String lpName)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateFileMapping' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeProcessHandle OpenProcess(System.Int32 access, System.Boolean inherit, System.Int32 processId)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.OpenProcess' has not been implemented!");
    //    }

    //    public static System.Boolean EnumProcessModules(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.IntPtr modules, System.Int32 size, System.Int32* needed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.EnumProcessModules' has not been implemented!");
    //    }

    //    public static System.Boolean GetModuleInformation(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Runtime.InteropServices.HandleRef moduleHandle, Microsoft.Win32.NativeMethods+NtModuleInfo ntModuleInfo, System.Int32 size)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetModuleInformation' has not been implemented!");
    //    }

    //    public static System.Int32 GetModuleBaseName(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Runtime.InteropServices.HandleRef moduleHandle, System.Text.StringBuilder baseName, System.Int32 size)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetModuleBaseName' has not been implemented!");
    //    }

    //    public static System.Int32 GetModuleFileNameEx(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Runtime.InteropServices.HandleRef moduleHandle, System.Text.StringBuilder baseName, System.Int32 size)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetModuleFileNameEx' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(System.String lpFileName, System.Int32 dwDesiredAccess, System.Int32 dwShareMode, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpSecurityAttributes, System.Int32 dwCreationDisposition, System.Int32 dwFlagsAndAttributes, Microsoft.Win32.SafeHandles.SafeFileHandle hTemplateFile)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateFile' has not been implemented!");
    //    }

    //    public static System.Boolean DuplicateHandle(System.Runtime.InteropServices.HandleRef hSourceProcessHandle, System.Runtime.InteropServices.SafeHandle hSourceHandle, System.Runtime.InteropServices.HandleRef hTargetProcess, Microsoft.Win32.SafeHandles.SafeWaitHandle* targetHandle, System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.Int32 dwOptions)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.DuplicateHandle' has not been implemented!");
    //    }

    //    public static System.Boolean OpenProcessToken(System.Runtime.InteropServices.HandleRef ProcessHandle, System.Int32 DesiredAccess, System.IntPtr* TokenHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.OpenProcessToken' has not been implemented!");
    //    }

    //    public static System.Boolean LookupPrivilegeValue(System.String lpSystemName, System.String lpName, Microsoft.Win32.NativeMethods+LUID* lpLuid)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.LookupPrivilegeValue' has not been implemented!");
    //    }

    //    public static System.Boolean AdjustTokenPrivileges(System.Runtime.InteropServices.HandleRef TokenHandle, System.Boolean DisableAllPrivileges, Microsoft.Win32.NativeMethods+TokenPrivileges NewState, System.Int32 BufferLength, System.IntPtr PreviousState, System.IntPtr ReturnLength)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.AdjustTokenPrivileges' has not been implemented!");
    //    }

    //    public static System.IntPtr VirtualQuery(Microsoft.Win32.SafeHandles.SafeFileMapViewHandle address, Microsoft.Win32.NativeMethods+MEMORY_BASIC_INFORMATION* buffer, System.IntPtr sizeOfBuffer)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.VirtualQuery' has not been implemented!");
    //    }

    //    public static System.Boolean GetThreadTimes(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Int64* creation, System.Int64* exit, System.Int64* kernel, System.Int64* user)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetThreadTimes' has not been implemented!");
    //    }

    //    public static System.IntPtr GetStdHandle(System.Int32 whichHandle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetStdHandle' has not been implemented!");
    //    }

    //    public static System.Boolean CreatePipe(Microsoft.Win32.SafeHandles.SafeFileHandle* hReadPipe, Microsoft.Win32.SafeHandles.SafeFileHandle* hWritePipe, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpPipeAttributes, System.Int32 nSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreatePipe' has not been implemented!");
    //    }

    //    public static System.Boolean TerminateProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Int32 exitCode)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.TerminateProcess' has not been implemented!");
    //    }

    //    public static System.Boolean CreateProcessAsUser(System.Runtime.InteropServices.SafeHandle hToken, System.String lpApplicationName, System.String lpCommandLine, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpProcessAttributes, Microsoft.Win32.NativeMethods+SECURITY_ATTRIBUTES lpThreadAttributes, System.Boolean bInheritHandles, System.Int32 dwCreationFlags, System.Runtime.InteropServices.HandleRef lpEnvironment, System.String lpCurrentDirectory, Microsoft.Win32.NativeMethods+STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods+PROCESS_INFORMATION lpProcessInformation)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateProcessAsUser' has not been implemented!");
    //    }

    //    public static System.Boolean CreateProcessWithLogonW(System.String userName, System.String domain, System.IntPtr password, Microsoft.Win32.NativeMethods+LogonFlags logonFlags, System.String appName, System.Text.StringBuilder cmdLine, System.Int32 creationFlags, System.IntPtr environmentBlock, System.String lpCurrentDirectory, Microsoft.Win32.NativeMethods+STARTUPINFO lpStartupInfo, Microsoft.Win32.SafeNativeMethods+PROCESS_INFORMATION lpProcessInformation)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateProcessWithLogonW' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeFileMappingHandle OpenFileMapping(System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.String lpName)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.OpenFileMapping' has not been implemented!");
    //    }

    //    public static System.Int32 WaitForInputIdle(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.Int32 milliseconds)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.WaitForInputIdle' has not been implemented!");
    //    }

    //    public static System.Boolean EnumProcesses(System.Int32[] processIds, System.Int32 size, System.Int32* needed)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.EnumProcesses' has not been implemented!");
    //    }

    //    public static System.Int32 GetModuleFileNameEx(System.Runtime.InteropServices.HandleRef processHandle, System.Runtime.InteropServices.HandleRef moduleHandle, System.Text.StringBuilder baseName, System.Int32 size)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetModuleFileNameEx' has not been implemented!");
    //    }

    //    public static System.Boolean SetProcessWorkingSetSize(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.IntPtr min, System.IntPtr max)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetProcessWorkingSetSize' has not been implemented!");
    //    }

    //    public static System.Boolean GetProcessWorkingSetSize(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.IntPtr* min, System.IntPtr* max)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetProcessWorkingSetSize' has not been implemented!");
    //    }

    //    public static System.Boolean SetProcessAffinityMask(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.IntPtr mask)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetProcessAffinityMask' has not been implemented!");
    //    }

    //    public static System.Boolean GetProcessAffinityMask(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.IntPtr* processMask, System.IntPtr* systemMask)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetProcessAffinityMask' has not been implemented!");
    //    }

    //    public static System.Boolean GetThreadPriorityBoost(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Boolean* disabled)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetThreadPriorityBoost' has not been implemented!");
    //    }

    //    public static System.Boolean SetThreadPriorityBoost(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Boolean disabled)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetThreadPriorityBoost' has not been implemented!");
    //    }

    //    public static System.Boolean GetProcessPriorityBoost(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.Boolean* disabled)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetProcessPriorityBoost' has not been implemented!");
    //    }

    //    public static System.Boolean SetProcessPriorityBoost(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.Boolean disabled)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetProcessPriorityBoost' has not been implemented!");
    //    }

    //    public static Microsoft.Win32.SafeHandles.SafeThreadHandle OpenThread(System.Int32 access, System.Boolean inherit, System.Int32 threadId)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.OpenThread' has not been implemented!");
    //    }

    //    public static System.Boolean SetThreadPriority(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Int32 priority)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetThreadPriority' has not been implemented!");
    //    }

    //    public static System.Int32 GetThreadPriority(Microsoft.Win32.SafeHandles.SafeThreadHandle handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetThreadPriority' has not been implemented!");
    //    }

    //    public static System.IntPtr SetThreadAffinityMask(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Runtime.InteropServices.HandleRef mask)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetThreadAffinityMask' has not been implemented!");
    //    }

    //    public static System.Int32 SetThreadIdealProcessor(Microsoft.Win32.SafeHandles.SafeThreadHandle handle, System.Int32 processor)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetThreadIdealProcessor' has not been implemented!");
    //    }

    //    public static System.IntPtr CreateToolhelp32Snapshot(System.Int32 flags, System.Int32 processId)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.CreateToolhelp32Snapshot' has not been implemented!");
    //    }

    //    public static System.Boolean Process32First(System.Runtime.InteropServices.HandleRef handle, System.IntPtr entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Process32First' has not been implemented!");
    //    }

    //    public static System.Boolean Process32Next(System.Runtime.InteropServices.HandleRef handle, System.IntPtr entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Process32Next' has not been implemented!");
    //    }

    //    public static System.Boolean Thread32First(System.Runtime.InteropServices.HandleRef handle, Microsoft.Win32.NativeMethods+WinThreadEntry entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Thread32First' has not been implemented!");
    //    }

    //    public static System.Boolean Thread32Next(System.Runtime.InteropServices.HandleRef handle, Microsoft.Win32.NativeMethods+WinThreadEntry entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Thread32Next' has not been implemented!");
    //    }

    //    public static System.Boolean Module32First(System.Runtime.InteropServices.HandleRef handle, System.IntPtr entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Module32First' has not been implemented!");
    //    }

    //    public static System.Boolean Module32Next(System.Runtime.InteropServices.HandleRef handle, System.IntPtr entry)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.Module32Next' has not been implemented!");
    //    }

    //    public static System.Int32 GetPriorityClass(Microsoft.Win32.SafeHandles.SafeProcessHandle handle)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetPriorityClass' has not been implemented!");
    //    }

    //    public static System.Boolean SetPriorityClass(Microsoft.Win32.SafeHandles.SafeProcessHandle handle, System.Int32 priorityClass)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SetPriorityClass' has not been implemented!");
    //    }

    //    public static System.Boolean EnumWindows(Microsoft.Win32.NativeMethods+EnumThreadWindowsCallback callback, System.IntPtr extraData)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.EnumWindows' has not been implemented!");
    //    }

    //    public static System.Int32 GetWindowThreadProcessId(System.Runtime.InteropServices.HandleRef handle, System.Int32* processId)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetWindowThreadProcessId' has not been implemented!");
    //    }

    //    public static System.Boolean ShellExecuteEx(Microsoft.Win32.NativeMethods+ShellExecuteInfo info)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.ShellExecuteEx' has not been implemented!");
    //    }

    //    public static System.Int32 NtQueryInformationProcess(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle, System.Int32 query, Microsoft.Win32.NativeMethods+NtProcessBasicInfo info, System.Int32 size, System.Int32[] returnedSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.NtQueryInformationProcess' has not been implemented!");
    //    }

    //    public static System.Int32 NtQuerySystemInformation(System.Int32 query, System.IntPtr dataPtr, System.Int32 size, System.Int32* returnedSize)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.NtQuerySystemInformation' has not been implemented!");
    //    }

    //    public static System.Boolean DuplicateHandle(System.Runtime.InteropServices.HandleRef hSourceProcessHandle, System.Runtime.InteropServices.SafeHandle hSourceHandle, System.Runtime.InteropServices.HandleRef hTargetProcess, Microsoft.Win32.SafeHandles.SafeFileHandle* targetHandle, System.Int32 dwDesiredAccess, System.Boolean bInheritHandle, System.Int32 dwOptions)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.DuplicateHandle' has not been implemented!");
    //    }

    //    public static System.Int32 GetWindowText(System.Runtime.InteropServices.HandleRef hWnd, System.Text.StringBuilder lpString, System.Int32 nMaxCount)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetWindowText' has not been implemented!");
    //    }

    //    public static System.Int32 GetWindowTextLength(System.Runtime.InteropServices.HandleRef hWnd)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetWindowTextLength' has not been implemented!");
    //    }

    //    public static System.Boolean IsWindowVisible(System.Runtime.InteropServices.HandleRef hWnd)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.IsWindowVisible' has not been implemented!");
    //    }

    //    public static System.IntPtr SendMessageTimeout(System.Runtime.InteropServices.HandleRef hWnd, System.Int32 msg, System.IntPtr wParam, System.IntPtr lParam, System.Int32 flags, System.Int32 timeout, System.IntPtr* pdwResult)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.SendMessageTimeout' has not been implemented!");
    //    }

    //    public static System.Int32 GetWindowLong(System.Runtime.InteropServices.HandleRef hWnd, System.Int32 nIndex)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetWindowLong' has not been implemented!");
    //    }

    //    public static System.Int32 PostMessage(System.Runtime.InteropServices.HandleRef hwnd, System.Int32 msg, System.IntPtr wparam, System.IntPtr lparam)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.PostMessage' has not been implemented!");
    //    }

    //    public static System.IntPtr GetWindow(System.Runtime.InteropServices.HandleRef hWnd, System.Int32 uCmd)
    //    {
    //        throw new System.NotImplementedException("Method 'Microsoft.Win32.NativeMethods.GetWindow' has not been implemented!");
    //    }
    //}
}
