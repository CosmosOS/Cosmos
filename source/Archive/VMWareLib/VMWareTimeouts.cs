using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// VMWare job timeout metadata.
    /// </summary>
    public class VMWareTimeoutAttribute : Attribute
    {
        private int _multiplier = 1;

        /// <summary>
        /// A default base timeout multiplier.
        /// </summary>
        public int Multiplier
        {
            get
            {
                return _multiplier;
            }
            set
            {
                _multiplier = value;
            }
        }
    }

    /// <summary>
    /// A collection of default timeouts used in VMWareTasks functions exposed without a timeout parameter.
    /// </summary>
    public class VMWareTimeouts
    {
        /// <summary>
        /// Maximum time, in seconds, to establish a connection to a VMWare host.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int ConnectTimeout;
        /// <summary>
        /// Maximum time, in seconds, to open a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int OpenVMTimeout;
        /// <summary>
        /// Maximum time, in seconds, to register or unregister a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int RegisterVMTimeout;
        /// <summary>
        /// Maximum time, in seconds, to revert a snapshot.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int RevertToSnapshotTimeout;
        /// <summary>
        /// Maximum time, in seconds, to remove (delete) a snapshot.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 10)]
        public int RemoveSnapshotTimeout;
        /// <summary>
        /// Maximum time, in seconds, to create a snapshot.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 10)]
        public int CreateSnapshotTimeout;
        /// <summary>
        /// The maximum operational time, in seconds, to bring the power to/from the vm, not to boot it
        /// </summary>
        [VMWareTimeoutAttribute]
        public int PowerOnTimeout;
        /// <summary>
        /// The maximum time, in seconds, to power off a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int PowerOffTimeout;
        /// <summary>
        /// The maximum time, in seconds, to reset a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int ResetTimeout;
        /// <summary>
        /// The maximum time, in seconds, to suspend a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int SuspendTimeout;
        /// <summary>
        /// The maximum time, in seconds, to pause a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int PauseTimeout;
        /// <summary>
        /// The maximum time, in seconds, to unpause (continue execution of) a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int UnpauseTimeout;
        /// <summary>
        /// The maximum time, in seconds, to wait for tools in a guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int WaitForToolsTimeout;
        /// <summary>
        /// The maximum time, in seconds, to wait for a log-in to a guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int LoginTimeout;
        /// <summary>
        /// Maximum time, in seconds, to copy a file from guest to host and from host to guest.
        /// <remarks>
        /// Copy is very slow, see http://communities.vmware.com/thread/184489.
        /// </remarks>
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 20)]
        public int CopyFileTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for a file to be deleted in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int DeleteFileTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for a directory to be deleted in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int DeleteDirectoryTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for a program to run in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int RunProgramTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for a script to run in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int RunScriptTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for an url to open in a browser on the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int OpenUrlTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to check whether a file exists in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int FileExistsTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to check whether a directory exists in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int DirectoryExistsTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for a logout from a guest operating system to complete.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int LogoutTimeout;
        /// <summary>
        /// Maximum time, in seconds, to list the contents of a directory in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int ListDirectoryTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to read a remote variable.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int ReadVariableTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to write a remote variable.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int WriteVariableTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to enable or disable shared folders.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int EnableSharedFoldersTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait to fetch the list of shared folders.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int GetSharedFoldersTimeout;
        /// <summary>
        /// Maximum time, in seconds, to add/remove a shared folder.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int AddRemoveSharedFolderTimeout;
        /// <summary>
        /// Maximum time, in seconds, to capture a screen image.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int CaptureScreenImageTimeout;
        /// <summary>
        /// Maximum time, in seconds, to create a directory in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int CreateDirectoryTimeout;
        /// <summary>
        /// Maximum time, in seconds, to create a temporary file in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int CreateTempFileTimeout;
        /// <summary>
        /// Maximum time, in seconds, to list processes in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int ListProcessesTimeout;
        /// <summary>
        /// Maximum time, in seconds, to fetch a collection of items in find operations.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int FindItemsTimeout;
        /// <summary>
        /// Maximum time, in seconds, to kill a process in the guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int KillProcessTimeout;
        /// <summary>
        /// Maximum time, in seconds, to begin and end a recording.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 10)]
        public int RecordingTimeout;
        /// <summary>
        /// Maximum time, in seconds, to replay a snapshot.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 10)]
        public int ReplayTimeout;
        /// <summary>
        /// Maximum time, in seconds, to wait for an upgrade for the virtual hardware.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int UpgradeVirtualHardwareTimeout;
        /// <summary>
        /// Maximum time, in seconds, to clone a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute(Multiplier = 5)]
        public int CloneTimeout;
        /// <summary>
        /// Maximum time, in seconds, to delete a virtual machine.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int DeleteTimeout;
        /// <summary>
        /// Maximum time, in seconds, to get file information from a guest operating system.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int GetFileInfoTimeout;
        /// <summary>
        /// Maximum time, in seconds, to prepare to install or upgrade VMWare Tools.
        /// </summary>
        [VMWareTimeoutAttribute]
        public int InstallToolsTimeout;

        /// <summary>
        /// A collection of timeouts based on a default 60-seconds base timeout.
        /// </summary>
        public VMWareTimeouts()
            : this(60)
        {
        }

        /// <summary>
        /// A collection of timeouts based on a configurable base timeout.
        /// </summary>
        /// <param name="baseTimeout">a base timeout</param>
        public VMWareTimeouts(int baseTimeout)
        {
            FieldInfo[] timeouts = GetType().GetFields();
            foreach (FieldInfo timeout in timeouts)
            {
                object[] timeoutAttributes = timeout.GetCustomAttributes(typeof(VMWareTimeoutAttribute), false);
                if (timeoutAttributes == null || timeoutAttributes.Length == 0)
                    continue;

                VMWareTimeoutAttribute timeoutAttribute = timeoutAttributes[0] as VMWareTimeoutAttribute;
                timeout.SetValue(this, baseTimeout * timeoutAttribute.Multiplier);
            }
        }
    }
}
