// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.System.Network;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>Ring-defined facet a display implements when it can render 3D: <see cref="Canvas.GetFullScreen()"/> asks the primary display for it.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface ICanvas3DFactory
{
    /// <summary>Creates the 3D canvas over <paramref name="display"/>, which is the display this facet belongs to. Thread context.</summary>
    /// <param name="display">The display the facet was found on.</param>
    /// <returns>The 3D canvas, which draws on <paramref name="display"/>.</returns>
    Canvas3D CreateCanvas3D(DisplayDevice display);
}
