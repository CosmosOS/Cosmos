// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The network stack's lock. Two kinds of threads run the stack: the kit
/// worker, which hands it every received frame through
/// <see cref="NetworkStack.HandlePacket"/>, and the threads that use it (the
/// protocol clients, the socket plugs). Each takes this lock around what it
/// reads or changes in the state they share: the connection table and the
/// connections' receive buffers, the clients' tables and queues, the outgoing
/// queues, the address tables and the neighbor caches.
/// <para>
/// A blocking lock, not an interrupt mask or a spinlock. The stack allocates
/// all the way down, and the runtime locks it reaches (the class constructor
/// runner, the shared array pool, any lock a protocol callback takes) block
/// their thread when contended. A thread that blocks with interrupts masked
/// is never switched away from, so the receive path ran masked until it froze
/// the kernel on such a lock (#3387). With this lock a contended runtime lock
/// is an ordinary wait.
/// </para>
/// <para>
/// Recursive: the receive path answers through the same entry points the
/// other threads use (it queues and sends replies). Never held across a wait:
/// what the stack waits for (a datagram in a client's queue, the peer's
/// acknowledgment, an ARP reply) comes from the receive path, which needs the
/// lock to make progress. Thread context only. Before the scheduler runs the
/// lock is a no-op, as <see cref="SchedMutex"/> is.
/// </para>
/// </summary>
internal static class NetworkStackLock
{
    private static readonly SchedMutex s_mutex = new();

    /// <summary>Takes the lock, blocking while another thread holds it, until the returned scope is disposed.</summary>
    /// <returns>The scope that releases the lock once.</returns>
    public static Scope Enter()
    {
        s_mutex.Acquire();
        return new Scope(true);
    }

    /// <summary>One hold of the stack's lock; disposing it releases that hold.</summary>
    public ref struct Scope
    {
        private bool _held;

        internal Scope(bool held)
        {
            _held = held;
        }

        /// <summary>Releases the hold. A second call does nothing.</summary>
        public void Dispose()
        {
            if (_held)
            {
                _held = false;
                s_mutex.Release();
            }
        }
    }
}
