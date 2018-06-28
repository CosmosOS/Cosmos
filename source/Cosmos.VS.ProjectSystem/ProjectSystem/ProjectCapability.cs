using static Microsoft.VisualStudio.ProjectSystem.ProjectCapabilities;

namespace Cosmos.VS.ProjectSystem
{
    internal class ProjectCapability
    {
        #region CPS Capabilities

        /// <summary>
        /// Indicates that the project uses the app designer for managing project properties.
        /// </summary>
        public const string AppDesigner = nameof(AppDesigner);

        #endregion

        #region New Capabilities

        /// <summary>
        /// Bootable project.
        /// </summary>
        public const string Bootable = nameof(Bootable);

        /// <summary>
        /// Cosmos project.
        /// </summary>
        public const string Cosmos = nameof(Cosmos);

        #endregion

        #region Combined Capabilities

        public const string CosmosAndAppDesigner = Cosmos + " & " + AppDesigner;

        #endregion
    }
}
