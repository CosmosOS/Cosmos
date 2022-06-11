using System;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System;

public enum TimeZoneInfoOptions
{
    None = 0x1,
    NoThrowOnInvalidTime = 0x2
}

[Plug(typeof(TimeZoneInfo))]
internal class TimeZoneInfoImpl
{
    public static TimeZoneInfo get_Local() => throw new NotImplementedException();

    public static TimeSpan GetUtcOffset(TimeZoneInfo aThis, DateTime aDateTime) => throw new NotImplementedException();

    [PlugMethod(Signature =
        "System_TimeSpan__System_TimeZoneInfo_GetLocalUtcOffset__System_DateTime_System_TimeZoneInfoOptions_")]
    public static TimeSpan GetLocalUtcOffset(DateTime aDateTime, TimeZoneInfoOptions aOptions) =>
        throw new NotImplementedException();

    public static string TryGetLocalizedNameByMuiNativeResource(string aString) => throw new NotImplementedException();
}
