using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging {
        /// <summary>
        /// Represents a kernel stage.
        /// </summary>
        public abstract class StageBase {
                /// <summary>
                /// Gets the name of the stage.
                /// </summary>
                public abstract string Name {
                        get;
                }

                /// <summary>
                /// Initializes the stage.
                /// </summary>
                public abstract void Initialize();

                /// <summary>
                /// Tears the stage down.
                /// </summary>
                public abstract void Teardown();
        }
}
