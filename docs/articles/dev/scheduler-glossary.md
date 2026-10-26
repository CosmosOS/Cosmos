# Scheduler glossary

These notes give the background on the scheduling concepts that the contributor article on the [Scheduler](scheduler.md) and the guide to [Writing a scheduler](../user/scheduler-plugging.md) build on, for a reader who has not written a scheduler before. Each page explains the concept on its own, then closes with how Cosmos applies it. The table groups them in three parts: how a thread gets the CPU and loses it, what code that runs against the tick has to respect, then the properties a policy is judged by. The rules for hook code also build on a note from the driver glossary, [interrupt context](../user/driver-concepts/interrupt-context.md).

| Concept | Summary |
|---------|---------|
| [Policy and mechanism](sched-concepts/policy-and-mechanism.md) | Deciding which thread runs, kept apart from the code that switches threads |
| [Thread states and the run queue](sched-concepts/run-queue.md) | Running, ready and blocked threads, the structure ready threads wait in, and the idle thread |
| [Preemption](sched-concepts/preemption.md) | The kernel takes the CPU away on an interrupt; threads never have to volunteer |
| [The tick and the quantum](sched-concepts/scheduler-tick.md) | The timer interrupt that gives the scheduler control, the time slice, and accounting by sample |
| [Context switch](sched-concepts/context-switch.md) | Saving one thread's registers and restoring another's, and when the switch actually happens |
| [Interrupts at instruction boundaries](sched-concepts/instruction-boundary.md) | An interrupt lands after one instruction and before the next; nothing longer is atomic against it |
| [Spinlocks and interrupt masking](sched-concepts/spinlocks-and-masking.md) | Why a lock does not keep out an interrupt handler on the same CPU, and what does |
| [Virtual-time fair-share](sched-concepts/virtual-time-fair-share.md) | Weighted CPU shares, enforced by always running the thread whose virtual clock is furthest behind |
| [Starvation and aging](sched-concepts/starvation.md) | Ready threads that never run, and the guards against it |
| [Priority inversion and inheritance](sched-concepts/priority-inversion.md) | A high-priority thread stuck behind a lock holder that a medium-priority thread preempts |
| [Real-time scheduling](sched-concepts/real-time-scheduling.md) | Deadlines, Rate Monotonic and EDF, utilization bounds and admission control |
| [CPU affinity and load balancing](sched-concepts/cpu-affinity.md) | Per-CPU run queues, placement, migration and pinned threads |
