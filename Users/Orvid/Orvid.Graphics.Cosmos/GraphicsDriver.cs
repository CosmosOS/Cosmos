using System;
using System.Collections.Generic;

namespace Orvid.Graphics
{
    /// <summary>
    /// This class describes a graphics driver.
    /// </summary>
    public abstract class GraphicsDriver
    {
        /// <summary>
        /// Returns the name of the graphics driver.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Returns the version of the graphics driver.
        /// </summary>
        public abstract string Version { get; }
        /// <summary>
        /// Returns the company that created the graphics driver.
        /// </summary>
        public abstract string Company { get; }
        /// <summary>
        /// Returns the author of the graphics driver.
        /// </summary>
        public abstract string Author { get; }
        /// <summary>
        /// Returns the current graphics mode.
        /// </summary>
        public abstract GraphicsMode Mode { get; }

        /// <summary>
        /// This method is called to update the entire screen.
        /// </summary>
        /// <param name="i">The image to draw.</param>
        public abstract void Update(Image i);

        /// <summary>
        /// This method is used to get the resolutions the driver supports.
        /// </summary>
        /// <returns>A list of graphics modes that the driver supports.</returns>
        public abstract List<GraphicsMode> GetSupportedModes();

        /// <summary>
        /// This method is used to set the current graphics mode.
        /// </summary>
        /// <param name="mode">The mode to set to.</param>
        public abstract void SetMode(GraphicsMode mode);

        /// <summary>
        /// Returns true if the given graphics mode is supported.
        /// </summary>
        /// <param name="mode">The mode to check.</param>
        /// <returns>True if the given mode is supported.</returns>
        public bool SupportsMode(GraphicsMode mode)
        {
            return GetSupportedModes().Contains(mode);
        }

        /// <summary>
        /// This method is used to determine whether or not to load the driver.
        /// </summary>
        /// <returns>True if the driver is valid for the current system, otherwise false.</returns>
        public abstract bool Supported();

        /// <summary>
        /// This method is called to initialize the graphics
        /// driver and tell it we're going to use it.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// This method is called to tell the driver to shut-down.
        /// </summary>
        public abstract void Shutdown();
    }
}
