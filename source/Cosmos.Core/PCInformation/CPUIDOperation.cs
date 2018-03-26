namespace Cosmos.Core.PCInformation
{
    public enum CPUIDOperation : uint
    {
        /// <summary>
        /// Calls cpuid with eax = 0 (to get vendor id)
        /// </summary>
        /// <remarks>
        /// Returns <c>ebx</c>, <c>ecx</c> and <c>edx</c> containing the vendor id
        /// </remarks>
        GetVendorID = 0,
        /// <summary>
        /// Returns general information related with the processor with cpuid <c>eax</c> = 1
        /// </summary>
        /// <remarks>Returns the first 32 bits of eax (processor signature), edx and ecx and additional information.</remarks>
        GetProcessorInformation = 1,
        /// <summary>
        /// Includes tlb information
        /// </summary>
        GetCacheInformation = 2,
        //GetTopology,
        GetExtendedFeatures = 0x80000001,
        /// <summary>
        /// Get the highest extended function supported
        /// </summary>
        /// <remarks>Return the parameter in eax</remarks>
        GetHighestExtendedFunctionSupported = 0x80000000,
        /// <summary>
        /// Returns the processor brand. Consists in 3 calls to cpuid.
        /// </summary>
        GetProcessorBrand = 0x80000002
    }
}