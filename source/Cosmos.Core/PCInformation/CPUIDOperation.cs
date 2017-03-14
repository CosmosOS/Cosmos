namespace Cosmos.Core.PCInformation
{
    public enum CPUIDOperation
    {
        /// <summary>
        /// Calls cpuid with eax = 0 (to get vendor id)
        /// </summary>
        /// <remarks>
        /// Returns <c>ebx</c>, <c>ecx</c> and <c>edx</c> containing the vendor id
        /// </remarks>
        GetVendorID,
        /// <summary>
        /// Returns general information related with the processor with cpuid <c>eax</c> = 1
        /// </summary>
        /// <remarks>Returns the first 32 bits of eax (processor signature) and additional information.</remarks>
        GetProcessorInformation,
        /// <summary>
        /// Returns the flags and extended flags of the processor with cpuid <c>eax</c> = 1
        /// </summary>
        /// <remarks>Returns edx and ecx registers</remarks>
        GetFlags,
        /// <summary>
        /// Includes tlb information
        /// </summary>
        GetCacheInformation,
        GetTopology,
        GetExtendedFeatures,
        /// <summary>
        /// Returns the processor brand. Consists in 3 calls to cpuid.
        /// </summary>
        GetProcessorBrand
    }
}