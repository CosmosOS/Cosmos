using System;
using System.Collections.Generic;

namespace Cosmos.BuildEngine
{
    /// <summary>
    /// A target onto which a "builded" version of Cosmos can be deployed.
    /// </summary>
    public abstract class CosmosBuildTarget
    {
        /// <summary>
        /// The result of a target on which the build engine may act.
        /// </summary>
        public class TargetResult
        {
            /// <summary>
            /// The type of action that may be taken on a result.
            /// </summary>
            public enum TargetResultType : byte
            {
                /// <summary>
                /// Target deploy failed. Error message provided as result.
                /// </summary>
                Failed = 0,
                /// <summary>
                /// A data file was created. Path (relative to build directory) provided as result.
                /// </summary>
                File_Data = 1,
                /// <summary>
                /// A file was created (or exists) which should be shell-executed. Path (relative to build directory) provided as result.
                /// </summary>
                File_Execute = 2,
                /// <summary>
                /// A command should be shell-executed. Command provided as result. All paths should be relative to build directory.
                /// </summary>
                String_Execute = 4
            }

            /// <summary>
            /// See TargetResultType.
            /// </summary>
            public readonly TargetResultType ResultType;
            /// <summary>
            /// The result of the Target. See TargetResultType.
            /// </summary>
            public readonly String Result;

            /// <summary>
            /// Primary Constructor.
            /// </summary>
            /// <param name="resulttype">See TargetResultType.</param>
            /// <param name="result">See TargetResultType.</param>
            public TargetResult(TargetResultType resulttype, String result)
            {
                ResultType = resulttype;
                Result = result;
            }
        }

        /// <summary>
        /// Deploys the target.
        /// </summary>
        /// <param name="BuiltBinaryPath">Path (relative to build directory) of the built Cosmos binary.</param>
        /// <returns>See TargetResult.</returns>
        public abstract TargetResult DeployTarget(String BuiltBinaryPath, IEnumerable<BuildOption> Options);
        public abstract IEnumerable<BuildOption> GetBuildOptions();
        public abstract String GetDisplayName();
    }
}