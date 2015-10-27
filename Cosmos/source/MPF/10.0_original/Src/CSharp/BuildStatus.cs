/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/



namespace Microsoft.VisualStudio.Project
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// Provides information about the current build status.
    /// </summary>
    internal static class BuildStatus
    {
        private static BuildKind? currentBuild;

        private static volatile object Mutex = new object();

        /// <summary>
        /// Gets a value whether a build is in progress.
        /// </summary>
        internal static bool IsInProgress
        {
            get { return BuildStatus.currentBuild.HasValue; }
        }

        /// <summary>
        /// Called when a build has started
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static bool StartBuild(BuildKind kind)
        {
            if (!BuildStatus.currentBuild.HasValue)
            {
                lock(BuildStatus.Mutex)
                {
                    BuildStatus.currentBuild = kind;
                }

                return true;
            }

            BuildKind currentBuildKind = BuildStatus.currentBuild.Value;

            switch (currentBuildKind)
            {
                case BuildKind.Sync:
                    // Attempt to start a build during sync build indicate reentrancy
                    Debug.Fail("Message pumping during sync build");
                    return false;

                case BuildKind.Async:
                    if (kind == BuildKind.Sync)
                    {
                        // if we need to do a sync build during async build, there is not much we can do:
                        // - the async build is user-invoked build
                        // - during that build UI thread is by design not blocked and messages are being pumped
                        // - therefore it is legitimate for other code to call Project System APIs and query for stuff
                        // In that case we just fail gracefully
                        return false;
                    }
                    else
                    {
                        // Somebody attempted to start a build while build is in progress, perhaps and Addin via
                        // the API. Inform them of an error in their ways.
                        throw new InvalidOperationException("Build is already in progress");
                    }
                default:
                    Debug.Fail("Unreachable");
                    return false;

            }
        }

        /// <summary>
        /// Called when a build is ended.
        /// </summary>
        internal static void EndBuild()
        {
            Debug.Assert(IsInProgress, "Attempt to end a build that is not started");
            lock (Mutex)
            {
                BuildStatus.currentBuild = null;
            }
        }        
    }
}