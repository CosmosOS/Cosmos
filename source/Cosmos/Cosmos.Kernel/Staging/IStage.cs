using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel.Staging {
        /// <summary>
        /// Represents a kernel stage.
        /// </summary>
        public interface IStage {
                /// <summary>
                /// Gets the name of the stage.
                /// </summary>
                string Name {
                        get;
                }

                /// <summary>
                /// Initializes the stage.
                /// </summary>
                void Initialize();

                /// <summary>
                /// Tears the stage down.
                /// </summary>
                void Teardown();
        }
}
