using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.HAL.Drivers.Video;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VGACanvas class. Used to control screen.
    /// </summary>
    public class VGACanvas : Canvas
    {
        /// <summary>
        /// Private boolean whether VGA graphics mode is enabled or not
        /// </summary>
        bool _Enabled;

        /// <summary>
        /// The HAL VGA driver
        /// </summary>
        private readonly VGADriver _VGADriver;

        /// <summary>
        /// VGA graphics mode Canvas constructor - see Canvas.cs
        /// </summary>
        /// <param name="aMode"></param>
        public VGACanvas(Mode aMode) : base()
        {
            Global.mDebugger.Send("Creating VGACanvas with mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(aMode), (VGADriver.ColorDepth)(int)aMode.ColorDepth);
            Mode = aMode;
            Enabled = true;
        }

        /// <summary>
        /// Creates a VGA graphics mode with the default mode
        /// </summary>
        public VGACanvas() : base()
        {
            Enabled = true;
            Mode = GetDefaultMode();
            Global.mDebugger.Send("Creating VGACanvas with standard mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(GetDefaultMode()), (VGADriver.ColorDepth)(int)DefaultGraphicMode.ColorDepth);
        }

        /// <summary>
        /// Name of the backend
        /// </summary>
        public override string GetName()
        {
            return "VGACanvas";
        }

        /// <summary>
        /// List of available resolutions
        /// </summary>
        private static readonly List<Mode> _AvailableModes = new()
        {
            new Mode(640, 480, ColorDepth.ColorDepth4),
            new Mode(720, 480, ColorDepth.ColorDepth4),
            new Mode(320, 200, ColorDepth.ColorDepth8)
        };

        public override List<Mode> AvailableModes
        {
            get
            {
                return _AvailableModes;
            }
        }

        /// <summary>
        /// The default graphics mode
        /// </summary>
        public override Mode GetDefaultMode()
        {
            return new(640, 480, ColorDepth.ColorDepth4);
        }

        /// <summary>
        /// Boolean value whether VGA is in graphics mode or not
        /// </summary>
        public bool Enabled { get => _Enabled; private set => _Enabled = value; }

        private ScreenSize ModeToScreenSize(Mode aMode)
        {
            if (aMode.Columns == 320 && aMode.Rows == 200)
            {
                return ScreenSize.Size320x200;
            }
            else if (aMode.Columns == 640 && aMode.Rows == 480)
            {
                return ScreenSize.Size640x480;
            }
            else if (aMode.Columns == 720 && aMode.Rows == 480)
            {
                return ScreenSize.Size720x480;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Display()
        {

        }
    }
}
