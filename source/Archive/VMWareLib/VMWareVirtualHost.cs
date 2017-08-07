using System;
using System.Collections.Generic;
using Interop.VixCOM;
using Microsoft.Win32;

namespace Vestris.VMWareLib
{
    /// <summary>
    /// A VMWare virtual host.
    /// </summary>
    public class VMWareVirtualHost : VMWareVixHandle<IHost>
    {
        /// <summary>
        /// VMWare service provider type.
        /// </summary>
        public enum ServiceProviderType
        {
            /// <summary>
            /// No service provider type, not connected.
            /// </summary>
            None = 0,
            /// <summary>
            /// VMWare Server.
            /// </summary>
            Server = Constants.VIX_SERVICEPROVIDER_VMWARE_SERVER,
            /// <summary>
            /// VMWare Workstation.
            /// </summary>
            Workstation = Constants.VIX_SERVICEPROVIDER_VMWARE_WORKSTATION,
            /// <summary>
            /// Virtual Infrastructure Server, eg. ESX.
            /// </summary>
            VirtualInfrastructureServer = Constants.VIX_SERVICEPROVIDER_VMWARE_VI_SERVER,
            /// <summary>
            /// VMWare Player.
            /// </summary>
            Player = Constants.VIX_SERVICEPROVIDER_VMWARE_PLAYER
        }

        private ServiceProviderType _serviceProviderType = ServiceProviderType.None;

        /// <summary>
        /// An IHost2 handle, where supported.
        /// </summary>
        protected IHost2 _host2
        {
            get
            {
                return _handle as IHost2;
            }
        }

        /// <summary>
        /// A VMWare virtual host.
        /// </summary>
        public VMWareVirtualHost()
        {

        }

        /// <summary>
        /// Connected host type.
        /// </summary>
        public ServiceProviderType ConnectionType
        {
            get
            {
                return _serviceProviderType;
            }
        }

