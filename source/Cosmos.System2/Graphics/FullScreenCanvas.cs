//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Cosmos.HAL;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// FullScreenCanvas class. Used to set and get full screen canvas.
    /// </summary>
    public static class FullScreenCanvas
    {

        public readonly static ICanvas FallBackDriver;
        public readonly static Mode FakeDefaultMode = new Mode(-1, -1, ColorDepth.ColorDepth4);

        static FullScreenCanvas()
        {
            FallBackDriver = new VGACanvas();
            AddDriver(new VBECanvas());
            AddDriver(new VGACanvas());
            AddDriver(new SVGAIICanvas());
            AddDriver(FallBackDriver);
        }

        /// <summary>
        /// Boolean value whether CGS is in use or not
        /// </summary>
        public static bool IsInUse { get; private set; } = false;

        /// <summary>
        /// Disables the specified Graphics Driver used and returns to VGA text mode 80x25
        /// </summary>
        public static void Disable(VGADriver.TextSize textSize = VGADriver.TextSize.Size80x25)
        {
            if (IsInUse)
            {
                _VideoDriver.Disable();
                VGAScreen.SetTextMode(textSize);
                IsInUse = false;
            }
        }

        /// <summary>
        /// Video driver.
        /// </summary>
        private static ICanvas _VideoDriver = null;

        static List<ICanvas> _Drivers = new List<ICanvas>();
        public static List<ICanvas> Drivers { get {
            return _Drivers;
        }}

        public static List<ICanvas> SupportedDrivers { get {
            List<ICanvas> _SupportedDrivers = new List<ICanvas>();
            foreach (var driver in Drivers)
            {
                if(driver.IsSupported()) _SupportedDrivers.Add(driver);
            }

            return _SupportedDrivers;
        }}

        public static void AddDriver(ICanvas canvas)
        {
            if(!_Drivers.Contains(canvas))
            {
                _Drivers.Add(canvas);
            }
        }

        public static void RemoveDriver(ICanvas canvas)
        {
            if(!_Drivers.Contains(canvas))
            {
                _Drivers.Remove(canvas);
            }
        }

        /// <summary>
        /// Get video driver.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        private static ICanvas GetVideoDriver(Mode mode)
        {
            var useFallbackDriver = false;
            foreach (var driver in SupportedDrivers)
            {
                var targetMode = mode;
                if(mode == FakeDefaultMode)
                {
                    if(driver == FallBackDriver) 
                    {
                        useFallbackDriver = true;
                    }
                    return driver;
                }
                foreach (var m in driver.AvailableModes)
                {
                    if (m == mode)
                    {
                        if(driver == FallBackDriver) 
                        {
                            useFallbackDriver = true;
                            continue;
                        }
                        return driver;
                    }
                }
            }
            if(useFallbackDriver)
            {
                return FallBackDriver;
            }
            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by any Drivers");
        
        }

        /// <summary>
        /// Get full screen canvas.
        /// Changes current Mode to default.
        /// </summary>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if default graphics mode is not suppoted.</exception>
        public static ICanvas GetFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with default mode");

            _VideoDriver = GetVideoDriver(FakeDefaultMode);
            _VideoDriver.Init(_VideoDriver.DefaultGraphicMode);
            IsInUse = true;
            return _VideoDriver;
        }

        /// <summary>
        /// Get full screen canvas.
        /// Changes the current Mode.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>Canvas value.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        public static ICanvas GetFullScreenCanvas(Mode mode)
        {
            Global.mDebugger.SendInternal($"GetFullScreenCanvas() with mode" + mode);


            _VideoDriver = GetVideoDriver(mode);

            _VideoDriver.Init(mode);
            
            IsInUse = true;
            return _VideoDriver;
        }

        /// <summary>
        /// Trys to get full screen canvas.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <returns>true if successfully; otherwise, false.</returns>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown if graphics mode is not suppoted.</exception>
        public static bool TryGetFullScreenCanvas(Mode mode, out ICanvas canvas)
        {
            Global.mDebugger.SendInternal($"TryGetFullScreenCanvas() with mode" + mode);

            try
            {
                canvas = GetFullScreenCanvas(mode);
                IsInUse = true;
                return true;
            }
            catch
            {
            }
            canvas = null;
            return false;
        }

        public static ICanvas SetFullScreenCanvasDriver(ICanvas canvas, Mode mode)
        {

            Disable();
            
            _VideoDriver = canvas;
            _VideoDriver.Init(mode);

            return _VideoDriver;
        }


        /// <summary>
        /// Gets current full screen canvas
        /// with out setting Mode.
        /// </summary>
        /// <returns>Canvas value.</returns>
        public static ICanvas GetCurrentFullScreenCanvas()
        {
            Global.mDebugger.SendInternal($"GetCurrentFullScreenCanvas()");

            return _VideoDriver;
        }

    }
}
