// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// Decides which drivers a node is offered to, and in what order: every
/// registered driver with a matching entry, by priority (highest first),
/// then by the specificity of its best match (highest first), then by
/// manifest position (earliest first). Each candidate gets a fresh binding;
/// the first to return bound keeps the node. A bus whose access object
/// implements <see cref="INodeHooks"/> quiesces the hardware before the
/// first probe, quiets it again after each probe that did not bind (before
/// the probe's memory is freed) and restores it when nobody binds; with no
/// candidate the hooks are not called. Once a driver binds a PCI function,
/// the firmware display inside one of its memory windows is retired. Worker
/// only.
/// </summary>
internal static class Arbitration
{
    /// <summary>Offers <paramref name="node"/> until a driver binds it or the candidates run out.</summary>
    /// <param name="node">A pending node.</param>
    internal static void Offer(DeviceNode node)
    {
        if (node.State != NodeState.Pending)
        {
            // Retracted before the worker reached it.
            return;
        }

        Candidate[] candidates = Collect(node);
        if (candidates.Length == 0)
        {
            node.State = NodeState.Unbound;
            DriverLog.NoDriver(node);
            return;
        }

        if (!Quiesce(node))
        {
            return;
        }

        DriverLog.Candidates(node, Describe(candidates));
        for (int i = 0; i < candidates.Length; i++)
        {
            Driver driver = candidates[i].Driver;
            DeviceBinding binding = new(node, driver);
            ProbeResult result;
            try
            {
                result = driver.Probe(binding);
            }
            catch (Exception exception)
            {
                result = ProbeResult.Failed(exception.Message);
            }

            if (result.Outcome == ProbeOutcome.Bound)
            {
                node.Binding = binding;
                node.State = NodeState.Bound;
                node.AddOffer(new DeviceOffer(driver.Name, driver.Priority, candidates[i].Specificity, ProbeOutcome.Bound, null, 0));
                DriverLog.Offer(node, driver, result);
                if (CosmosFeatures.GraphicsEnabled)
                {
                    DeviceRegistry.RetireFirmwareDisplaysBehind(node);
                }

                return;
            }

            Quiet(node);
            int released = binding.Unwind();
            node.AddOffer(new DeviceOffer(driver.Name, driver.Priority, candidates[i].Specificity, result.Outcome, result.Reason, released));
            DriverLog.Offer(node, driver, result);
        }

        node.State = NodeState.Unbound;
        Restore(node);
    }

    /// <summary>
    /// Runs the bus's <see cref="INodeHooks.BeforeFirstOffer"/>, when the
    /// node has one. A hook that throws leaves the node unbound with no
    /// offer: a driver must not see a function the bus could not quiet.
    /// </summary>
    /// <returns>False when the node was skipped.</returns>
    private static bool Quiesce(DeviceNode node)
    {
        if (node.AccessObject is not INodeHooks hooks)
        {
            return true;
        }

        try
        {
            hooks.BeforeFirstOffer();
            return true;
        }
        catch (Exception exception)
        {
            DriverLog.HookThrew(node, "quiesce", exception.Message);
            node.State = NodeState.Unbound;
            DriverLog.NodeSkipped(node, exception.Message);
            return false;
        }
    }

    /// <summary>
    /// Runs the bus's <see cref="INodeHooks.AfterOfferDeclined"/>, when the
    /// node has one, before the binding of a probe that did not bind is
    /// unwound: whatever the probe armed stops before its memory is freed.
    /// </summary>
    private static void Quiet(DeviceNode node)
    {
        if (node.AccessObject is not INodeHooks hooks)
        {
            return;
        }

        try
        {
            hooks.AfterOfferDeclined();
        }
        catch (Exception exception)
        {
            DriverLog.HookThrew(node, "quiet", exception.Message);
        }
    }

    /// <summary>
    /// Runs the bus's <see cref="INodeHooks.AfterUnbound"/>, when the node
    /// has one, so a function nobody bound is left as firmware left it.
    /// </summary>
    private static void Restore(DeviceNode node)
    {
        if (node.AccessObject is not INodeHooks hooks)
        {
            return;
        }

        try
        {
            hooks.AfterUnbound();
        }
        catch (Exception exception)
        {
            DriverLog.HookThrew(node, "restore", exception.Message);
        }
    }

    private static Candidate[] Collect(DeviceNode node)
    {
        IReadOnlyList<Driver> drivers = DriverRegistry.Drivers;
        List<Candidate> candidates = new();
        for (int position = 0; position < drivers.Count; position++)
        {
            Driver driver = drivers[position];
            int specificity = -1;
            ReadOnlySpan<DeviceMatch> matches = driver.Matches;
            for (int m = 0; m < matches.Length; m++)
            {
                if (matches[m].Matches(node.Identity) && matches[m].Specificity > specificity)
                {
                    specificity = matches[m].Specificity;
                }
            }

            if (specificity >= 0)
            {
                Insert(candidates, new Candidate(driver, position, specificity));
            }
        }

        return candidates.ToArray();
    }

    /// <summary>Insertion in arbitration order; the lists are short.</summary>
    private static void Insert(List<Candidate> candidates, Candidate candidate)
    {
        int index = 0;
        while (index < candidates.Count && !candidate.Precedes(candidates[index]))
        {
            index++;
        }

        candidates.Insert(index, candidate);
    }

    private static string Describe(Candidate[] candidates)
    {
        string text = string.Empty;
        for (int i = 0; i < candidates.Length; i++)
        {
            text = $"{text} {candidates[i].Driver.Name}(prio {candidates[i].Driver.Priority}, spec {candidates[i].Specificity})";
        }

        return text;
    }

    private readonly struct Candidate
    {
        internal Candidate(Driver driver, int position, int specificity)
        {
            Driver = driver;
            Position = position;
            Specificity = specificity;
        }

        internal Driver Driver { get; }

        internal int Position { get; }

        internal int Specificity { get; }

        internal bool Precedes(Candidate other)
        {
            if (Driver.Priority != other.Driver.Priority)
            {
                return Driver.Priority > other.Driver.Priority;
            }

            if (Specificity != other.Specificity)
            {
                return Specificity > other.Specificity;
            }

            return Position < other.Position;
        }
    }
}
