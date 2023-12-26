using IL2CPU.API.Attribs;
using Cosmos.System;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Environment))]
    public class EnvironmentImpl
    {
        #region Properties

        // We still need to add support for ".." and "." here, best method when ".." is detected is just remove last directory from current instead of adding the literal "..".
        public static string CurrentDirectory
        {
            get => Directory.GetCurrentDirectory();
            set => Directory.SetCurrentDirectory(value);
        }

        #endregion

        #region Methods

        public static void Exit(int exitCode)
        {
            Power.Shutdown();
        }

        #endregion
    }
}