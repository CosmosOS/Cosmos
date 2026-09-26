// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.TestRunner.Engine.Hosts;

namespace Cosmos.Tests.Patcher;

/// <summary>
/// How the engine reads a guest's host request: the words the test
/// framework sends, and every malformed shape, which the engine reports and
/// drops instead of acting on a guess.
/// </summary>
public class HostRequestTests
{
    // The kind is passed by name: HostRequestKind is internal to the engine,
    // and a public test method cannot take it.
    [Theory]
    [InlineData("usb-unplug", "StickUnplug", 0, 0, 0, 0)]
    [InlineData("usb-plug", "StickPlug", 0, 0, 0, 0)]
    [InlineData("usb-unplug 1", "StickUnplug", 1, 0, 0, 0)]
    [InlineData("usb-plug 0", "StickPlug", 0, 0, 0, 0)]
    [InlineData("usb-device-unplug 0", "DeviceUnplug", 0, 0, 0, 0)]
    [InlineData("usb-device-plug 2", "DevicePlug", 2, 0, 0, 0)]
    [InlineData("usb-pointer-move 0 5 -3", "PointerMove", 0, 5, -3, 0)]
    [InlineData("usb-pointer-move 1 -127 127 7", "PointerMove", 1, -127, 127, 7)]
    [InlineData("usb-pointer-move 0 0 0 2", "PointerMove", 0, 0, 0, 2)]
    [InlineData("  usb-device-plug   3 ", "DevicePlug", 3, 0, 0, 0)]
    public void Parse_ReadsEveryRequestTheFrameworkSends(string text, string kind, int index, int deltaX, int deltaY, int buttons)
    {
        HostRequest request = HostRequest.Parse(text);

        Assert.Equal(new HostRequest(Enum.Parse<HostRequestKind>(kind), index, deltaX, deltaY, buttons), request);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("usb-eject")]
    [InlineData("USB-UNPLUG")]
    [InlineData("usb-unplug 1 2")]
    [InlineData("usb-unplug -1")]
    [InlineData("usb-unplug +1")]
    [InlineData("usb-unplug x")]
    [InlineData("usb-device-unplug")]
    [InlineData("usb-device-plug 1 2")]
    [InlineData("usb-device-plug -1")]
    [InlineData("usb-device-plug 99999999999")]
    [InlineData("usb-pointer-move 0 5")]
    [InlineData("usb-pointer-move 0 5 3 1 9")]
    [InlineData("usb-pointer-move 0 five 3")]
    [InlineData("usb-pointer-move 0 5 3.5")]
    [InlineData("usb-pointer-move 0 99999999999 0")]
    [InlineData("usb-pointer-move 0 5 3 8")]
    [InlineData("usb-pointer-move 0 5 3 -1")]
    [InlineData("usb-pointer-move -1 5 3")]
    public void Parse_RefusesMalformedRequests(string text)
    {
        Assert.Throws<FormatException>(() => HostRequest.Parse(text));
    }
}
