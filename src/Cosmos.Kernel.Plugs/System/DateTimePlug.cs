using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System;
#if ARCH_X64
using Cosmos.Kernel.HAL.X64.Devices.Clock;
#elif ARCH_ARM64
using Cosmos.Kernel.HAL.ARM64.Devices.Clock;
#endif

namespace Cosmos.Kernel.Plugs.System;

/// <summary>
/// Plug for System.DateTime to provide current time functionality.
/// Uses RTC for initial boot time and hardware counter for elapsed time tracking.
/// </summary>
[Plug(typeof(DateTime))]
public static partial class DateTimePlug
{
    private static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Returns the current time as DateTime ticks from the platform RTC.
    /// </summary>
    private static long GetCurrentTicks()
    {
        if (KernelFeatures.Timer)
        {
            if (RTC.Instance is null)
            {
                return 0;
            }

            return RTC.Instance.GetCurrentTicks();
        }
        else
        {
            return 0;
        }
    }

    /// <summary>Gets the current UTC date and time.</summary>
    [PlugMember("get_UtcNow")]
    public static DateTime get_UtcNow()
    {
        long ticks = GetCurrentTicks();
        return ticks > 0 ? new DateTime(ticks, DateTimeKind.Utc) : s_epoch;
    }

    /// <summary>
    /// Gets the current local date and time.
    /// Note: Returns UTC time as timezone support is not implemented.
    /// </summary>
    [PlugMember("get_Now")]
    public static DateTime get_Now()
    {
        long ticks = GetCurrentTicks();
        return ticks > 0 ? new DateTime(ticks, DateTimeKind.Local) : new DateTime(s_epoch.Ticks, DateTimeKind.Local);
    }

    /// <summary>Gets the current date (time component is 00:00:00).</summary>
    [PlugMember("get_Today")]
    public static DateTime get_Today()
    {
        return get_Now().Date;
    }
}