        /// <summary>
        /// Connect to a WMWare Player.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWarePlayer();
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("C:\Virtual Machines\xp\xp.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        public void ConnectToVMWarePlayer()
        {
            ConnectToVMWarePlayer(VMWareInterop.Timeouts.ConnectTimeout);
        }

        /// <summary>
        /// Connect to a WMWare Player.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void ConnectToVMWarePlayer(int timeoutInSeconds)
        {
            Connect(ServiceProviderType.Player, null, 0, null, null, timeoutInSeconds);
        }

        /// <summary>
        /// Connect to a WMWare Workstation.
        /// </summary>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWareWorkstation();
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("C:\Virtual Machines\xp\xp.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        public void ConnectToVMWareWorkstation()
        {
            ConnectToVMWareWorkstation(VMWareInterop.Timeouts.ConnectTimeout);
        }

        /// <summary>
        /// Connect to a WMWare Workstation.
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void ConnectToVMWareWorkstation(int timeoutInSeconds)
        {
            Connect(ServiceProviderType.Workstation, null, 0, null, null, timeoutInSeconds);
        }

        /// <summary>
        /// Connect to a WMWare Virtual Infrastructure Server (eg. ESX or VMWare Server 2.x).
        /// </summary>
        /// <param name="hostName">VMWare host name and optional port.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWareVIServer("esx.mycompany.com", "vmuser", "password");
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("[storage] testvm/testvm.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWareVIServer("localhost:8333", "vmuser", "password");
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("[standard] testvm/testvm.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        public void ConnectToVMWareVIServer(string hostName, string username, string password)
        {
            ConnectToVMWareVIServer(hostName, username, password, VMWareInterop.Timeouts.ConnectTimeout);
        }

        /// <summary>
        /// Connect to a WMWare Virtual Infrastructure Server (eg. ESX or VMWare Server 2.x).
        /// </summary>
        /// <param name="hostName">VMWare host name and optional port.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWareVIServer("esx.mycompany.com", "vmuser", "password");
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("[storage] testvm/testvm.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// using System;
        /// using System.Collections.Generic;
        /// using Vestris.VMWareLib;
        /// 
        /// VMWareVirtualHost virtualHost = new VMWareVirtualHost();
        /// virtualHost.ConnectToVMWareVIServer("localhost:8333", "vmuser", "password");
        /// VMWareVirtualMachine virtualMachine = virtualHost.Open("[standard] testvm/testvm.vmx");
        /// virtualMachine.PowerOn();
        /// </code>
        /// </example>
        public void ConnectToVMWareVIServer(string hostName, string username, string password, int timeoutInSeconds)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                throw new ArgumentException("The host is required.");
            }

            ConnectToVMWareVIServer(new Uri(string.Format("https://{0}/sdk", hostName)),
                username, password, timeoutInSeconds);
        }

        /// <summary>
        /// Connect to a WMWare Virtual Infrastructure Server (eg. ESX).
        /// </summary>
        /// <param name="hostUri">Host SDK uri, eg. http://server/sdk.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void ConnectToVMWareVIServer(Uri hostUri, string username, string password, int timeoutInSeconds)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("The username is required.");
            }

            Connect(ServiceProviderType.VirtualInfrastructureServer,
                hostUri.ToString(), 0, username, password, timeoutInSeconds);
        }

        /// <summary>
        /// Connect to a WMWare Server.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="hostName">DNS name or IP address of a VMWare host, leave blank for localhost.</param>
        public void ConnectToVMWareServer(string hostName, string username, string password)
        {
            ConnectToVMWareServer(hostName, username, password, VMWareInterop.Timeouts.ConnectTimeout);
        }

        /// <summary>
        /// Connect to a WMWare Server.
        /// </summary>
        /// <param name="hostName">DNS name or IP address of a VMWare host, leave blank for localhost.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void ConnectToVMWareServer(string hostName, string username, string password, int timeoutInSeconds)
        {
            Connect(ServiceProviderType.Server,
                hostName, 0, username, password, timeoutInSeconds);
        }

        /// <summary>
        /// Connects to a VMWare VI Server, VMWare Server or Workstation.
        /// </summary>
        private void Connect(ServiceProviderType serviceProviderType,
            string hostName, int hostPort, string username, string password, int timeout)
        {
            try
            {
                // check if VmWare COM object could be loaded
                // if we try to load a COM object which does not exist, an RCW exception will occure on exit of Visual Studio 2013
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32)) {
                  using (var xReg = baseKey.OpenSubKey(@"CLSID\{6874E949-7186-4308-A1B9-D55A91F60728}", false)) {
                    if (xReg == null) {
                      throw new ApplicationException("No Vmware Library installed!");
                    }
                  }
                }

                int serviceProvider = (int)serviceProviderType;
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(new VixLib().Connect(
                    Constants.VIX_API_VERSION, serviceProvider, hostName, hostPort,
                    username, password, 0, null, callback), callback))
                {
                    _handle = job.Wait<IHost>(Constants.VIX_PROPERTY_JOB_RESULT_HANDLE, timeout);
                }
                _serviceProviderType = serviceProviderType;
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to connect: serviceProviderType=\"{0}\" hostName=\"{1}\" hostPort={2} username=\"{3}\" timeout={4}", 
                    Enum.GetName(serviceProviderType.GetType(), serviceProviderType), hostName, hostPort, username, timeout), ex);
            }
        }

        /// <summary>
        /// Open a virtual machine.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        /// <returns>An instance of a virtual machine.</returns>
        public VMWareVirtualMachine Open(string fileName)
        {
            return Open(fileName, VMWareInterop.Timeouts.OpenVMTimeout);
        }

        /// <summary>
        /// Open a virtual machine.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        /// <returns>An instance of a virtual machine.</returns>
        public VMWareVirtualMachine Open(string fileName, int timeoutInSeconds)
        {
            try
            {
                if (_handle == null)
                {
                    throw new InvalidOperationException("No connection established");
                }

                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.OpenVM(fileName, callback), callback))
                {
                    return new VMWareVirtualMachine(job.Wait<IVM2>(
                        Constants.VIX_PROPERTY_JOB_RESULT_HANDLE,
                        timeoutInSeconds));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to open virtual machine: fileName=\"{0}\" timeoutInSeconds={1}",
                    fileName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Add a virtual machine to the host's inventory.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        public void Register(string fileName)
        {
            Register(fileName, VMWareInterop.Timeouts.RegisterVMTimeout);
        }

        /// <summary>
        /// Add a virtual machine to the host's inventory.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Register(string fileName, int timeoutInSeconds)
        {
            if (_handle == null)
            {
                throw new InvalidOperationException("No connection established");
            }

            switch (ConnectionType)
            {
                case ServiceProviderType.VirtualInfrastructureServer:
                case ServiceProviderType.Server:
                    break;
                default:
                    throw new NotSupportedException(string.Format("Register is not supported on {0}",
                        ConnectionType));
            }

            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.RegisterVM(fileName, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to register virtual machine: fileName=\"{0}\" timeoutInSeconds={1}",
                    fileName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Remove a virtual machine from the host's inventory.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        public void Unregister(string fileName)
        {
            Unregister(fileName, VMWareInterop.Timeouts.RegisterVMTimeout);
        }

        /// <summary>
        /// Remove a virtual machine from the host's inventory.
        /// </summary>
        /// <param name="fileName">Virtual Machine file, local .vmx or [storage] .vmx.</param>
        /// <param name="timeoutInSeconds">Timeout in seconds.</param>
        public void Unregister(string fileName, int timeoutInSeconds)
        {
            if (_handle == null)
            {
                throw new InvalidOperationException("No connection established");
            }

            switch (ConnectionType)
            {
                case ServiceProviderType.VirtualInfrastructureServer:
                case ServiceProviderType.Server:
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unregister is not supported on {0}",
                        ConnectionType));
            }

            try
            {
                VMWareJobCallback callback = new VMWareJobCallback();
                using (VMWareJob job = new VMWareJob(_handle.UnregisterVM(fileName, callback), callback))
                {
                    job.Wait(timeoutInSeconds);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Failed to unregister virtual machine: fileName=\"{0}\" timeoutInSeconds={1}",
                    fileName, timeoutInSeconds), ex);
            }
        }

        /// <summary>
        /// Dispose the object, hard-disconnect from the remote host.
        /// </summary>
        public override void Dispose()
        {
            if (_handle != null)
            {
                Disconnect();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnect from a remote host.
        /// </summary>
        public void Disconnect()
        {
            if (_handle == null)
            {
                throw new InvalidOperationException("No connection established");
            }

            _handle.Disconnect();
            _handle = null;
            _serviceProviderType = ServiceProviderType.None;
        }

        /// <summary>
        /// Returns true when connected to a virtual host, false otherwise.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _handle != null;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~VMWareVirtualHost()
        {
            if (_handle != null)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Returns all running virtual machines.
        /// </summary>
        public IEnumerable<VMWareVirtualMachine> RunningVirtualMachines
        {
            get
            {
                if (_handle == null)
                {
                    throw new InvalidOperationException("No connection established");
                }

                try
                {
                    List<VMWareVirtualMachine> runningVirtualMachines = new List<VMWareVirtualMachine>();

                    VMWareJobCallback callback = new VMWareJobCallback();
                    using (VMWareJob job = new VMWareJob(_handle.FindItems(
                        Constants.VIX_FIND_RUNNING_VMS, null, -1, callback),
                        callback))
                    {
                        object[] properties = { Constants.VIX_PROPERTY_FOUND_ITEM_LOCATION };
                        foreach (object[] runningVirtualMachine in job.YieldWait(
                            properties, VMWareInterop.Timeouts.FindItemsTimeout))
                        {
                            runningVirtualMachines.Add(this.Open((string)runningVirtualMachine[0]));
                        }
                    }

                    return runningVirtualMachines;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to get all running virtual machines", ex);
                }
            }
        }

        /// <summary>
        /// All registered virtual machines.
        /// </summary>
        /// <remarks>This function is only supported on Virtual Infrastructure servers.</remarks>
        public IEnumerable<VMWareVirtualMachine> RegisteredVirtualMachines
        {
            get
            {
                switch (ConnectionType)
                {
                    case ServiceProviderType.VirtualInfrastructureServer:
                    case ServiceProviderType.Server:
                        break;
                    default:
                        throw new NotSupportedException(string.Format("RegisteredVirtualMachines is not supported on {0}",
                            ConnectionType));
                }

                try
                {
                    List<VMWareVirtualMachine> registeredVirtualMachines = new List<VMWareVirtualMachine>();

                    VMWareJobCallback callback = new VMWareJobCallback();
                    using (VMWareJob job = new VMWareJob(_handle.FindItems(
                        Constants.VIX_FIND_REGISTERED_VMS, null, -1, callback),
                        callback))
                    {
                        object[] properties = { Constants.VIX_PROPERTY_FOUND_ITEM_LOCATION };
                        foreach (object[] runningVirtualMachine in job.YieldWait(properties, VMWareInterop.Timeouts.FindItemsTimeout))
                        {
                            registeredVirtualMachines.Add(this.Open((string)runningVirtualMachine[0]));
                        }
                    }

                    return registeredVirtualMachines;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to get all registered virtual machines", ex);
                }
            }
        }
    }
}