namespace Cosmos.Kernel.Core.Memory;

/// <summary>
/// panics used be the Memory sub system
/// </summary>
internal static class Panics
{
    /// <summary>
    /// this page is not managed by a heap backend
    /// </summary>
    public const uint NonManagedPage = 1100;

    public class SmallHeap
    {
        /// <summary>
        /// you need to add root mst blocks from smallest to largest
        /// </summary>
        public const uint AddRootSmtBlockOrder = 1200;

        /// <summary>
        /// failed to add a page
        /// </summary>
        public const uint AddPage = 1201;

        /// <summary>
        /// RAM is corrupted, since we know we had a space but it turns out we didnt
        /// </summary>
        public const uint RamCorrupted = 1202;

        /// <summary>
        /// double free
        /// </summary>
        public const uint DoubleFree = 1203;

        /// <summary>
        /// failed to free
        /// </summary>
        public const uint FailedFree = 1204;
    }
}
