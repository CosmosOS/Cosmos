using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Demo.Pacman.ConsoleViewers
{
    class GhostViewer
    {
        Cosmos.Demo.Pacman.Elements.Ghost ghost;
        ConsoleColor ghostColor;
        ConsoleColor weakColor;
        public GhostViewer(Cosmos.Demo.Pacman.Elements.Ghost ghost, ConsoleColor ghostColor, ConsoleColor weakColor)
        {
            this.ghost = ghost;
            this.ghostColor = ghostColor;
            this.weakColor = weakColor;
        }

        #region accessors

        #endregion
    }
}
