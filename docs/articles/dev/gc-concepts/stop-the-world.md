# Stop-the-world

A stop-the-world collector pauses every mutator (every thread running application code) for the full duration of a collection. Nothing allocates or writes references while the collector runs, so the object graph cannot change under it, and the collector needs no synchronization with running code.

The alternative family, concurrent and incremental collectors, lets threads keep running during parts of the collection. That removes the pause but demands heavy machinery: write barriers that tell the collector about references mutated mid-scan, and careful ordering so that a live object is never freed.

In OrionGC every collection is stop-the-world. It runs with interrupts masked on the CPU that triggered it and every other CPU held in the stop rendezvous, each answering from the stop interrupt or from a safepoint inside a masked wait, so no thread switch, interrupt handler or other CPU runs until it finishes. See [Collection](../garbage-collector.md#collection) and [Stopping the world](../garbage-collector.md#stopping-the-world).
