// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// What a bus does around a node's life when its access object implements
/// this: quiesce the hardware before the first driver sees it, quiet it
/// again after each probe that did not bind, put it back as firmware left
/// it when nobody binds, and quiet it once more after a binding is torn
/// down. Implemented explicitly by the access object, so a driver holding
/// it cannot call the hooks. The engine calls each one inside a try/catch
/// and logs an exception, so a hook that throws never stops the worker.
/// Worker only.
/// </summary>
internal interface INodeHooks
{
    /// <summary>
    /// Runs once, before the first probe, when at least one driver is a
    /// candidate. An exception here leaves the node unbound without an
    /// offer: a driver must not see a function the bus could not quiet.
    /// </summary>
    void BeforeFirstOffer();

    /// <summary>
    /// Runs after a probe declined, failed or threw, before the binding is
    /// unwound: a function the probe armed (bus mastering on, a ring that
    /// points at DMA buffers) is quiet before those buffers are freed, and
    /// the next candidate sees the same quiet function the first one did.
    /// </summary>
    void AfterOfferDeclined();

    /// <summary>
    /// Runs after every candidate declined or failed: the hardware goes back
    /// to what firmware left, so a function a legacy driver still operates
    /// keeps its bus mastering and its message interrupts.
    /// </summary>
    void AfterUnbound();

    /// <summary>
    /// Runs after a binding's teardown, before the bus's own allocation is
    /// released, whether or not resources leaked; a node nobody bound is
    /// not quieted when it is retracted, since a legacy driver may operate
    /// its function. With <paramref name="hardwarePresent"/> false the
    /// hardware is gone and nothing is written.
    /// </summary>
    /// <param name="hardwarePresent">Whether the hardware is still there to be quiesced.</param>
    void AfterTeardown(bool hardwarePresent);
}
