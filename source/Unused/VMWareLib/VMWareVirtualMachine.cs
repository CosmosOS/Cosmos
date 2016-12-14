using System;
using System.Collections.Generic;
using System.IO;
using Interop.VixCOM;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// Virtual machine clone type.
    /// </summary>
    public enum VMWareVirtualMachineCloneType
    {
        /// <summary>
        /// A full, independent clone of the virtual machine.
        /// </summary>
        Full = Constants.VIX_CLONETYPE_FULL,
        /// <summary>
        /// A linked clone is a copy of a virtual machine that shares virtual disks with the parent virtual 
        /// machine in an ongoing manner. 
        /// </summary>
        Linked = Constants.VIX_CLONETYPE_LINKED
    }

    /// <summary>
    /// A VMWare Virtual Machine.
    /// </summary>
    public class VMWareVirtualMachine : VMWareVixHandle<IVM2>
    {
        /// <summary>
        /// Guest file info.
        /// </summary>
        public class GuestFileInfo
        {
            private int _flags = 0;
            private long _fileSize = 0;
            private Nullable<DateTime> _lastModified = null;
            private string _guestPathName;

            /// <summary>
            /// File size in bytes, zero for directories.
            /// </summary>
            public long FileSize
            {
                get { return _fileSize; }
                set { _fileSize = value; }
            }

            /// <summary>
            /// File attributes/flags.
            /// </summary>
            public int Flags
            {
                get { return _flags; }
                set { _flags = value; }
            }

            /// <summary>
            /// True if directory.
            /// </summary>
            public bool IsDirectory
            {
                get { return (_flags & Constants.VIX_FILE_ATTRIBUTES_DIRECTORY) > 0; }
            }

            /// <summary>
            /// True if symbolic link.
            /// </summary>
            public bool IsSymLink
            {
                get { return (_flags & Constants.VIX_FILE_ATTRIBUTES_SYMLINK) > 0; }
            }

            /// <summary>
            /// Last modified time.
            /// </summary>
            public Nullable<DateTime> LastModified
            {
                get { return _lastModified; }
                set { _lastModified = value; }
            }

            /// <summary>
            /// Guest file or directory name.
            /// </summary>
            public string GuestPathName
            {
                get { return _guestPathName; }
                set { _guestPathName = value; }
            }
        }

        /// <summary>
        /// An indexer for variables.
        /// </summary>
        public class VariableIndexer
        {
            private IVM2 _handle;
            private int _variableType;

            /// <summary>
            /// A variables indexer.
            /// </summary>
            /// <param name="vm">Virtual machine's variables to index.</param>
            /// <param name="variableType">Variable type, one of the following.
            /// <list type="bullet">
            ///  <item>Constants.VIX_VM_GUEST_VARIABLE</item>
            ///  <item>Constants.VIX_VM_CONFIG_RUNTIME_ONLY</item>
            ///  <item>Constants.VIX_GUEST_ENVIRONMENT_VARIABLE</item>
            /// </list>
            /// </param>
            public VariableIndexer(IVM2 vm, int variableType)
            {
                _handle = vm;
                _variableType = variableType;
            }

            /// <summary>
            /// Environment, guest and runtime variables.
            /// </summary>
            /// <param name="name">Name of the variable.</param>
            [IndexerName("Variables")]
            public string this[string name]
            {
                get
                {
                    try
                    {
                        VMWareJobCallback callback = new VMWareJobCallback();
                        using (VMWareJob job = new VMWareJob(_handle.ReadVariable(
                            _variableType, name, 0, callback),
                            callback))
                        {
                            return job.Wait<string>(
                                Constants.VIX_PROPERTY_JOB_RESULT_VM_VARIABLE_STRING,
                                VMWareInterop.Timeouts.ReadVariableTimeout);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format("Failed to get virtual machine variable: name=\"{0}\"",
                            name), ex);
                    }
                }
                set
                {
                    try
                    {
                        VMWareJobCallback callback = new VMWareJobCallback();
                        using (VMWareJob job = new VMWareJob(_handle.WriteVariable(
                            _variableType, name, value, 0, callback),
                            callback))
                        {
                            job.Wait(VMWareInterop.Timeouts.WriteVariableTimeout);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            string.Format("Failed to set virtual machine variable: name=\"{0}\" value=\"{1}\"", name, value), ex);
                    }
                }
            }
        }

        /// <summary>
        /// A process running in the guest operating system.
        /// </summary>
        public class Process
        {
            /// <summary>
            /// Process ID.
            /// </summary>
            public long Id;
            /// <summary>
            /// Process name.
            /// </summary>
            public string Name;
            /// <summary>
            /// Process owner.
            /// </summary>
            public string Owner;
            /// <summary>
            /// Process start date/time.
            /// </summary>
            public DateTime StartDateTime;
            /// <summary>
            /// Process command line.
            /// </summary>
            public string Command;
            /// <summary>
            /// True if process is being debugged.
            /// </summary>
            public bool IsBeingDebugged = false;
            /// <summary>
            /// Process exit code for finished processes.
            /// </summary>
            public int ExitCode = 0;

            private IVM2 _vm;

            /// <summary>
            /// A process running in the guest operating system on a virtual machine.
            /// </summary>
            /// <param name="vm">Virtual machine.</param>
            public Process(IVM2 vm)
            {
                _vm = vm;
            }

            /// <summary>
            /// Kill a process in the guest operating system.
            /// </summary>
            public void KillProcessInGuest()
            {
                KillProcessInGuest(VMWareInterop.Timeouts.KillProcessTimeout);
            }

            /// <summary>
            /// Kill a process in the guest operating system.
            /// </summary>
            /// <param name="timeoutInSeconds">Timeout in seconds.</param>
            public void KillProcessInGuest(int timeoutInSeconds)
            {
                try
                {
                    VMWareJobCallback callback = new VMWareJobCallback();
                    using (VMWareJob job = new VMWareJob(_vm.KillProcessInGuest(
                        Convert.ToUInt64(Id), 0, callback),
                        callback))
                    {
                        job.Wait(timeoutInSeconds);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        string.Format("Failed to kill process in guest: processId={0}", 
                        Id), ex);
                }
            }
        }

        private VariableIndexer _guestEnvironmentVariables = null;
        private VariableIndexer _runtimeConfigVariables = null;
        private VariableIndexer _guestVariables = null;
        private VMWareRootSnapshotCollection _snapshots = null;
        private VMWareSharedFolderCollection _sharedFolders = null;

        /// <summary>
        /// A VMWare Virtual Machine.
        /// </summary>
        /// <param name="vm">A handle to a virtual machine.</param>
        public VMWareVirtualMachine(IVM2 vm)
            : base(vm)
        {
            _guestEnvironmentVariables = new VariableIndexer(_handle, Constants.VIX_GUEST_ENVIRONMENT_VARIABLE);
            _runtimeConfigVariables = new VariableIndexer(_handle, Constants.VIX_VM_CONFIG_RUNTIME_ONLY);
            _guestVariables = new VariableIndexer(_handle, Constants.VIX_VM_GUEST_VARIABLE);
            _sharedFolders = new VMWareSharedFolderCollection(_handle);
            _snapshots = new VMWareRootSnapshotCollection(_handle);
        }


        /// <summary>
        /// The path to the virtual machine configuration file.
        /// </summary>
        public string PathName
        {
            get
            {
                return GetProperty<string>(Constants.VIX_PROPERTY_VM_VMX_PATHNAME);
            }
        }

        /// <summary>
        /// Returns true if the virtual machine is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return GetProperty<bool>(Constants.VIX_PROPERTY_VM_IS_RUNNING);
            }
        }

        /// <summary>
        /// Returns virtual machine powerstate, an OR-ed set of VIX_POWERSTATE_* values.
        /// </summary>
        public int PowerState
        {
            get
            {
                return GetProperty<int>(Constants.VIX_PROPERTY_VM_POWER_STATE);
            }
        }

        /// <summary>
        /// Returns true if the virtual machine is paused.
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return (PowerState & Constants.VIX_POWERSTATE_PAUSED) > 0;
            }
        }

        /// <summary>
        /// Returns true if the virtual machine is suspended.
        /// </summary>
        public bool IsSuspended
        {
            get
            {
                return (PowerState & Constants.VIX_POWERSTATE_SUSPENDED) > 0;
            }
        }

        /// <summary>
        /// The memory size of the virtual machine. 
        /// </summary>
        public int MemorySize
        {
            get
            {
                return GetProperty<int>(Constants.VIX_PROPERTY_VM_MEMORY_SIZE);
            }
        }

        /// <summary>
        /// The number of virtual CPUs configured for the virtual machine.
        /// </summary>
        public int CPUCount
        {
            get
            {
                return GetProperty<int>(Constants.VIX_PROPERTY_VM_NUM_VCPUS);
            }
        }

        /// <summary>
        /// Power on a virtual machine.
        /// </summary>
        public void PowerOn()
        {
            PowerOn(VMWareInterop.Timeouts.PowerOnTimeout);
        }

        /// <summary>
        /// Power on a virtual machine.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void PowerOn(int timeoutInSeconds)
        {
            PowerOn(Constants.VIX_VMPOWEROP_NORMAL | Constants.VIX_VMPOWEROP_LAUNCH_GUI,
                timeoutInSeconds);
        }

        /// <summary>
        /// Power on a virtual machine.
        /// </summary>
        /// <param name="powerOnOptions">Additional power options.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void PowerOn(int powerOnOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.PowerOn(
                    powerOnOptions, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to power on virtual machine: powerOnOptions={0} timeoutInSeconds={1}",
                    powerOnOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// This function returns when VMware Tools has successfully started in the guest operating system. 
        /// VMware Tools is a collection of services that run in the guest. 
        /// </summary>
        public void WaitForToolsInGuest()
        {
            WaitForToolsInGuest(VMWareInterop.Timeouts.WaitForToolsTimeout);
        }

        /// <summary>
        /// This function returns when VMware Tools has successfully started in the guest operating system. 
        /// VMware Tools is a collection of services that run in the guest. 
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void WaitForToolsInGuest(int timeoutInSeconds)
        {
            try
            {
                // wait till the machine boots or times out with an error
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(
                    _handle.WaitForToolsInGuest(timeoutInSeconds, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to wait for tools in guest: timeoutInSeconds={0}", 
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Get all snapshots.
        /// </summary>
        /// <returns>A list of snapshots.</returns>
        public VMWareRootSnapshotCollection Snapshots
        {
            get
            {
                return _snapshots;
            }
        }

        /// <summary>
        /// This function establishes a guest operating system authentication context. 
        /// </summary>
        /// <param name="username">The name of a user account on the guest operating system.</param>
        /// <param name="password">The password of the account identified by userName.</param>
        public void LoginInGuest(string username, string password)
        {
            LoginInGuest(username, password, VMWareInterop.Timeouts.LoginTimeout);
        }

        /// <summary>
        /// This function establishes a guest operating system authentication context.
        /// </summary>
        /// <param name="username">The name of a user account on the guest operating system.</param>
        /// <param name="password">The password of the account identified by userName.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void LoginInGuest(string username, string password, int timeoutInSeconds)
        {
            LoginInGuest(username, password, 0, timeoutInSeconds);
        }

        /// <summary>
        /// This function establishes a guest operating system authentication context.
        /// </summary>
        /// <param name="username">The name of a user account on the guest operating system.</param>
        /// <param name="password">The password of the account identified by userName.</param>
        /// <param name="options">
        ///  Must be 0 or VixCOM.Constants.VIX_LOGIN_IN_GUEST_REQUIRE_INTERACTIVE_ENVIRONMENT, which forces interactive 
        ///  guest login within a graphical session that is visible to the user. On Linux, interactive environment 
        ///  requires that the X11 window system be running to start the vmware-user process. Without X11, pass 0 as 
        ///  options to start the vmware-guestd process instead.
        /// </param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <remarks>
        /// Logins are supported on Linux and Windows. To log in as a Windows Domain user, specify the "userName" parameter in 
        /// the form "domain\username". Other guest operating systems are not supported for login, including Solaris, FreeBSD, 
        /// and Netware.
        /// </remarks>
        public void LoginInGuest(string username, string password, int options, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.LoginInGuest(
                    username, password, options, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to login in guest: username=\"{0}\" options={1} timeoutInSeconds={2}", 
                    username, options, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// This function waits for the vmwareuser process to exist in the guest.
        /// </summary>
        /// <param name="username">The name of a user account on the guest operating system.</param>
        /// <param name="password">The password of the account identified by userName.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void WaitForVMWareUserProcessInGuest(string username, string password, int timeoutInSeconds)
        {
            //http://communities.vmware.com/message/1154264
            bool loggedIn = false;

            try
            {
                LoginInGuest(username, password, timeoutInSeconds);

                loggedIn = true;

                DateTime dtStart = DateTime.Now;

                while (true)
                {
                    VMWareProcessCollection guestProcesses = this.GuestProcesses;

                    Process vmwareUserExeProcess = guestProcesses.FindProcess("vmwareuser.exe", StringComparison.OrdinalIgnoreCase);
                    if (vmwareUserExeProcess != null)
                    {
                        break;
                    }

                    vmwareUserExeProcess = guestProcesses.FindProcess("vmware-user", StringComparison.OrdinalIgnoreCase);
                    if (vmwareUserExeProcess != null)
                    {
                        break;
                    }

                    TimeSpan ts = DateTime.Now.Subtract(dtStart);
                    if (ts.TotalSeconds >= timeoutInSeconds)
                    {
                        throw new VMWareException(Constants.VIX_E_TIMEOUT_WAITING_FOR_TOOLS, "vmwareuser");
                    }
                }
            }
            finally
            {
                if (loggedIn)
                {
                    try
                    {
                        LogoutFromGuest(timeoutInSeconds);
                    }
                    catch
                    {
                        //ignore this exception so that it does not swallow any previous exceptions
                    }
                }
            }
        }

        /// <summary>
        /// This function waits for the vmwareuser process to exist in the guest.
        /// </summary>
        /// <param name="username">The name of a user account on the guest operating system.</param>
        /// <param name="password">The password of the account identified by userName.</param>
        public void WaitForVMWareUserProcessInGuest(string username, string password)
        {
            WaitForVMWareUserProcessInGuest(username, password, VMWareInterop.Timeouts.WaitForToolsTimeout);
        }

        /// <summary>
        /// Copies a file or directory from the local system (where the Vix client is running) to the guest operating system.
        /// </summary>
        /// <param name="hostPathName">File location on the host operating system.</param>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        public void CopyFileFromHostToGuest(string hostPathName, string guestPathName)
        {
            CopyFileFromHostToGuest(hostPathName, guestPathName, VMWareInterop.Timeouts.CopyFileTimeout);
        }

        /// <summary>
        /// Copies a file or directory from the local system (where the Vix client is running) to the guest operating system.
        /// You must call LoginInGuest() before calling this procedure.
        /// Only absolute paths should be used for files in the guest; the resolution of relative paths is not specified.
        /// </summary>
        /// <param name="hostPathName">File location on the host operating system.</param>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void CopyFileFromHostToGuest(string hostPathName, string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.CopyFileFromHostToGuest(
                    hostPathName, guestPathName, 0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to copy file from host to guest: hostPathName=\"{0}\" guestPathName=\"{1}\" timeoutInSeconds={2}", 
                    hostPathName, guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Deletes a file from guest file system.
        /// </summary>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        public void DeleteFileFromGuest(string guestPathName)
        {
            DeleteFileFromGuest(guestPathName, VMWareInterop.Timeouts.DeleteFileTimeout);
        }

        /// <summary>
        /// Deletes a file from guest file system.
        /// </summary>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void DeleteFileFromGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.DeleteFileInGuest(
                    guestPathName, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to delete file from guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Deletes a directory from guest directory system.
        /// </summary>
        /// <param name="guestPathName">Directory location on the guest operating system.</param>
        public void DeleteDirectoryFromGuest(string guestPathName)
        {
            DeleteDirectoryFromGuest(guestPathName, VMWareInterop.Timeouts.DeleteDirectoryTimeout);
        }

        /// <summary>
        /// Deletes a directory from guest directory system.
        /// </summary>
        /// <param name="guestPathName">Directory location on the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void DeleteDirectoryFromGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.DeleteDirectoryInGuest(
                    guestPathName, 0, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to delete directory from guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Copies a file or directory from the guest operating system to the local system (where the Vix client is running).
        /// </summary>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        /// <param name="hostPathName">File location on the host operating system.</param>
        public void CopyFileFromGuestToHost(string guestPathName, string hostPathName)
        {
            CopyFileFromGuestToHost(guestPathName, hostPathName, VMWareInterop.Timeouts.CopyFileTimeout);
        }

        /// <summary>
        /// Copies a file or directory from the guest operating system to the local system (where the Vix client is running).
        /// You must call LoginInGuest() before calling this procedure.
        /// Only absolute paths should be used for files in the guest; the resolution of relative paths is not specified. 
        /// </summary>
        /// <param name="guestPathName">File location on the guest operating system.</param>
        /// <param name="hostPathName">File location on the host operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void CopyFileFromGuestToHost(string guestPathName, string hostPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.CopyFileFromGuestToHost(
                    guestPathName, hostPathName, 0, null, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to copy file from guest to host: guestPathName=\"{0}\" hostPathName=\"{1}\" timeoutInSeconds={2}",
                    guestPathName, hostPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Creates a directory on the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Directory location on the guest operating system.</param>
        public void CreateDirectoryInGuest(string guestPathName)
        {
            CreateDirectoryInGuest(guestPathName, VMWareInterop.Timeouts.CreateDirectoryTimeout);
        }

        /// <summary>
        /// Creates a directory on the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Directory location on the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void CreateDirectoryInGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.CreateDirectoryInGuest(
                    guestPathName, null, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to create directory in guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Creates a temp file on the guest operating system.
        /// </summary>
        /// <returns>Name of the temporary file created.</returns>
        public string CreateTempFileInGuest()
        {
            return CreateTempFileInGuest(VMWareInterop.Timeouts.CreateTempFileTimeout);
        }

        /// <summary>
        /// Creates a temp file on the guest operating system.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Name of the temporary file created.</returns>
        public string CreateTempFileInGuest(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.CreateTempFileInGuest(
                    0, null, callback),
                    callback))
                {
                    return job.Wait<string>(Constants.VIX_PROPERTY_JOB_RESULT_ITEM_NAME, timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to create temp file in guest: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Return information about a file or directory in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">File or path in the guest operating system.</param>
        /// <returns>Guest file information.</returns>
        public GuestFileInfo GetFileInfoInGuest(string guestPathName)
        {
            return GetFileInfoInGuest(guestPathName, VMWareInterop.Timeouts.GetFileInfoTimeout);
        }

        /// <summary>
        /// Return information about a file or directory in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">File or path in the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Guest file information.</returns>
        public GuestFileInfo GetFileInfoInGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.GetFileInfoInGuest(guestPathName, callback), callback))
                {
                    object[] properties = 
                    { 
                        Constants.VIX_PROPERTY_JOB_RESULT_FILE_SIZE,
                        Constants.VIX_PROPERTY_JOB_RESULT_FILE_FLAGS,
                        Constants.VIX_PROPERTY_JOB_RESULT_FILE_MOD_TIME
                    };
                    object[] propertyValues = job.Wait<object[]>(properties, timeoutInSeconds);
                    GuestFileInfo fileInfo = new GuestFileInfo();
                    fileInfo.GuestPathName = guestPathName;
                    fileInfo.FileSize = (long)propertyValues[0];
                    fileInfo.Flags = (int)propertyValues[1];
                    fileInfo.LastModified = VMWareInterop.FromUnixEpoch((long)propertyValues[2]);
                    return fileInfo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to get file info in guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Runs a program in the guest operating system.
        /// </summary>       
        /// <param name="guestProgramName">Program to execute.</param>
        /// <returns>Process information.</returns>
        public Process RunProgramInGuest(string guestProgramName)
        {
            return RunProgramInGuest(guestProgramName, string.Empty);
        }

        /// <summary>
        /// Run a program in the guest operating system.
        /// </summary>
        /// <param name="commandLineArgs">Additional command line arguments.</param>
        /// <param name="guestProgramName">Program to execute.</param>
        /// <returns>Process information.</returns>
        public Process RunProgramInGuest(string guestProgramName, string commandLineArgs)
        {
            return RunProgramInGuest(guestProgramName, commandLineArgs,
                Constants.VIX_RUNPROGRAM_ACTIVATE_WINDOW,
                VMWareInterop.Timeouts.RunProgramTimeout);
        }

        /// <summary>
        /// Run a detached program in the guest operating system.
        /// </summary>
        /// <param name="guestProgramName">Program to execute.</param>
        /// <returns>Process information.</returns>
        public Process DetachProgramInGuest(string guestProgramName)
        {
            return DetachProgramInGuest(guestProgramName, string.Empty);
        }

        /// <summary>
        /// Run a detached program in the guest operating system.
        /// </summary>
        /// <param name="guestProgramName">Program to execute.</param>
        /// <param name="commandLineArgs">Additional command line arguments.</param>
        /// <returns>Process information.</returns>
        public Process DetachProgramInGuest(string guestProgramName, string commandLineArgs)
        {
            return DetachProgramInGuest(guestProgramName, commandLineArgs,
                VMWareInterop.Timeouts.RunProgramTimeout);
        }

        /// <summary>
        /// Run a detached program in the guest operating system.
        /// </summary>
        /// <param name="guestProgramName">Program to execute.</param>
        /// <param name="commandLineArgs">Additional command line arguments.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Process information.</returns>
        public Process DetachProgramInGuest(string guestProgramName, string commandLineArgs, int timeoutInSeconds)
        {
            return RunProgramInGuest(guestProgramName, commandLineArgs,
                Constants.VIX_RUNPROGRAM_ACTIVATE_WINDOW | Constants.VIX_RUNPROGRAM_RETURN_IMMEDIATELY,
                timeoutInSeconds);
        }

        /// <summary>
        /// Run a program in the guest operating system.
        /// </summary>
        /// <param name="guestProgramName">Guest program to run.</param>
        /// <param name="commandLineArgs">Additional command line arguments.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Process information.</returns>
        public Process RunProgramInGuest(string guestProgramName, string commandLineArgs, int timeoutInSeconds)
        {
            return RunProgramInGuest(guestProgramName, commandLineArgs, 0, 
                timeoutInSeconds);
        }

        /// <summary>
        /// Run a program in the guest operating system.
        /// </summary>
        /// <param name="guestProgramName">Guest program to run.</param>
        /// <param name="commandLineArgs">Additional command line arguments.</param>
        /// <param name="options">Additional options, one of VIX_RUNPROGRAM_RETURN_IMMEDIATELY or VIX_RUNPROGRAM_ACTIVATE_WINDOW.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Process information.</returns>
        public Process RunProgramInGuest(string guestProgramName, string commandLineArgs, int options, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.RunProgramInGuest(
                    guestProgramName, commandLineArgs, options, null, callback),
                    callback))
                {
                    object[] properties = 
                    { 
                        Constants.VIX_PROPERTY_JOB_RESULT_GUEST_PROGRAM_EXIT_CODE, 
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_ID,
                        // Constants.VIX_PROPERTY_JOB_RESULT_GUEST_PROGRAM_ELAPSED_TIME
                    };
                    object[] propertyValues = job.Wait<object[]>(properties, timeoutInSeconds);
                    Process process = new Process(_handle);
                    process.Name = Path.GetFileName(guestProgramName);
                    process.Command = guestProgramName;
                    if (!string.IsNullOrEmpty(commandLineArgs))
                    {
                        process.Command += " ";
                        process.Command += commandLineArgs;
                    }
                    process.ExitCode = (int)propertyValues[0];
                    process.Id = (long)propertyValues[1];
                    return process;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to run program in guest: guestProgramName=\"{0}\" commandLineArgs=\"{1}\"", 
                    guestProgramName, commandLineArgs), ex);
            }
        }

        /// <summary>
        /// Run a script in the guest operating system.
        /// </summary>
        /// <param name="interpreter">The path to the script interpreter.</param>
        /// <param name="scriptText">The text of the script.</param>
        /// <returns>Process information.</returns>
        public Process RunScriptInGuest(string interpreter, string scriptText)
        {
            return RunScriptInGuest(interpreter, scriptText, 0,
                VMWareInterop.Timeouts.RunScriptTimeout);
        }

        /// <summary>
        /// Detach a script in the guest operating system.
        /// </summary>
        /// <param name="interpreter">The path to the script interpreter.</param>
        /// <param name="scriptText">The text of the script.</param>
        /// <returns>Process information.</returns>
        public Process DetachScriptInGuest(string interpreter, string scriptText)
        {
            return DetachScriptInGuest(interpreter, scriptText, 
                VMWareInterop.Timeouts.RunScriptTimeout);
        }

        /// <summary>
        /// Detach a script in the guest operating system.
        /// </summary>
        /// <param name="interpreter">The path to the script interpreter.</param>
        /// <param name="scriptText">The text of the script.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Process information.</returns>
        public Process DetachScriptInGuest(string interpreter, string scriptText, int timeoutInSeconds)
        {
            return RunScriptInGuest(interpreter, scriptText,
                Constants.VIX_RUNPROGRAM_RETURN_IMMEDIATELY,
                timeoutInSeconds);
        }

        /// <summary>
        /// Run a script in the guest operating system.
        /// </summary>
        /// <param name="interpreter">The path to the script interpreter.</param>
        /// <param name="scriptText">The text of the script.</param>
        /// <param name="options">Run options for the program.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Process information.</returns>
        public Process RunScriptInGuest(string interpreter, string scriptText, int options, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.RunScriptInGuest(
                    interpreter, scriptText, options, null, callback),
                    callback))
                {
                    object[] properties = 
                { 
                    Constants.VIX_PROPERTY_JOB_RESULT_GUEST_PROGRAM_EXIT_CODE, 
                    Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_ID,
                    // Constants.VIX_PROPERTY_JOB_RESULT_GUEST_PROGRAM_ELAPSED_TIME
                };
                    object[] propertyValues = job.Wait<object[]>(properties, timeoutInSeconds);
                    Process process = new Process(_handle);
                    process.Name = Path.GetFileName(interpreter);
                    process.Command = interpreter;
                    process.ExitCode = (int)propertyValues[0];
                    process.Id = (long)propertyValues[1];
                    return process;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to run script in guest: interpreter=\"{0}\" scriptText=\"{1}\" options={2} timeoutInSeconds={3}",
                    interpreter, scriptText, options, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Open a browser window on the specified URL in the guest operating system.
        /// </summary>
        /// <param name="url">The url to be opened.</param>
        [Obsolete]
        public void OpenUrlInGuest(string url)
        {
            OpenUrlInGuest(url, VMWareInterop.Timeouts.OpenUrlTimeout);
        }

        /// <summary>
        /// Open a browser window on the specified URL in the guest operating system.
        /// </summary>
        /// <param name="url">The url to be opened.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        [Obsolete]
        public void OpenUrlInGuest(string url, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.OpenUrlInGuest(url, 0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to open url in guest: url=\"{0}\" timeoutInSeconds={1}",
                    url, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Tests the existence of a file in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Path to a file in the guest operating system.</param>
        /// <returns>True if the file exists in the guest operating system.</returns>
        public bool FileExistsInGuest(string guestPathName)
        {
            return FileExistsInGuest(guestPathName, VMWareInterop.Timeouts.FileExistsTimeout);
        }

        /// <summary>
        /// Tests the existence of a file in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Path to a file in the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>True if the file exists in the guest operating system.</returns>
        public bool FileExistsInGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.FileExistsInGuest(
                    guestPathName, callback),
                    callback))
                {
                    return job.Wait<bool>(Constants.VIX_PROPERTY_JOB_RESULT_GUEST_OBJECT_EXISTS, timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to check if file exists in guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Tests the existence of a directory in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Path to a directory in the guest operating system.</param>
        /// <returns>True if the directory exists in the guest operating system.</returns>
        public bool DirectoryExistsInGuest(string guestPathName)
        {
            return DirectoryExistsInGuest(guestPathName, VMWareInterop.Timeouts.DirectoryExistsTimeout);
        }

        /// <summary>
        /// Tests the existence of a directory in the guest operating system.
        /// </summary>
        /// <param name="guestPathName">Path to a directory in the guest operating system.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>True if the directory exists in the guest operating system.</returns>
        public bool DirectoryExistsInGuest(string guestPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.DirectoryExistsInGuest(
                    guestPathName, callback),
                    callback))
                {
                    return job.Wait<bool>(Constants.VIX_PROPERTY_JOB_RESULT_GUEST_OBJECT_EXISTS, timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to check if directory exists in guest: guestPathName=\"{0}\" timeoutInSeconds={1}",
                    guestPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Remove any guest operating system authentication context created by a previous call to LoginInGuest(), ie. Logout.
        /// </summary>
        public void LogoutFromGuest()
        {
            LogoutFromGuest(VMWareInterop.Timeouts.LogoutTimeout);
        }

        /// <summary>
        /// Remove any guest operating system authentication context created by a previous call to LoginInGuest(), ie. Logout.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void LogoutFromGuest(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.LogoutFromGuest(callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to logout from guest: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Power off a virtual machine. The virtual machine will be powered off at the hardware level. 
        /// Any state of the guest that has not been committed to disk will be lost. 
        /// </summary>
        public void PowerOff()
        {
            PowerOff(Constants.VIX_VMPOWEROP_NORMAL, VMWareInterop.Timeouts.PowerOffTimeout);
        }

        /// <summary>
        /// Power off a virtual machine. The virtual machine will be powered off at the hardware level. 
        /// Any state of the guest that has not been committed to disk will be lost. 
        /// </summary>
        public void ShutdownGuest()
        {
            ShutdownGuest(VMWareInterop.Timeouts.PowerOffTimeout);
        }

        /// <summary>
        /// Power off a virtual machine. The virtual machine will be powered off at the hardware level. 
        /// Any state of the guest that has not been committed to disk will be lost. 
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void ShutdownGuest(int timeoutInSeconds)
        {
            PowerOff(Constants.VIX_VMPOWEROP_FROM_GUEST, timeoutInSeconds);
        }

        /// <summary>
        /// Power off or shutdown a virtual machine.
        /// If you call this function while the virtual machine is powered off or suspended, the operation will throw an 
        /// exception with a VIX_E_VM_NOT_RUNNING error.
        /// </summary>
        /// <param name="powerOffOptions">Power-off options. Passing the VIX_VMPOWEROP_FROM_GUEST flag will cause the function 
        /// to try to power off the guest OS. This will ensure a clean shutdown of the guest. This option requires that the 
        /// VMware Tools be installed and running in the guest. If VIX_VMPOWEROP_NORMAL is passed as the "powerOffOptions" parameter, 
        /// then the virtual machine will be powered off at the hardware level. Any state of the guest that has not been committed 
        /// to disk will be lost.
        /// </param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void PowerOff(int powerOffOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.PowerOff(powerOffOptions, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to power off virtual machine: powerOffOptions={0} timeoutInSeconds={1}",
                    powerOffOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Hardware reset the virtual machine.
        /// </summary>
        public void Reset()
        {
            Reset(Constants.VIX_VMPOWEROP_NORMAL);
        }

        /// <summary>
        /// Hardware reset the virtual machine.
        /// </summary>
        /// <param name="resetOptions">Reset options.
        /// Passing VIX_VMPOWEROP_NORMAL will force a hardware reset.
        /// Passing VIX_VMPOWEROP_FROM_GUEST will attempt a clean shutdown of the guest operating system.
        /// </param>
        public void Reset(int resetOptions)
        {
            Reset(resetOptions, VMWareInterop.Timeouts.ResetTimeout);
        }

        /// <summary>
        /// Reset a virtual machine.
        /// </summary>
        /// <param name="resetOptions">Reset options.
        /// Passing VIX_VMPOWEROP_NORMAL will force a hardware reset.
        /// Passing VIX_VMPOWEROP_FROM_GUEST will attempt a clean shutdown of the guest operating system.
        /// </param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Reset(int resetOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Reset(resetOptions, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to reset virtual machine: resetOptions={0} timeoutInSeconds={1}",
                    resetOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Suspend the virtual machine.
        /// </summary>
        public void Suspend()
        {
            Suspend(VMWareInterop.Timeouts.SuspendTimeout);
        }

        /// <summary>
        /// Suspend a virtual machine.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Suspend(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Suspend(0, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to suspend virtual machine: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Pause the virtual machine.
        /// </summary>
        public void Pause()
        {
            Pause(VMWareInterop.Timeouts.PauseTimeout);
        }

        /// <summary>
        /// Pause a virtual machine.
        /// This stops execution of the virtual machine. 
        /// Call Unpause to continue execution of the virtual machine. 
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Pause(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Pause(0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to pause virtual machine: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Continue execution of a virtual machine that was stopped using Pause. 
        /// </summary>
        public void Unpause()
        {
            Unpause(VMWareInterop.Timeouts.UnpauseTimeout);
        }

        /// <summary>
        /// Continue execution of a virtual machine that was stopped using Pause. 
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Unpause(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Unpause(0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to unpause virtual machine: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// List files in the guest operating system.
        /// </summary>
        /// <param name="pathName">Path in the guest operating system to list.</param>
        /// <param name="recurse">Recruse into subdirectories.</param>
        /// <returns>A list of files and directories with full paths.</returns>
        public List<string> ListDirectoryInGuest(string pathName, bool recurse)
        {
            return ListDirectoryInGuest(pathName, recurse, VMWareInterop.Timeouts.ListDirectoryTimeout);
        }

        /// <summary>
        /// List files in the guest operating system.
        /// </summary>
        /// <param name="pathName">Path in the guest operating system to list.</param>
        /// <param name="recurse">Recruse into subdirectories.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <remarks>
        /// The function throws an exception if pathName doesn't exist.
        /// </remarks>
        /// <returns>A list of files and directories with full paths.</returns>
        public List<string> ListDirectoryInGuest(string pathName, bool recurse, int timeoutInSeconds)
        {
            // ListDirectoryInGuest behaves differently on VMWare Workstation (returns empty list) and 
            // ESX (throws an exception) for directories or files that don't exist.
            if (!DirectoryExistsInGuest(pathName))
                throw new VMWareException(2);

            try
            {
                List<string> results = new List<string>();
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.ListDirectoryInGuest(
                    pathName, 0, callback), callback))
                {

                    object[] properties = 
                { 
                    Constants.VIX_PROPERTY_JOB_RESULT_ITEM_NAME, 
                    Constants.VIX_PROPERTY_JOB_RESULT_FILE_FLAGS
                };

                    try
                    {
                        foreach (object[] fileProperties in job.YieldWait(properties, timeoutInSeconds))
                        {
                            string fileName = (string)fileProperties[0];
                            int flags = (int)fileProperties[1];

                            if ((flags & 1) > 0)
                            {
                                if (recurse)
                                {
                                    results.AddRange(ListDirectoryInGuest(Path.Combine(pathName, fileName),
                                        true, timeoutInSeconds));
                                }
                            }
                            else
                            {
                                results.Add(Path.Combine(pathName, fileName));
                            }
                        }
                    }
                    catch (VMWareException ex)
                    {
                        switch (ex.ErrorCode)
                        {
                            case 2:
                            // file not found? empty directory in ESX
                            case Constants.VIX_E_UNRECOGNIZED_PROPERTY:
                                // unrecognized property returned by GetNumProperties, the directory exists, but contains no files
                                break;
                            default:
                                throw;
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to list directory in guest: pathName=\"{0}\" recurse={1} timeoutInSeconds={2}",
                    pathName, recurse, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// An environment variable in the guest of the VM. On a Windows NT series guest, writing these 
        /// values is saved persistently so they are immediately visible to every process. On a Linux or Windows 9X guest, 
        /// writing these values is not persistent so they are only visible to the VMware tools process. 
        /// </summary>
        public VariableIndexer GuestEnvironmentVariables
        {
            get
            {
                return _guestEnvironmentVariables;
            }
        }

        /// <summary>
        /// A "Guest Variable". This is a runtime-only value; it is never stored persistently. 
        /// This is the same guest variable that is exposed through the VMControl APIs, and is a simple 
        /// way to pass runtime values in and out of the guest. 
        /// VMWare doesn't publish a list of known variables, the following guest variables have been observed.
        /// <list type="bullet">
        /// <item>ip: IP address of the guest operating system.</item>
        /// </list>
        /// </summary>
        public VariableIndexer GuestVariables
        {
            get
            {
                return _guestVariables;
            }
        }

        /// <summary>
        /// The configuration state of the virtual machine. This is the .vmx file that is stored on the host. 
        /// You can read this and it will return the persistent data. If you write to this, it will only be a 
        /// runtime change, so changes will be lost when the VM powers off. 
        /// </summary>
        public VariableIndexer RuntimeConfigVariables
        {
            get
            {
                return _runtimeConfigVariables;
            }
        }

        /// <summary>
        /// Shared folders on this virtual machine.
        /// </summary>
        public VMWareSharedFolderCollection SharedFolders
        {
            get
            {
                return _sharedFolders;
            }
        }

        /// <summary>
        /// Captures the screen of the guest operating system.
        /// </summary>
        /// <returns>A <see cref="System.Drawing.Image"/> object holding the captured screen image.</returns>
        public Image CaptureScreenImage()
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.CaptureScreenImage(
                    Constants.VIX_CAPTURESCREENFORMAT_PNG, null, callback),
                    callback))
                {
                    byte[] imageBytes = job.Wait<byte[]>(
                        Constants.VIX_PROPERTY_JOB_RESULT_SCREEN_IMAGE_DATA,
                        VMWareInterop.Timeouts.CaptureScreenImageTimeout);
                    return Image.FromStream(new MemoryStream(imageBytes));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screen image", ex);
            }
        }

        /// <summary>
        /// Running processes in the guest operating system.
        /// </summary>
        public VMWareProcessCollection GuestProcesses
        {
            get
            {
                try
                {
                    VMWareProcessCollection processes = new VMWareProcessCollection();
                    VMWareJobCallback callback = new VMWareJobCallback();
                    using (VMWareJob job = new VMWareJob(_handle.ListProcessesInGuest(
                        0, callback), callback))
                    {
                        object[] properties = 
                    { 
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_ID,
                        Constants.VIX_PROPERTY_JOB_RESULT_ITEM_NAME,
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_OWNER,
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_START_TIME,
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_COMMAND,
                        Constants.VIX_PROPERTY_JOB_RESULT_PROCESS_BEING_DEBUGGED,
                    };

                        foreach (object[] processProperties in job.YieldWait(properties, VMWareInterop.Timeouts.ListProcessesTimeout))
                        {
                            Process process = new Process(_handle);
                            process.Id = (long)processProperties[0];
                            process.Name = (string)processProperties[1];
                            process.Owner = (string)processProperties[2];
                            process.StartDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((int)processProperties[3]);
                            process.Command = (string)processProperties[4];
                            process.IsBeingDebugged = (bool)processProperties[5];
                            processes.Add(process.Id, process);
                        }

                        return processes;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to list processes in guest", ex);
                }
            }
        }

        /// <summary>
        /// Returns true if the virtual machine is in the process of recording.
        /// </summary>
        public bool IsRecording
        {
            get
            {
                return GetProperty<bool>(Constants.VIX_PROPERTY_VM_IS_RECORDING);
            }
        }

        /// <summary>
        /// Returns true if the virtual machine is in the process of replaying.
        /// </summary>
        public bool IsReplaying
        {
            get
            {
                return GetProperty<bool>(Constants.VIX_PROPERTY_VM_IS_REPLAYING);
            }
        }

        /// <summary>
        /// Records a virtual machine's activity as a snapshot object.
        /// </summary>
        /// <param name="name">Snapshot name.</param>
        /// <returns>Resulting snapshot.</returns>
        public VMWareSnapshot BeginRecording(string name)
        {
            return BeginRecording(name, string.Empty);
        }

        /// <summary>
        /// Records a virtual machine's activity as a snapshot object.
        /// </summary>
        /// <param name="name">Snapshot name.</param>
        /// <param name="description">Snapshot description.</param>
        /// <returns>Resulting snapshot.</returns>
        public VMWareSnapshot BeginRecording(string name, string description)
        {
            return BeginRecording(name, description, VMWareInterop.Timeouts.RecordingTimeout);
        }

        /// <summary>
        /// Records a virtual machine's activity as a snapshot object.
        /// </summary>
        /// <param name="name">Snapshot name.</param>
        /// <param name="description">Snapshot description.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>Resulting snapshot.</returns>
        public VMWareSnapshot BeginRecording(string name, string description, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.BeginRecording(name, description, 0, null, callback), callback))
                {
                    VMWareSnapshot snapshot = new VMWareSnapshot(_handle,
                        job.Wait<ISnapshot>(Constants.VIX_PROPERTY_JOB_RESULT_HANDLE, timeoutInSeconds),
                        null);
                    _snapshots.Add(snapshot);
                    return snapshot;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to begin recording: name=\"{0}\" description=\"{1}\" timeoutInSeconds={2}", 
                    name, description, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// This function stops recording a virtual machine's activity.
        /// </summary>
        public void EndRecording()
        {
            EndRecording(VMWareInterop.Timeouts.RecordingTimeout);
        }

        /// <summary>
        /// This function stops recording a virtual machine's activity.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void EndRecording(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.EndRecording(
                    0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to end recording: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Upgrades the virtual hardware version of the virtual machine to match the version of the VIX library. 
        /// This has no effect if the virtual machine is already at the same version or at a newer version than the VIX library.
        /// </summary>
        public void UpgradeVirtualHardware()
        {
            UpgradeVirtualHardware(VMWareInterop.Timeouts.UpgradeVirtualHardwareTimeout);
        }

        /// <summary>
        /// Upgrades the virtual hardware version of the virtual machine to match the version of the VIX library. 
        /// This has no effect if the virtual machine is already at the same version or at a newer version than the VIX library.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void UpgradeVirtualHardware(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.UpgradeVirtualHardware(
                    0, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to upgrade virtual hardware: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Creates a copy of the virtual machine at current state.
        /// </summary>
        /// <param name="cloneType">Virtual Machine clone type.</param>
        /// <param name="destConfigPathName">The path name of the virtual machine configuration file that will be created.</param>
        public void Clone(VMWareVirtualMachineCloneType cloneType, string destConfigPathName)
        {
            Clone(cloneType, destConfigPathName, VMWareInterop.Timeouts.CloneTimeout);
        }

        /// <summary>
        /// Creates a copy of the virtual machine at current state.
        /// </summary>
        /// <param name="cloneType">Virtual Machine clone type.</param>
        /// <param name="destConfigPathName">The path name of the virtual machine configuration file that will be created.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Clone(VMWareVirtualMachineCloneType cloneType, string destConfigPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Clone(
                    null, (int)cloneType, destConfigPathName, 0, null, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to clone virtual machine: cloneType=\"{0}\" destConfigPathName=\"{1}\" timeoutInSeconds={2}",
                    Enum.GetName(cloneType.GetType(), cloneType), destConfigPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Permanently deletes a virtual machine from the host system.
        /// </summary>
        /// <remarks>
        /// Does not delete all associated files.
        /// </remarks>
        public void Delete()
        {
            Delete(0, VMWareInterop.Timeouts.DeleteTimeout);
        }

        /// <summary>
        /// Permanently deletes a virtual machine from the host system.
        /// </summary>
        /// <param name="deleteOptions">Delete options.
        /// <list type="bullet">
        ///  <item>VixCOM.Constants.VIX_VMDELETE_DISK_FILES: delete all associated files.</item>
        /// </list>
        /// </param>
        public void Delete(int deleteOptions)
        {
            Delete(deleteOptions, VMWareInterop.Timeouts.DeleteTimeout);
        }

        /// <summary>
        /// Permanently deletes a virtual machine from the host system.
        /// </summary>
        /// <param name="deleteOptions">
        /// <list type="bullet">
        ///  <item>VixCOM.Constants.VIX_VMDELETE_DISK_FILES: delete all associated files.</item>
        /// </list>
        /// </param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Delete(int deleteOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.Delete(deleteOptions, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to delete virtual machine: deleteOptions={0} timeoutInSeconds={1}",
                    deleteOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Prepares to install VMware Tools on the guest operating system.
        /// </summary>
        /// <remarks>
        /// Prepares an ISO image to install VMware Tools on the guest operating system. 
        /// If autorun is enabled, as it often is on Windows, installation begins, otherwise 
        /// you must initiate installation. If VMware Tools is already installed, this function 
        /// prepares to upgrade it to the version matching the product. 
        /// </remarks>
        public void InstallTools()
        {
            InstallTools(VMWareInterop.Timeouts.InstallToolsTimeout);
        }

        /// <summary>
        /// Prepares to install VMware Tools on the guest operating system.
        /// </summary>
        /// <remarks>
        /// Prepares an ISO image to install VMware Tools on the guest operating system. 
        /// If autorun is enabled, as it often is on Windows, installation begins, otherwise 
        /// you must initiate installation. If VMware Tools is already installed, this function 
        /// prepares to upgrade it to the version matching the product. 
        /// </remarks>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void InstallTools(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.InstallTools(0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to install tools: timeoutInSeconds={0}",
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Dispose the virtual machine object.
        /// </summary>
        public override void Dispose()
        {
            _guestVariables = null;
            _runtimeConfigVariables = null;
            _guestEnvironmentVariables = null;
            
            if (_snapshots != null)
            {
                _snapshots.Dispose();
                _snapshots = null;
            }

            _sharedFolders = null;
            base.Dispose();
        }
    }
}
