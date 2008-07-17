using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	// This is the base class for all hardware devices. Hardware devices 
	// are defined here as abstracts and overridden in specific implementations
	// later
	public abstract class Device:Hardware {
		public enum DeviceType {
			Unknown,
			Other,
			Keyboard,
			Mouse,
			Storage
		};

		public Device() {
		}

		static protected List<Device> mDevices = new List<Device>();
		static public List<Device> Devices {
			get {
				return mDevices;
			}
		}

		static public void Add(Device aDevice) {
			if (aDevice == null) {
				throw new ArgumentNullException("aDevice");
			}
			mDevices.Add(aDevice);
		}

        private static string DeviceTypeToString(DeviceType aType) {
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
                default:
                    return "<Value Unimplemented>";
            }
        }

	    public static Device FindFirst(DeviceType aType) {
            DebugUtil.SendNumber("Hardware", "DeviceCount in FindFirst", (uint)mDevices.Count, 32);
            DebugUtil.SendMessage("Hardware|Looking for device", DeviceTypeToString(aType));
			for (int i = 0; i < mDevices.Count; i++) {
                DebugUtil.SendNumber("Hardware", "DeviceId", (uint)i, 32);
				var xDevice = mDevices[i];
                DebugUtil.SendMessage("Hardware|DeviceType", DeviceTypeToString(xDevice.Type));
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
