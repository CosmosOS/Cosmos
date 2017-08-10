using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Interop.VixCOM;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// A VMWare snapshot.
    /// </summary>
    public class VMWareSnapshot : VMWareVixHandle<ISnapshot>
    {
        private IVM2 _vm = null;
        private VMWareSnapshotCollection _childSnapshots = null;
        private VMWareSnapshot _parent = null;

        /// <summary>
        /// A VMWare snapshot constructor.
        /// </summary>
        /// <param name="vm">Virtual machine.</param>
        /// <param name="snapshot">Snapshot.</param>
        /// <param name="parent">Parent snapshot.</param>
        public VMWareSnapshot(IVM2 vm, ISnapshot snapshot, VMWareSnapshot parent)
            : base(snapshot)
        {
            _vm = vm;
            _parent = parent;
        }

        /// <summary>
        /// Parent snapshot.
        /// </summary>
        /// <remarks>
        /// Root snapshots have a null parent.
        /// </remarks>
        public VMWareSnapshot Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }

        /// <summary>
        /// Restores the virtual machine to the state when the specified snapshot was created.
        /// </summary>
        /// <param name="powerOnOptions">
        ///  Any applicable VixVMPowerOpOptions. If the virtual machine was powered on when the snapshot was created, 
        ///  then this will determine how the virtual machine is powered back on. To prevent the virtual machine from being 
        ///  powered on regardless of the power state when the snapshot was created, use the 
        ///  VIX_VMPOWEROP_SUPPRESS_SNAPSHOT_POWERON flag. VIX_VMPOWEROP_SUPPRESS_SNAPSHOT_POWERON is mutually exclusive to 
        ///  all other VixVMPowerOpOptions. 
        /// </param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void RevertToSnapshot(int powerOnOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_vm.RevertToSnapshot(
                    _handle, powerOnOptions, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to revert to snapshot: powerOnOptions={0} timeoutInSeconds={1}", 
                    powerOnOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Restores the virtual machine to the state when the specified snapshot was created.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void RevertToSnapshot(int timeoutInSeconds)
        {
            RevertToSnapshot(Constants.VIX_VMPOWEROP_NORMAL, timeoutInSeconds);
        }

        /// <summary>
        /// Restores the virtual machine to the state when the specified snapshot was created.
        /// </summary>
        public void RevertToSnapshot()
        {
            RevertToSnapshot(VMWareInterop.Timeouts.RevertToSnapshotTimeout);
        }

        /// <summary>
        /// Remove/delete this snapshot.
        /// </summary>
        public void RemoveSnapshot()
        {
            RemoveSnapshot(VMWareInterop.Timeouts.RemoveSnapshotTimeout);
        }

        /// <summary>
        /// Remove/delete this snapshot.
        /// </summary>
        /// <remarks>
        /// If the snapshot is a member of a collection, the latter is updated with orphaned
        /// snapshots appended to the parent.
        /// </remarks>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void RemoveSnapshot(int timeoutInSeconds)
        {
            try
            {
                // resolve child snapshots that will move one level up
                IEnumerable<VMWareSnapshot> childSnapshots = ChildSnapshots;

                // remove the snapshot
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_vm.RemoveSnapshot(_handle, 0, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }

                // remove from parent
                if (_parent != null)
                {
                    // child snapshots from this snapshot have now moved one level up
                    _parent.ChildSnapshots.Remove(this);
                }

                Close();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to remove snapshot: timeoutInSeconds={0}", 
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Child snapshots.
        /// </summary>
        public VMWareSnapshotCollection ChildSnapshots
        {
            get
            {
                if (_childSnapshots == null)
                {
                    VMWareSnapshotCollection childSnapshots = new VMWareSnapshotCollection(_vm, this);
                    int nChildSnapshots = 0;
                    VMWareInterop.Check(_handle.GetNumChildren(out nChildSnapshots));
                    for (int i = 0; i < nChildSnapshots; i++)
                    {
                        ISnapshot childSnapshot = null;
                        VMWareInterop.Check(_handle.GetChild(i, out childSnapshot));
                        childSnapshots.Add(new VMWareSnapshot(_vm, childSnapshot, this));
                    }
                    _childSnapshots = childSnapshots;
                }
                return _childSnapshots;
            }
        }

        /// <summary>
        /// Display name of the snapshot.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return GetProperty<string>(Constants.VIX_PROPERTY_SNAPSHOT_DISPLAYNAME);
            }
        }

        /// <summary>
        /// Display name of the snapshot.
        /// </summary>
        public string Description
        {
            get
            {
                return GetProperty<string>(Constants.VIX_PROPERTY_SNAPSHOT_DESCRIPTION);
            }
        }

        /// <summary>
        /// Complete snapshot path, from root.
        /// </summary>
        public string Path
        {
            get
            {
                ISnapshot parentSnapshot = null;
                ulong ulError = 0;
                switch ((ulError = _handle.GetParent(out parentSnapshot)))
                {
                    case Constants.VIX_OK:
                        return parentSnapshot == null 
                            ? DisplayName 
                            : System.IO.Path.Combine(new VMWareSnapshot(_vm, parentSnapshot, null).Path, DisplayName);
                    case Constants.VIX_E_SNAPSHOT_NOTFOUND: // no parent
                        return DisplayName;
                    case Constants.VIX_E_INVALID_ARG: // root snapshot
                        return string.Empty;
                    default:
                        throw new VMWareException(ulError);
                }
            }
        }

        /// <summary>
        /// The power state of this snapshot, an OR-ed set of VIX_POWERSTATE_* values.
        /// </summary>
        public int PowerState
        {
            get
            {
                return GetProperty<int>(Constants.VIX_PROPERTY_SNAPSHOT_POWERSTATE);
            }
        }

        /// <summary>
        /// Returns true if the snapshot is replayable.
        /// </summary>
        public bool IsReplayable
        {
            get
            {
                return GetProperty<bool>(Constants.VIX_PROPERTY_SNAPSHOT_IS_REPLAYABLE);
            }
        }

        /// <summary>
        /// Replay a recording of a virtual machine. 
        /// </summary>
        public void BeginReplay()
        {
            BeginReplay(Constants.VIX_VMPOWEROP_NORMAL, 
                VMWareInterop.Timeouts.ReplayTimeout);
        }

        /// <summary>
        /// Replay a recording of a virtual machine. 
        /// </summary>
        /// <param name="powerOnOptions">One of VIX_VMPOWEROP_NORMAL or VIX_VMPOWEROP_LAUNCH_GUI.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void BeginReplay(int powerOnOptions, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_vm.BeginReplay(
                    _handle, powerOnOptions, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to begin replay: powerOnOptions={0} timeoutInSeconds={1}", 
                    powerOnOptions, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Stop replaying a virtual machine's recording.
        /// </summary>
        public void EndReplay()
        {
            EndReplay(VMWareInterop.Timeouts.ReplayTimeout);
        }

        /// <summary>
        /// Stop replaying a virtual machine's recording.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void EndReplay(int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_vm.EndReplay(
                    0, null, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to end replay: timeoutInSeconds={0}", 
                    timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Creates a copy of the virtual machine at the state at which this snapshot was taken.
        /// </summary>
        /// <param name="cloneType">Virtual Machine clone type.</param>
        /// <param name="destConfigPathName">The path name of the virtual machine configuration file that will be created.</param>
        public void Clone(VMWareVirtualMachineCloneType cloneType, string destConfigPathName)
        {
            Clone(cloneType, destConfigPathName, VMWareInterop.Timeouts.CloneTimeout);
        }

        /// <summary>
        /// Creates a copy of the virtual machine at the state at which this snapshot was taken.
        /// </summary>
        /// <param name="cloneType">Virtual Machine clone type.</param>
        /// <param name="destConfigPathName">The path name of the virtual machine configuration file that will be created.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Clone(VMWareVirtualMachineCloneType cloneType, string destConfigPathName, int timeoutInSeconds)
        {
            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_vm.Clone(
                    _handle, (int)cloneType, destConfigPathName, 0, null, callback),
                    callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to clone virtual machine snapshot: cloneType=\"{0}\" destConfigPathName=\"{1}\" timeoutInSeconds={2}",
                    Enum.GetName(cloneType.GetType(), cloneType), destConfigPathName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Dispose the snapshot.
        /// </summary>
        public override void Dispose()
        {
            if (_childSnapshots != null)
            {
                _childSnapshots.Dispose();
                _childSnapshots = null;
            }

            base.Dispose();            
        }
    }
}
