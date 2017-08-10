using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2 {
	// This is the base class for all hardware devices. Hardware devices 
	// are defined here as abstracts and overridden in specific implementations
	// later
	public abstract class Device:Hardware {
		public enum DeviceType {
			Unknown,
			Other,
			Keyboard,
			Mouse,
			Storage,
            Network
		};

        public delegate void InitializeDriver();

        private static List<InitializeDriver> mDriverInits;
        public static void AddDriverInit(InitializeDriver aInit)
        {
            if(mDriverInits==null)
            {
                mDriverInits = new List<InitializeDriver>(4);
            }
            mDriverInits.Add(aInit);
        }

        public static void DriversInit()
        {
            for (int d = 0; d < mDriverInits.Count; d++)
            {
                InitializeDriver dlgt = mDriverInits[d];
                dlgt();
            }
        }

		public Device() {
		}

		static protected List<Device> mDevices = new List<Device>(16);
		static public List<Device> Devices {
			get {
				return mDevices;
			}
		}

        static public void Add(Device aDevice)
        {
            if (aDevice == null)
            {
                throw new ArgumentNullException("aDevice");
            }
            Console.WriteLine("Adding device");
            mDevices.Add(aDevice);
        }

        public static string DeviceTypeToString(DeviceType aType) {
            switch (aType) {
                case DeviceType.Keyboard:
                    return "Keyboard";
                case DeviceType.Mouse:
                    return "Mouse";
                case DeviceType.Storage:
                    return "Storage";
                case DeviceType.Other:
                    return "Other";
                case DeviceType.Unknown:
                    return "Unknown";
                case DeviceType.Network:
                    return "Network";
                default:
                    return "<Value Unimplemented>";
            }
        }

	    public static Device FindFirst(DeviceType aType) {
			for (int i = 0; i < mDevices.Count; i++) {
				var xDevice = mDevices[i];
				if (xDevice.Type == aType) {
					return xDevice;
				}
			}
			return null;
		}

		static public List<Device> Find(DeviceType aType) {
			var xResult = new List<Device>();
			for (int i = 0; i < mDevices.Count;i++){
				var xDevice = mDevices[i];
				if (xDevice.Type == aType) {
					xResult.Add(xDevice);
				}
			}
			return xResult;
		}

		protected DeviceType mType;
		public DeviceType Type {
			get {
				return mType;
			}
		}

		public abstract string Name {
			get;
		}

        private bool _Enabled = false;
        public bool IsEnabled
        {
            get
            {
                return _Enabled;
            }
        }
        public virtual bool Enable()
        {
            _Enabled = true;
            return true;
        }

        public virtual bool Disable()
        {
            _Enabled = false;
            return true;
        }
	}
}
