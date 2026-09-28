// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// Decides which drivers a node is offered to, and in what order: every
/// registered driver with a matching entry, by priority (highest first),
/// then by the specificity of its best match (highest first), then by
/// manifest position (earliest first). Each candidate gets a fresh binding;
/// the first to return bound keeps the node. Worker only.
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
                return;
            }

            int released = binding.Unwind();
            node.AddOffer(new DeviceOffer(driver.Name, driver.Priority, candidates[i].Specificity, result.Outcome, result.Reason, released));
            DriverLog.Offer(node, driver, result);
        }

        node.State = NodeState.Unbound;
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
            text = string.Concat(text, " ", candidates[i].Driver.Name, "(prio ", candidates[i].Driver.Priority.ToString(), ", spec ", candidates[i].Specificity.ToString(), ")");
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
